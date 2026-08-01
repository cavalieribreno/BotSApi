namespace BotSaaS.Api.Shared.AI;

// Neutral AI client: hides the provider so the model can be swapped without touching callers.
public interface IAiClient
{
    Task<AiResponse> GenerateReplyAsync(string systemPrompt, IReadOnlyList<ChatMessage> history, IReadOnlyList<ToolDefinition> tools);
}