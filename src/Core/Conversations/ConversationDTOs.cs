namespace BotSaaS.Api.Core.Conversations;

// Entry dto: customer's phone + message text for the conversation flow.
public class ProcessMessageRequest
{
    public string CustomerPhone { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}