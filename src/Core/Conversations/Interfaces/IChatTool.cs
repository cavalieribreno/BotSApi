using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Core.Conversations;

public record ToolOutcome(string Content, bool FinalResponse);

public interface IChatTool
{
    ToolDefinition Definition { get; }
    Task<ToolOutcome> Handle(Guid companyId, Guid conversationId, string argsJson);
}