using System.Security.Claims;
using BotSaaS.Api.Core.Conversations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Channels;

[Authorize]
[ApiController]
[Route("api/channels")]
public class ChannelController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetChannels()
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        List<Channel> channels = await _channelService.GetChannelsByCompany(companyGuid);
        return Ok(channels);
    }

    [HttpPost]
    public async Task<IActionResult> ConnectChannel([FromBody] ConnectChannelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Identifier))
        {
            return BadRequest(new { error = "Identifier is required." });
        }

        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        var result = await _channelService.ConnectChannel(companyGuid, request.Type, request.Identifier);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DisconnectChannel(Guid id)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        var result = await _channelService.DisconnectChannel(companyGuid, id);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}
