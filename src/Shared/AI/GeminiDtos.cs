namespace BotSaaS.Api.Shared.AI;

record GeminiResponse(List<GeminiCandidate> Candidates);
record GeminiCandidate(GeminiContent Content);
record GeminiContent(List<GeminiPart> Parts);
record GeminiPart(string Text);