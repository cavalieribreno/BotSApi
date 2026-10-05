namespace BotSaaS.Api.Core.Conversations;

// Origin channel of the conversation.
public enum MessageChannel
{
    WhatsApp = 0,
    Telegram = 1,
    Instagram = 2
}

// Conversation: a customer's thread with one company (tenant-scoped by company + channel + contact).
public class Conversation
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public MessageChannel Channel { get; set; } = MessageChannel.WhatsApp;
    public string ChannelContactId { get; set; } = string.Empty; // whatsapp phone, telegram chat_id, or igsid
    public string? CustomerPhone { get; set; } // real phone when provided
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
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