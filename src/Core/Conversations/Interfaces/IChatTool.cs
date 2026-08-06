using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Core.Conversations;

public interface IChatTool
{
    ToolDefinition Definition { get; }
    Task<string> Handle(Guid companyId, Guid conversationId, string argsJson);
}