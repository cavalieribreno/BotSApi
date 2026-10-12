using BotSaaS.Api.Core.Conversations;

namespace BotSaaS.Api.Core.Channels;

// Domain entity representing a communication channel connected by a company.
public class Channel
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public MessageChannel Type { get; set; }
    public string Identifier { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Generic request payload to connect any channel provider.
public record ConnectChannelRequest(MessageChannel Type, string Identifier);

// Response DTO exposed to the owner dashboard.
public record ChannelResponse(
    Guid Id,
    Guid CompanyId,
    MessageChannel Type,
    string Identifier,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
