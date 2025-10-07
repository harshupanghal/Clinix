using Clinix.Domain.Entities;


namespace Clinix.Application.Interfaces;


public interface INotificationService
    {
    Task NotifyAppointmentCreatedAsync(Appointment appointment, CancellationToken ct);
    }