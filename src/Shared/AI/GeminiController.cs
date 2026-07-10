using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Shared.AI;

// Temporary test endpoint for the AI integration -- builds a 1-message history (no persistence yet).
[ApiController]
[Route("api/ai")]
public class AiTestController : ControllerBase
{
    private readonly IAiClient _gemini;
    private readonly string _systemPrompt;
    private ChatMemoryAI _historyChat;
     
    public AiTestController(IAiClient gemini, ChatMemoryAI historyChat)
    {
        _gemini = gemini;
        _historyChat = historyChat;
        _systemPrompt = Environment.GetEnvironmentVariable("SYSTEM_PROMPT")
            ?? throw new InvalidOperationException("SYSTEM_PROMPT não definido");
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test([FromQuery] string messageUser)
    {
        ChatMessage message = new ChatMessage(ChatRole.User, messageUser);
        _historyChat.History.Add(message);
        
        var result = await _gemini.GenerateReplyAsync(_systemPrompt, _historyChat.History);
        if(result != null)
        {
            ChatMessage messageModel = new ChatMessage(ChatRole.Assistant, result);
            _historyChat.History.Add(messageModel);
        }
        
        return Ok(new { result });
    }
}
