using System.Data.Common;
using System.Globalization;
using System.Text.Json;
using BotSaaS.Api.Core.Scheduling;
using BotSaaS.Api.Shared.AI;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Conversations;

// Conversation flow: find-or-create the customer's thread, save their message, load history, ask the AI, save the reply (or run the tool call).
public class ConversationService : IConversationService
{
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IAiClient _aiClient;
    private readonly ISchedulingService _schedulingService;
    public ConversationService(IDatabase databaseConnection, DbSession dbSession, IConversationRepository conversationRepository, IMessageRepository messageRepository, IAiClient aiClient, ISchedulingService schedulingService)
    {
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _aiClient = aiClient;
        _schedulingService = schedulingService;
    }
    public async Task<Result<string>> ProcessMessage(Guid companyId, string customerPhone, string messageText)
    {
        // survive across the two connection scopes (LLM runs between them)
        Guid conversationId;
        List<ChatMessage> chatHistory;

        // 1) writes before the LLM - open, write, close
        try
        {
            using DbConnection connection = _databaseConnection.CreateConnection();
            _dbSession.Connection = connection;
            await connection.OpenAsync();

            Conversation? conversation = await _conversationRepository.GetConversationByCompanyAndPhone(companyId, customerPhone);

            if(conversation == null)
            {
                conversation = new Conversation
                {
                    Id = Guid.NewGuid(),
                    CompanyId = companyId,
                    CustomerPhone = customerPhone,
                    CreatedAt = DateTime.UtcNow
                };
                await _conversationRepository.InsertConversation(conversation);
            }
            conversationId = conversation.Id;

            Message userMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = ChatRole.User,
                Content = messageText,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.InsertMessage(userMessage);

            List<Message> messages = await _messageRepository.GetMessagesByConversation(conversationId);
            chatHistory = new List<ChatMessage>();

            foreach(Message findMessage in messages)
            {
                ChatMessage chatMessage = new ChatMessage(findMessage.Role, findMessage.Content);
                chatHistory.Add(chatMessage);
            }
        }
        catch (DbException)
        {
            return Result<string>.Failure("Erro ao inserir conversa");
        }

        // 2) LLM - no DB connection open here
        AiResponse aiResponse;
        try
        {
            string baseSystemPrompt = Environment.GetEnvironmentVariable("SYSTEM_PROMPT")
                ?? throw new InvalidOperationException("System prompt não definido");

            // Inject today's date (business-local, not UTC) so the model can resolve relative dates like "amanhã".
            DateTime now = DateTime.Now;
            CultureInfo ptBr = new CultureInfo("pt-BR");
            string dateContext = $"Hoje é {now.ToString("dddd, dd 'de' MMMM 'de' yyyy", ptBr)} ({now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}). " +
                "Use esta data para resolver referências como 'hoje', 'amanhã', 'sexta que vem'. A data de um agendamento deve estar no formato AAAA-MM-DD.";
            string systemPrompt = $"{dateContext}\n\n{baseSystemPrompt}";

            // Tools the model may call. Flat for now; varies per segment/niche later.
            // TODO: move tool ownership (definition + args + handler) into the Scheduling module. Conversations should just route tool calls, not know appointment-shaped data.
            List<ToolDefinition> tools = new List<ToolDefinition>
            {
                new ToolDefinition(
                    "registrar_agendamento",
                    "Registra um agendamento. Só chame quando tiver serviço, nome, a DATA e o HORÁRIO EXATO. Se o cliente der algo vago como 'de manhã' ou 'à tarde', pergunte o horário específico ANTES de chamar.",
                    new List<ToolParameter>
                    {
                        new ToolParameter("servico", "string", "O serviço desejado, ex: corte de cabelo", true),
                        new ToolParameter("data", "string", "A data no formato AAAA-MM-DD", true),
                        new ToolParameter("hora", "string", "O horário EXATO no formato HH:MM 24h, ex: 09:00. Nunca use termos vagos como 'de manhã'.", true),
                        new ToolParameter("nome", "string", "O nome do cliente", true)
                    }
                )
            };

            aiResponse = await _aiClient.GenerateReplyAsync(systemPrompt, chatHistory, tools);
        }
        catch (AiClientException)
        {
            return Result<string>.Failure("Erro ao gerar resposta da IA");
        }

        // 3) resolve the reply (text or tool call) + write it -- one connection scope
        string response;
        try
        {
            using DbConnection connection = _databaseConnection.CreateConnection();
            _dbSession.Connection = connection;
            await connection.OpenAsync();

            if (aiResponse is TextReply text)
            {
                response = text.Text;
            }
            else if (aiResponse is ToolCallReply toolCall)
            {
                response = await HandleToolCall(companyId, conversationId, toolCall);
            }
            else
            {
                throw new InvalidOperationException();
            }

            Message assistantMessage = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Role = ChatRole.Assistant,
                Content = response,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepository.InsertMessage(assistantMessage);
        }
        catch (DbException)
        {
            return Result<string>.Failure("Erro ao salvar resposta");
        }

        return Result<string>.Success(response);
    }

    // Runs the tool call: parse args -> create the appointment -> return a confirmation (or a clarification if it failed).
    private async Task<string> HandleToolCall(Guid companyId, Guid conversationId, ToolCallReply toolCall)
    {
        // only tool declared today. Web options = case-insensitive (JSON "servico" -> C# "Servico").
        AgendamentoArgs? args = JsonSerializer.Deserialize<AgendamentoArgs>(toolCall.ArgumentsJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (args is null)
        {
            return "Desculpe, não consegui entender os dados do agendamento. Pode repetir?";
        }

        Result<Appointment> result = await _schedulingService.CreateAppointment(companyId, conversationId, args.Servico, args.Nome, args.Data, args.Hora);
        if (!result.IsSuccess)
        {
            return "Não consegui entender a data ou a hora. Pode confirmar, por favor?";
        }

        return $"Pronto, {args.Nome}! Seu {args.Servico} ficou agendado para {args.Data} às {args.Hora}.";
    }

    // Owner's view: a conversation + its messages, tenant-scoped. Verifies the conversation is the company's -> null = 404.
    public async Task<Result<ConversationMessages>> GetMessages(Guid companyId, Guid conversationId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Conversation? conversation = await _conversationRepository.GetConversationByIdAndCompany(conversationId, companyId);
        if (conversation is null)
        {
            return Result<ConversationMessages>.Failure("Conversa não encontrada");
        }

        List<Message> messages = await _messageRepository.GetMessagesByConversation(conversationId);
        return Result<ConversationMessages>.Success(new ConversationMessages(conversation, messages));
    }
}