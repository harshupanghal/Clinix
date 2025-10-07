using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.ServiceInterfaces;

public interface INotificationService
    {
    Task NotifyAppointmentCreatedAsync(Appointment appointment, CancellationToken ct);
    }