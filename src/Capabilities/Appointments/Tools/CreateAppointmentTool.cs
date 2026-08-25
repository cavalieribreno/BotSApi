using System.Globalization;
using System.Text.Json;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.AI;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Appointments;

// The registrar_agendamento tool - owns its definition + handler, inside the Appointments capability.
public class RegisterAppointmentTool : IChatTool
{
    private readonly IAppointmentService _appointmentService;

    public RegisterAppointmentTool(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // tool definition
    public ToolDefinition Definition => new ToolDefinition(
        "registrar_agendamento",
        "Registra um agendamento. Só chame quando JÁ TIVER os quatro dados: serviço, nome do cliente, " +
        "a DATA e o HORÁRIO EXATO. Se faltar qualquer um — inclusive o NOME — pergunte ao cliente ANTES de chamar. " +
        "NUNCA invente dados nem use genéricos: nada de 'cliente' no nome nem 'de manhã' na hora.",
        new List<ToolParameter>
        {
            new ToolParameter("servico", "string", "O serviço desejado, ex: corte de cabelo", true),
            new ToolParameter("data", "string", "A data no formato AAAA-MM-DD", true),
            new ToolParameter("hora", "string", "O horário EXATO no formato HH:MM 24h, ex: 09:00. Nunca use termos vagos como 'de manhã'.", true),
            new ToolParameter("nome", "string", "O nome do cliente, dito por ele. Nunca invente nem use genéricos como 'cliente'; se ele não informou, pergunte antes.", true)
        });

    // Parse the args the model filled in -> create the appointment -> return a message for the customer.
    public async Task<string> Handle(Guid companyId, Guid conversationId, string argsJson)
    {
        AppointmentArgs? args = JsonSerializer.Deserialize<AppointmentArgs>(argsJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        if(args is null)
        {
            return "Desculpe, não consegui entender os dados do agendamento. Pode repetir?";
        }

        Result<Appointment> result = await _appointmentService.CreateAppointment(companyId, conversationId, args.Servico, args.Nome, args.Data, args.Hora);

        if (!result.IsSuccess)
        {
            if(result.ErrorType == ErrorType.Conflict)
            {
                return "Esse horário não está disponível. Quer tentar outro?";
            }
            else
            {
                return "Não consegui entender a data ou hora. Pode confirmar, por favor?";
            }
        }
        // format from the parsed ScheduledAt (source of truth), pt-BR so the weekday reads in Portuguese
        Appointment appointment = result.Value!;
        string quando = appointment.ScheduledAt.ToString("dddd, dd/MM 'às' HH:mm", new CultureInfo("pt-BR"));
        return $"Pronto, {args.Nome}! Seu {args.Servico} ficou agendado para {quando}.";
    }
}
