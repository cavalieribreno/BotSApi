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
    public async Task<IActionResult> GetAppointments([FromQuery] DateOnly? date = null)
    {
        string? companyId = User.FindFirstValue("companyId");
        if(!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida"});
        }
        DateOnly targetDate;
        if (date.HasValue)
        {
            targetDate = date.Value;
        }
        else
        {
            targetDate = DateOnly.FromDateTime(DateTime.Now);
        }
        List<Appointment> appointments = await _appointmentService.GetAppointments(companyGuid, targetDate);

        List<AppointmentResponse> response = new List<AppointmentResponse>();
        foreach(Appointment appointment in appointments)
        {
            response.Add(new AppointmentResponse(
                appointment.Id, appointment.CompanyId, appointment.ConversationId, appointment.ProfessionalId, appointment.ProfessionalName, appointment.ServiceName, appointment.CustomerName, appointment.ScheduledAt, appointment.Status.ToString(), appointment.CreatedAt
            ));
        }
        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] AppointmentStatus status)
    {
        string? companyId = User.FindFirstValue("companyId");
        if (!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida" });
        }

        Result<bool> result = await _appointmentService.UpdateAppointmentStatus(id, companyGuid, status);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error });
        }
        return NoContent();
    }
}