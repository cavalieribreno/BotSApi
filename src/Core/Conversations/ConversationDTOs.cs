namespace BotSaaS.Api.Core.Conversations;

// Entry dto: customer's phone + message text for the conversation flow.
public class ProcessMessageRequest
{
    public string CustomerPhone { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
}

// Deserialized args of the registrar_agendamento tool call (bridge from the model's JSON to the domain).
// TODO: should live in the Scheduling module with the tool, once tool ownership moves there (see ConversationService).
public record AgendamentoArgs(string Servico, string Data, string Hora, string Nome);