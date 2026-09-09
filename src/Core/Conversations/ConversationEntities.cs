namespace BotSaaS.Api.Core.Conversations;

// Conversation: a customer's thread with one company (tenant-scoped by company + phone).
public class Conversation
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Who sent a persisted message. Persistence discriminator (stored as INT in messages.role) - not an AI concept.
public enum MessageRole { User, Assistant }

// Message: one turn in a conversation.
public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public MessageRole Role { get ; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}