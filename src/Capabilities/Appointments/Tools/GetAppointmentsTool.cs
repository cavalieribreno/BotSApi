using System.Data.Common;
using System.Globalization;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Capabilities.Appointments;

public class GetAppointmentsTool : IChatTool
{
    private readonly IAppointmentService _appointmentService;
    public GetAppointmentsTool(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // No parameters: the customer is identified by the phone in WhoContext, not by model-filled args.
    public ToolDefinition Definition => new ToolDefinition(
        "consultar_agendamentos",
        "Consulta os agendamentos ativos do próprio cliente. Use quando ele perguntar o que tem marcado " +
        "(ex: 'o que tenho marcado?', 'quais meus horários?'). Não precisa de parâmetros.",
        new List<ToolParameter>());

    // Fetch the customer's appointments (company + phone), keep only upcoming active ones, return them as data
    // for the model to phrase. FinalResponse: false -> loops back to the model (listing is informational, not a
    // new commitment, so it's safe to let the model word it). A DB failure is terminal, phrased by us.
    public async Task<ToolOutcome> Handle(WhoContext context, string argsJson)
    {
        List<Appointment> appointments;
        try
        {
            appointments = await _appointmentService.GetAppointmentsByCustomer(context.CompanyId, context.CustomerPhone);
        }
        catch (DbException)
        {
            return new ToolOutcome("Não consegui consultar seus agendamentos agora. Tente de novo em instantes.", FinalResponse: true);
        }

        CultureInfo ptBr = new CultureInfo("pt-BR");
        DateTime now = DateTime.Now;

        List<string> lines = new List<string>();
        foreach (Appointment appointment in appointments)
        {
            bool active = appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Confirmed;
            if (!active || appointment.ScheduledAt < now)
            {
                continue; // only upcoming, active bookings
            }

            string quando = appointment.ScheduledAt.ToString("dddd, dd/MM 'às' HH:mm", ptBr);
            string status = "pendente";
            if (appointment.Status == AppointmentStatus.Confirmed)
            {
                status = "confirmado";
            }
            lines.Add($"{appointment.ServiceName} — {quando} ({status})");
        }
        if (lines.Count == 0)
        {
            return new ToolOutcome("Nenhum agendamento ativo.", FinalResponse: false);
        }
        return new ToolOutcome(string.Join("\n", lines), FinalResponse: false);
    }
}