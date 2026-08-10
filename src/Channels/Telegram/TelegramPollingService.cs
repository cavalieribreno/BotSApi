using System.Text.Json;
using BotSaaS.Api.Core.Conversations;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Channels.Telegram;

// Test channel: long-polls Telegram and routes each message through the SAME ConversationService the API uses.
// Tenant is resolved from config here (single test bot). Real multi-tenant resolution = a channels table, later.
public class TelegramPollingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _http = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    // Scoped services can't be injected into a singleton BackgroundService -> resolve them per message via the factory.
    public TelegramPollingService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
        string? companyRaw = Environment.GetEnvironmentVariable("TELEGRAM_TEST_COMPANY_ID");

        // No config = channel stays off; the rest of the app runs normally.
        if (string.IsNullOrWhiteSpace(token) || !Guid.TryParse(companyRaw, out Guid companyId))
        {
            Console.WriteLine("Telegram desativado: defina TELEGRAM_BOT_TOKEN e TELEGRAM_TEST_COMPANY_ID no .env.");
            return;
        }

        Console.WriteLine("Telegram polling iniciado.");
        long offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (TgUpdate update in await GetUpdates(token, offset, stoppingToken))
                {
                    offset = update.UpdateId + 1; // ack: the next poll won't re-deliver this update

                    if (update.Message?.Text is not string text) continue; // ignore non-text updates
                    await HandleMessage(companyId, update.Message.Chat.Id, text, token, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break; // shutting down
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Telegram: erro no loop, retomando. {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    // Route one message through the conversation flow, then reply on Telegram.
    private async Task HandleMessage(Guid companyId, long chatId, string text, string token, CancellationToken ct)
    {
        string reply;

        // New scope per message: ProcessMessage is Scoped (its DbSession/repos are per unit of work).
        using (IServiceScope scope = _scopeFactory.CreateScope())
        {
            IConversationService conversations = scope.ServiceProvider.GetRequiredService<IConversationService>();
            Result<string> result = await conversations.ProcessMessage(companyId, chatId.ToString(), text);
            reply = result.IsSuccess ? result.Value! : "Desculpe, tive um problema aqui. Pode tentar de novo?";
        }

        await SendMessage(token, chatId, reply, ct);
    }

    // Long poll: the request blocks up to 30s server-side until an update arrives.
    private async Task<List<TgUpdate>> GetUpdates(string token, long offset, CancellationToken ct)
    {
        string url = $"https://api.telegram.org/bot{token}/getUpdates?offset={offset}&timeout=30";
        TgResponse? response = await _http.GetFromJsonAsync<TgResponse>(url, _json, ct);
        return response?.Result ?? new List<TgUpdate>();
    }

    private async Task SendMessage(string token, long chatId, string text, CancellationToken ct)
    {
        string url = $"https://api.telegram.org/bot{token}/sendMessage";
        await _http.PostAsJsonAsync(url, new TgSendMessage(chatId, text), _json, ct);
    }
}
