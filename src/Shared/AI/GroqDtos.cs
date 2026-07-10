namespace BotSaaS.Api.Shared.AI;

record GroqResponse(List<GroqChoice> Choices);
record GroqChoice(GroqMessage Message);
record GroqMessage(string Content);
