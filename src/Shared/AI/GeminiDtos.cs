namespace BotSaaS.Api.Shared.AI;

// DTOs mirroring the Gemini generateContent response JSON.
record GeminiResponse(List<GeminiCandidate> Candidates);
record GeminiCandidate(GeminiContent Content);
record GeminiContent(List<GeminiPart> Parts);
record GeminiPart(string Text);