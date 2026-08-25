namespace BotSaaS.Api.Capabilities.Appointments;

// Default availability rule: a slot is free unless an active booking already sits at the exact same time.
// The seam (IAvailabilityPolicy) lets a niche (e.g. Barber) swap this for an overlap-window rule later.
public class GenericAvailabilityPolicy : IAvailabilityPolicy
{
    private readonly IAppointmentRepository _appointmentRepository;
    public GenericAvailabilityPolicy(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    // Generic = exact-minute match: start == end == scheduledAt (SlotTaken uses BETWEEN, inclusive).
    // Race-safe ONLY when the caller holds the tenant lock (LockCompany) inside the same transaction.
    public async Task<bool> IsSlotFree(Guid companyId, DateTime scheduledAt)
    {
        return !await _appointmentRepository.SlotTaken(companyId, scheduledAt, scheduledAt); // same hour, not window
    }
}