namespace BotSaaS.Api.Shared.AI;

// Neutral result of one AI call: either a text reply or a tool-call request.
public abstract record AiResponse;

// The model answered with text.
public record TextReply(string Text) : AiResponse;

// The model requested a tool call - Name + raw JSON args (Core parses; Shared stays dumb).
public record ToolCallReply(string Id, string Name, string ArgumentsJson) : AiResponse;