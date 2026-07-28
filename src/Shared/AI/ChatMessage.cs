namespace BotSaaS.Api.Shared.AI;


public enum ChatRole { User, Assistant }; // who
public record ChatMessage(ChatRole Role, string Content); // who + what