using System.Security.Claims;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Conversations;

// Dev/token entry to the conversation flow: tenant comes from the JWT.
// Production entry is the WhatsApp webhook (companyId from the Channel).
[ApiController]
[Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;
    public ConversationController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProcessMessage([FromBody] ProcessMessageRequest request)
    {
        string? companyId = User.FindFirstValue("companyId");

        if(!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new {error = "Empresa inválida"});
        }

        Result<string> result = await _conversationService.ProcessMessage(companyGuid, request.CustomerPhone, request.MessageText);

        if(!result.IsSuccess) return BadRequest(new { error = result.Error });

        return Ok(new { reply = result.Value });
    }

    // Owner's view: the messages of a conversation (tenant-scoped). Used by the panel to show the chat behind an appointment.
    [Authorize]
    [HttpGet("{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<ConversationMessages> result = await _conversationService.GetMessages(companyGuid, id);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        ConversationMessages detail = result.Value!;
        List<MessageResponse> messages = new List<MessageResponse>();
        foreach (Message message in detail.Messages)
        {
            messages.Add(new MessageResponse(message.Role.ToString(), message.Content, message.CreatedAt));
        }
        return Ok(new ConversationDetailResponse(detail.Conversation.CustomerPhone, messages));
    }
}