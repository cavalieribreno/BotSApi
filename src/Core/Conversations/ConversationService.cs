using System.Data.Common;
using System.Globalization;
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
    // IEnumerable: the DI injects ALL registered IChatTool implementations (one per tool). We route by Definition.Name.
    private readonly IEnumerable<IChatTool> _chatTools; 
    
    public ConversationService(IDatabase databaseConnection, DbSession dbSession, IConversationRepository conversationRepository, IMessageRepository messageRepository, IAiClient aiClient, IEnumerable<IChatTool> chatTools)
    {
        _databaseConnection = databaseConnection;
        _dbSession = dbSession;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _aiClient = aiClient;
        _chatTools = chatTools;
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
                if(findMessage.Role == ChatRole.User)
                {
                    chatHistory.Add(new UserMessage(findMessage.Content));
                }
                else
                {
                    chatHistory.Add(new AssistantMessage(findMessage.Content));
                }
            }
        }
        catch (DbException)
        {
            return Result<string>.Failure("Erro ao inserir conversa");
        }

        // 2) LLM - no DB connection open here
        string response;
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

            List<ToolDefinition> toolDefs = new List<ToolDefinition>();
            foreach(IChatTool tool in _chatTools)
            {
                toolDefs.Add(tool.Definition);
            }

            // Multi-turn: keep asking the model until it phrases a text reply (or we hit the ceiling).
            string? finalText = null;
            const int maxTurns = 3;

            for(int turn = 0; turn < maxTurns && finalText is null; turn++)
            {
                AiResponse aiResponse = await _aiClient.GenerateReplyAsync(systemPrompt, chatHistory, toolDefs);
                
                if(aiResponse is TextReply text)
                {
                    finalText = text.Text;
                }
                else if(aiResponse is ToolCallReply toolCall)
                {
                    IChatTool? chosen = null;
                    foreach(IChatTool tool in _chatTools)
                    {
                        if(tool.Definition.Name == toolCall.Name)
                        {
                            chosen = tool;
                            break;
                        }
                    }
                    if(chosen is null)
                    {
                        finalText = "Desculpe, não entendi o pedido";
                    }
                    else
                    {
                        ToolOutcome toolOutcome = await chosen.Handle(companyId, conversationId, toolCall.ArgumentsJson);
                        if (toolOutcome.FinalResponse)
                        {
                            finalText = toolOutcome.Content;
                        }
                        else
                        {
                            // feed the call + its result back so the model can phrase the reply next turn
                            chatHistory.Add(new AssistantToolCall(toolCall.Id, toolCall.Name, toolCall.ArgumentsJson));
                            chatHistory.Add(new ToolResult(toolCall.Id, toolOutcome.Content));
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("AiResponse inesperado");
                }
            }
            response = finalText ?? "Tudo certo por aqui! Posso ajudar em mais alguma coisa?"; // ceiling hit
        }
        catch (AiClientException)
        {
            return Result<string>.Failure("Erro ao gerar resposta da IA");
        }

        // 3) write the assistant message - own connection scope
        try
        {
            using DbConnection connection = _databaseConnection.CreateConnection();
            _dbSession.Connection = connection;
            _dbSession.Transaction = null!; // possible open transaction by tool
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
    
    // Owner's view: a conversation + its messages, tenant-scoped. Verifies the conversation is the company's -> null = 404.
    public async Task<Result<ConversationMessages>> GetMessages(Guid companyId, Guid conversationId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        Conversation? conversation = await _conversationRepository.GetConversationByIdAndCompany(conversationId, companyId);
        if (conversation is null)
        {
            return Result<ConversationMessages>.Failure("Conversa não encontrada.");
        }

        List<Message> messages = await _messageRepository.GetMessagesByConversation(conversationId);
        return Result<ConversationMessages>.Success(new ConversationMessages(conversation, messages));
    }
}