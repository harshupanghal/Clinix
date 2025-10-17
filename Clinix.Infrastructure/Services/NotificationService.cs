using Clinix.Application.Interfaces.Functionalities;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Services;

/// <summary>
/// Simple notification service that logs messages. Replace with real email/SMS/provider integration.
/// </summary>
public class NotificationService : INotificationService
    {
    private readonly ILogger<NotificationService> _log;
    public NotificationService(ILogger<NotificationService> log) => _log = log;

    public Task NotifyAdminAsync(string subject, string message)
        {
        _log.LogInformation("[NotifyAdmin] {Subject} - {Message}", subject, message);
        return Task.CompletedTask;
        }

    public Task NotifyDoctorAsync(long doctorId, string subject, string message)
        {
        _log.LogInformation("[NotifyDoctor:{DoctorId}] {Subject} - {Message}", doctorId, subject, message);
        return Task.CompletedTask;
        }

    public Task NotifyPatientAsync(long patientId, string subject, string message)
        {
        _log.LogInformation("[NotifyPatient:{PatientId}] {Subject} - {Message}", patientId, subject, message);
        return Task.CompletedTask;
        }
    }
