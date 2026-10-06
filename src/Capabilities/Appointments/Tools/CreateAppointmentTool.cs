using System.Globalization;
using System.Text.Json;
using BotSaaS.Api.Capabilities.Professionals;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Core.Customers;
using BotSaaS.Api.Shared.AI;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Appointments;

// The registrar_agendamento tool - owns its definition + handler, inside the Appointments capability.
public class CreateAppointmentTool : IChatTool
{
    private readonly IAppointmentService _appointmentService;
    private readonly IProfessionalService _professionalService;
    private readonly ICustomerService _customerService;

    public CreateAppointmentTool(IAppointmentService appointmentService, IProfessionalService professionalService, ICustomerService customerService)
    {
        _appointmentService = appointmentService;
        _professionalService = professionalService;
        _customerService = customerService;
    }

    // tool definition
    public ToolDefinition Definition => new ToolDefinition(
        "registrar_agendamento",
        "Registra um agendamento. Só chame quando JÁ TIVER os dados: serviço, nome do cliente, " +
        "o profissional, a DATA e o HORÁRIO EXATO. Se faltar qualquer um — pergunte ao cliente ANTES de chamar. " +
        "NUNCA invente dados nem use genéricos: nada de 'cliente' no nome nem 'de manhã' na hora.",
        new List<ToolParameter>
        {
            new ToolParameter("servico", "string", "O serviço desejado, ex: corte de cabelo", true),
            new ToolParameter("data", "string", "A data no formato AAAA-MM-DD", true),
            new ToolParameter("hora", "string", "O horário EXATO no formato HH:MM 24h, ex: 09:00. Nunca use termos vagos como 'de manhã'.", true),
            new ToolParameter("nome", "string", "O nome do cliente, dito por ele. Nunca invente nem use genéricos como 'cliente'; se ele não informou, pergunte antes.", true),
            new ToolParameter("profissional", "string", "O nome do profissional/barbeiro", true),
            new ToolParameter("telefone", "string", "O número de WhatsApp do cliente com DDD (obrigatório se o canal for Telegram ou Instagram. Se o cliente ainda não informou, pergunte antes de chamar a tool)", false)
        });

    // Parse the args the model filled in -> create the appointment -> return a message for the customer.
    public async Task<ToolOutcome> Handle(WhoContext whoContext, string argsJson)
    {
        AppointmentArgs? args = JsonSerializer.Deserialize<AppointmentArgs>(argsJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        if (args is null ||
            string.IsNullOrWhiteSpace(args.Servico) ||
            string.IsNullOrWhiteSpace(args.Nome) ||
            string.IsNullOrWhiteSpace(args.Data) ||
            string.IsNullOrWhiteSpace(args.Hora) ||
            string.IsNullOrWhiteSpace(args.Profissional))
        {
            return new ToolOutcome("Desculpe, não consegui entender os dados do agendamento. Pode repetir?", FinalResponse: true);
        }

        List<Professional> professionals = await _professionalService.GetProfessionals(whoContext.CompanyId, ProfessionalStatus.Active);
        Professional? professional = professionals.FirstOrDefault(p => p.Name.Equals(args.Profissional.Trim(), StringComparison.OrdinalIgnoreCase));
        if (professional is null)
        {
            return new ToolOutcome($"Profissional '{args.Profissional}' não encontrado.", FinalResponse: true);
        }

        if (!DateTime.TryParseExact($"{args.Data} {args.Hora}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledAt))
        {
            return new ToolOutcome("Data ou hora inválida. Por favor, informe a data e hora desejada.", FinalResponse: true);
        }

        string finalPhone = string.Empty;
        if (whoContext.Channel == MessageChannel.WhatsApp)
        {
            finalPhone = whoContext.ChannelContactId;
        }

        if (!string.IsNullOrWhiteSpace(whoContext.CustomerPhone))
        {
            finalPhone = whoContext.CustomerPhone;
        }

        if (!string.IsNullOrWhiteSpace(args.Telefone))
        {
            finalPhone = args.Telefone;
        }

        if (string.IsNullOrWhiteSpace(finalPhone))
        {
            return new ToolOutcome("Por favor, me informe seu número de WhatsApp com DDD para confirmarmos o agendamento.", FinalResponse: true);
        }

        CreateAppointmentRequest createRequest = new CreateAppointmentRequest(professional.Id, args.Servico, args.Nome, finalPhone, scheduledAt);

        Result<Appointment> result = await _appointmentService.CreateAppointment(whoContext.CompanyId, createRequest, AppointmentOrigin.Bot, whoContext.ConversationId);

        if (!result.IsSuccess)
        {
            if (result.ErrorType == ErrorType.Conflict)
            {
                return new ToolOutcome("Esse horário não está disponível. Quer tentar outro?", FinalResponse: true);
            }
            else
            {
                string errorMessage = "Não foi possível concluir o agendamento.";
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    errorMessage = result.Error;
                }
                return new ToolOutcome(errorMessage, FinalResponse: true);
            }
        }
        // format from the parsed ScheduledAt (source of truth), pt-BR so the weekday reads in Portuguese
        Appointment appointment = result.Value!;
        await _customerService.FindOrCreateCustomer(whoContext.CompanyId, finalPhone, args.Nome);
        
        string quando = appointment.ScheduledAt.ToString("dddd, dd/MM 'às' HH:mm", new CultureInfo("pt-BR"));
        return new ToolOutcome($"Pronto, {args.Nome}! Seu {args.Servico} com {professional.Name} ficou agendado para {quando}.", FinalResponse: true);
    }
}
