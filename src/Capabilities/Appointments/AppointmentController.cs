using System.Security.Claims;
using BotSaaS.Api.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Capabilities.Appointments;
// Owner's view: lists the appointments of the caller's company (tenant from the JWT).
[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    public AppointmentController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        string? companyId = User.FindFirstValue("companyId");
        if(!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida"});
        }
        List <Appointment> appointments = await _appointmentService.GetAppointments(companyGuid);

        List<AppointmentResponse> response = new List<AppointmentResponse>();
        foreach(Appointment appointment in appointments)
        {
            response.Add(new AppointmentResponse(
                appointment.Id, appointment.CompanyId, appointment.ConversationId, appointment.ServiceName, appointment.CustomerName, appointment.ScheduledAt, appointment.Status.ToString(), appointment.CreatedAt
            ));
        }
        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        // enum name -> AppointmentStatus. IsDefined blocks bogus/out-of-range values (Enum.TryParse accepts any int).
        if (!Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out AppointmentStatus parsedStatus) || !Enum.IsDefined(parsedStatus))
        {
            return BadRequest(new { error = "Status inválido" });
        }

        Result<bool> result = await _appointmentService.UpdateAppointmentStatus(companyGuid, id, parsedStatus);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }
        return NoContent();
    }
}