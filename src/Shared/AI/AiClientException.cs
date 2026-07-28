namespace BotSaaS.Api.Shared.AI;

// Neutral AI failure (bad HTTP status or empty body). Keeps provider/HTTP types out of the Core.
public class AiClientException : Exception
{
    public AiClientException(string message) : base(message){}
}