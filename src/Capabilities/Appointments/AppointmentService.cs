using System.Data.Common;
using System.Globalization;
using BotSaaS.Api.Core.Companies;
using BotSaaS.Api.Shared.Database;
using BotSaaS.Api.Shared.Results;

namespace BotSaaS.Api.Capabilities.Appointments;

// Appointment domain: create an appointment (from a conversation) and list a company's appointments.
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDatabase _databaseConnection;
    private readonly ICompaniesRepository _companiesRepository;
    private readonly IAvailabilityPolicy _availabilityPolicy;
    private readonly DbSession _dbSession;
    public AppointmentService(IAppointmentRepository appointmentRepository, IDatabase databaseConnection, ICompaniesRepository companiesRepository, IAvailabilityPolicy availabilityPolicy, DbSession dbSession)
    {
        _appointmentRepository = appointmentRepository;
        _databaseConnection = databaseConnection;
        _companiesRepository = companiesRepository;
        _availabilityPolicy = availabilityPolicy;
        _dbSession = dbSession;
    }

    // Builds a valid Appointment from primitives (parses "yyyy-MM-dd" + "HH:mm" into ScheduledAt, business-local naive time) and persists it.
    public async Task<Result<Appointment>> CreateAppointment(Guid companyId, Guid conversationId, string serviceName, string customerName, string data, string hora)
    {
        // strict parse - if the model sent a bad date/time, fail cleanly instead of crashing
        if (!DateTime.TryParseExact($"{data} {hora}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime scheduledAt))
        {
            return Result<Appointment>.Failure("Data ou hora inválida");
        }
        
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        using DbTransaction transaction = await connection.BeginTransactionAsync();
        _dbSession.Transaction = transaction;

        Appointment appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ConversationId = conversationId,
            ServiceName = serviceName,
            CustomerName = customerName,
            ScheduledAt = scheduledAt,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        try
        {
            await _companiesRepository.LockCompany(companyId);
            if(!await _availabilityPolicy.IsSlotFree(companyId, scheduledAt))
            {
                await transaction.RollbackAsync();
                return Result<Appointment>.Conflict("Esse horário não está disponível.");
            }
            await _appointmentRepository.InsertAppointment(appointment);
            await transaction.CommitAsync();
            return Result<Appointment>.Success(appointment); 
        }
        catch (DbException)
        {
            await transaction.RollbackAsync();
            return Result<Appointment>.Failure("Não foi possível concluir o agendamento. Tente novamente.");
        }
    }

    // Owner's view: all appointments of a company. Entry point (from a controller), so it opens its own connection.
    public async Task<List<Appointment>> GetAppointments(Guid companyId)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        return await _appointmentRepository.GetAppointmentsByCompany(companyId);
    }

    // Owner action: change an appointment's status (tenant-scoped). 0 rows updated -> not found for this company.
    public async Task<Result<bool>> UpdateAppointmentStatus(Guid appointmentId, Guid companyId, AppointmentStatus status)
    {
        using DbConnection connection = _databaseConnection.CreateConnection();
        _dbSession.Connection = connection;
        await connection.OpenAsync();

        int rows = await _appointmentRepository.UpdateAppointmentStatus(appointmentId, companyId, status);
        if (rows == 0)
        {
            return Result<bool>.Failure("Agendamento não encontrado.");
        }
        return Result<bool>.Success(true);
    }
}
