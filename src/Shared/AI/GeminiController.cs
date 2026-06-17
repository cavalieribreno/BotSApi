using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Shared.AI;

[ApiController]
[Route("api/ai")]
public class AiTestController : ControllerBase
{
    private readonly GeminiAiClient _gemini;
    public AiTestController(GeminiAiClient gemini) => _gemini = gemini;

    [HttpGet("test")]
    public async Task<IActionResult> Test([FromQuery] string prompt)
    {
        var result = await _gemini.GenerateContentAsync(prompt);
        return Ok(new { result });
    }
}
