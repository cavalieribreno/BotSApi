using System.Text.Json.Serialization;

namespace BotSaaS.Api.Shared.AI;

// Slice of the Groq (OpenAI-format) response we read: choices[0].message.content
record GroqResponse(List<GroqChoice> Choices);
record GroqChoice(GroqMessage Message);
record GroqMessage(string? Content, [property: JsonPropertyName("tool_calls")] List<GroqToolCall>? ToolCalls);
record GroqToolCall(GroqFunction Function);
record GroqFunction(string Name, string Arguments);
