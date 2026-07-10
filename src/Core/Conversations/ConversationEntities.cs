using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Core.Conversations;

public class Conversation
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public ChatRole Role { get ; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}