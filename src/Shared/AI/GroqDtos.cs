namespace BotSaaS.Api.Shared.AI;

// Slice of the Groq (OpenAI-format) response we read: choices[0].message.content
record GroqResponse(List<GroqChoice> Choices);
record GroqChoice(GroqMessage Message);
record GroqMessage(string Content);
