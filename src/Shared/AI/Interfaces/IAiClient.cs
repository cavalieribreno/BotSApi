namespace BotSaaS.Api.Shared.AI;

public interface IAiClient
{
    Task<string> GenerateReply(string systemPrompt, IReadOnlyList<ChatMessage> history);
}