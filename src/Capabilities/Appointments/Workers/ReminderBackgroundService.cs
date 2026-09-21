using System.Data.Common;
using BotSaaS.Api.Channels;
using BotSaaS.Api.Shared.Database;

namespace BotSaaS.Api.Capabilities.Appointments;

// Background worker that periodically scans for appointments in the next 2 hours and sends proactive reminders.
public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public ReminderBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendReminders(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Graceful shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ReminderBackgroundService: {ex.Message}");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CheckAndSendReminders(CancellationToken stoppingToken)
    {
        DateTime now = DateTime.Now; // Business-local naive time
        DateTime windowStart = now;
        DateTime windowEnd = now.AddHours(2);

        using IServiceScope scope = _scopeFactory.CreateScope();
        IDatabase database = scope.ServiceProvider.GetRequiredService<IDatabase>();
        DbSession dbSession = scope.ServiceProvider.GetRequiredService<DbSession>();
        IAppointmentRepository appointmentRepository = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
        IChannelSender channelSender = scope.ServiceProvider.GetRequiredService<IChannelSender>();

        // 1. Fetch pending reminders and release DB connection immediately
        List<UpcomingReminder> reminders = new List<UpcomingReminder>();
        using (DbConnection connection = database.CreateConnection())
        {
            dbSession.Connection = connection;
            await connection.OpenAsync(stoppingToken);
            reminders = await appointmentRepository.GetPendingReminders(windowStart, windowEnd);
        }

        if (reminders.Count == 0) return;

        // 2. Outbound HTTP requests over the network (DB connection is free)
        List<Guid> sentIds = new List<Guid>();
        foreach (UpcomingReminder reminder in reminders)
        {
            if (stoppingToken.IsCancellationRequested) break;

            string horario = reminder.ScheduledAt.ToString("HH:mm");
            string text = $"Olá, {reminder.CustomerName}! Passando para lembrar que seu agendamento de {reminder.ServiceName} está marcado para hoje às {horario}.";

            bool sent = await channelSender.SendMessageAsync(reminder.CustomerPhone, text, stoppingToken);
            if (sent)
            {
                sentIds.Add(reminder.AppointmentId);
            }
        }

        // 3. Fast batch update in DB only for successfully delivered reminders
        if (sentIds.Count > 0)
        {
            using DbConnection connection = database.CreateConnection();
            dbSession.Connection = connection;
            await connection.OpenAsync(stoppingToken);

            DateTime nowUtc = DateTime.UtcNow;
            foreach (Guid id in sentIds)
            {
                await appointmentRepository.MarkReminded(id, nowUtc);       
            }
        }
    }
}
