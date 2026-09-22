using System.Security.Claims;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Capabilities.Professionals;

// Owner's view: manages professionals of the caller's company (tenant from JWT).
[Authorize]
[ApiController]
[Route("api/professionals")]
public class ProfessionalController : ControllerBase
{
    private readonly IProfessionalService _professionalService;

    public ProfessionalController(IProfessionalService professionalService)
    {
        _professionalService = professionalService;
    }

    // Owner creates a new professional for their company
    [HttpPost]
    public async Task<IActionResult> CreateProfessional([FromBody] CreateProfessionalRequest request)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<Professional> result = await _professionalService.CreateProfessional(companyGuid, request);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        Professional professional = result.Value!;
        ProfessionalResponse response = new ProfessionalResponse(professional.Id, professional.Name, professional.Status.ToString());
        return Ok(response);
    }

    // Owner lists all professionals of their company, optionally filtered by status
    [HttpGet]
    public async Task<IActionResult> GetProfessionals([FromQuery] ProfessionalStatus? status = null)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        List<Professional> professionals = await _professionalService.GetProfessionals(companyGuid, status);

        List<ProfessionalResponse> response = new List<ProfessionalResponse>();
        foreach (Professional professional in professionals)
        {
            response.Add(new ProfessionalResponse(professional.Id, professional.Name, professional.Status.ToString()));
        }
        return Ok(response);
    }

    // Owner retrieves a single professional by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfessionalById(Guid id)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<Professional> result = await _professionalService.GetProfessionalById(id, companyGuid);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        Professional professional = result.Value!;
        ProfessionalResponse response = new ProfessionalResponse(professional.Id, professional.Name, professional.Status.ToString());
        return Ok(response);
    }

    // Owner updates an existing professional's details and status
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfessional(Guid id, [FromBody] UpdateProfessionalRequest request)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<bool> result = await _professionalService.UpdateProfessional(id, companyGuid, request);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(new { message = "Profissional atualizado com sucesso." });
    }
}
