using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.AI;

namespace BotSaaS.Api.Capabilities.Professionals;

public class GetProfessionalsTool : IChatTool
{
    private readonly IProfessionalService _professionalService;

    public GetProfessionalsTool(IProfessionalService professionalService)
    {
        _professionalService = professionalService;
    }

    // No parameters: lists the company's active professionals.
    public ToolDefinition Definition => new ToolDefinition(
        "consultar_profissionais",
        "Consulta os profissionais/barbeiros ativos disponíveis para atendimento na empresa. " +
        "Use quando o cliente perguntar quem atende, quais são os profissionais, ou com quem pode agendar. " +
        "Não precisa de parâmetros.",
        new List<ToolParameter>());

    // Fetch active professionals for the company, return them as data for the model to phrase.
    // FinalResponse: false -> loops back to the model to phrase the reply naturally to the customer.
    public async Task<ToolOutcome> Handle(WhoContext context, string argsJson)
    {
        List<Professional> professionals = await _professionalService.GetProfessionals(context.CompanyId, ProfessionalStatus.Active);
        if (professionals.Count == 0)
        {
            return new ToolOutcome("Nenhum profissional disponível no momento.", FinalResponse: false);
        }

        List<string> lines = new List<string>();
        foreach (Professional professional in professionals)
        {
            lines.Add($"- {professional.Name}");
        }

        return new ToolOutcome($"Profissionais disponíveis:\n{string.Join("\n", lines)}", FinalResponse: false);
    }
}
