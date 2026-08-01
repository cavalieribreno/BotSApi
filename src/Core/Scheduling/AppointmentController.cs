using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BotSaaS.Api.Core.Scheduling;
// Owner's view: lists the appointments of the caller's company (tenant from the JWT).
[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly ISchedulingService _schedulingService;
    public AppointmentController(ISchedulingService schedulingService)
    {
        _schedulingService = schedulingService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        string? companyId = User.FindFirstValue("companyId");
        if(!Guid.TryParse(companyId, out Guid companyGuid))
        {
            return Unauthorized(new { error = "Empresa inválida"});
        }
        List <Appointment> appointments = await _schedulingService.GetAppointments(companyGuid);

        List<AppointmentResponse> response = new List<AppointmentResponse>();
        foreach(Appointment appointment in appointments)
        {
            response.Add(new AppointmentResponse(
                appointment.Id, appointment.CompanyId, appointment.ConversationId, appointment.ServiceName, appointment.CustomerName, appointment.ScheduledAt, appointment.Status.ToString(), appointment.CreatedAt
            ));
        }
        return Ok(response);
    }
}