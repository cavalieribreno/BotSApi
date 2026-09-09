namespace BotSaaS.Api.Shared.AI;

public abstract record ChatMessage;

public record UserMessage (string Content) : ChatMessage;
public record AssistantMessage (string Content) : ChatMessage;
public record AssistantToolCall (string Id, string Name, string ArgumentsJson) : ChatMessage;
public record ToolResult (string ToolCallId, string Content) : ChatMessage;