namespace BotSaaS.Api.Core.Conversations;

// Entry dto: customer's phone + message text for the conversation flow.
public class ProcessMessageRequest
{
    public string CustomerPhone { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}

// Owner-facing view of a message in a conversation. Role as string ("User"/"Assistant").
public record MessageResponse(string Role, string Content, DateTime CreatedAt);

// Owner-facing conversation detail: who it's with (phone) + the messages.
public record ConversationDetailResponse(string CustomerPhone, List<MessageResponse> Messages);

// Service return bundle: the conversation + its messages (domain, mapped to the DTO in the controller).
public record ConversationMessages(Conversation Conversation, List<Message> Messages);