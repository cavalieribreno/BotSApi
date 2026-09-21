using System.Security.Claims;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Companies;

[ApiController]
[Route("api/business-hours")]
[Authorize]
public class BusinessHoursController : ControllerBase
{
    private readonly ICompaniesService _companiesService;

    public BusinessHoursController(ICompaniesService companiesService)
    {
        _companiesService = companiesService;
    }

    // Owner retrieves their company's 7-day operating hours schedule
    [HttpGet]
    public async Task<IActionResult> GetBusinessHours()
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida." });
        }

        List<BusinessHoursResponse> hours = await _companiesService.GetBusinessHours(companyGuid);
        return Ok(hours);
    }

    // Owner updates their company's operating hours schedule
    [HttpPut]
    public async Task<IActionResult> UpdateBusinessHours([FromBody] List<BusinessHoursRequest> request)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida." });
        }

        Result<bool> result = await _companiesService.UpdateBusinessHours(companyGuid, request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { message = "Horários de funcionamento atualizados com sucesso." });
    }
}
