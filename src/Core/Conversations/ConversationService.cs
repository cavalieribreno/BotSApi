using System.Data.Common;
using BotSaaS.Api.Shared.AI;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Core.Conversations;

// Conversation flow: find-or-create the customer's thread, save their message, load history, ask the AI, save the reply.
public class ConversationService : IConversationService
{
    private readonly IDatabase _databaseConnection;
    private readonly DbSession _dbSession;
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IAiClient _aiClient;
    public ConversationService(IDatabase databaseConnection, DbSession dbSession, IConversationRepository conversationRepository, IMessageRepository messageRepository, IAiClient aiClient)
    {
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _aiClient = aiClient;
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
            string systemPrompt = Environment.GetEnvironmentVariable("SYSTEM_PROMPT")
                ?? throw new InvalidOperationException("System prompt não definido");

            // Tools the model may call. Flat for now; varies per segment/niche later.
            List<ToolDefinition> tools = new List<ToolDefinition>
            {
                new ToolDefinition(
                    "registrar_agendamento",
                    "Registra um agendamento quando o cliente confirmar serviço, data e hora. Só chame quando tiver todas as informações.",
                    new List<ToolParameter>
                    {
                        new ToolParameter("servico", "string", "O serviço desejado, ex: corte de cabelo", true),
                        new ToolParameter("data", "string", "A data no formato AAAA-MM-DD", true),
                        new ToolParameter("hora", "string", "A hora no formato HH:MM (24h)", true),
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

        // text or tool call
        string response;
        if(aiResponse is TextReply text)
        {
            response = text.Text;
        } 
        else if(aiResponse is ToolCallReply)
        {
           throw new NotImplementedException("tool call não tratado");
        }
        else
        {
            throw new InvalidOperationException();
        }

        // 3) write the reply - open, write, close
        try
        {
            using DbConnection connection = _databaseConnection.CreateConnection();
            _dbSession.Connection = connection;
            await connection.OpenAsync();

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
}