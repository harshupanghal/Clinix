namespace Clinix.Application.Interfaces.Functionalities;

public interface INotificationService
    {
    Task NotifyPatientAsync(long patientId, string subject, string message);
    Task NotifyDoctorAsync(long doctorId, string subject, string message);
    Task NotifyAdminAsync(string subject, string message);
    }
