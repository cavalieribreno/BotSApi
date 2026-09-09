using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Core.Conversations;

public record ToolOutcome(string Content, bool FinalResponse);

public record WhoContext(Guid CompanyId, Guid ConversationId, string CustomerPhone);

public interface IChatTool
{
    ToolDefinition Definition { get; }
    Task<ToolOutcome> Handle(WhoContext whoContext, string argsJson);
}