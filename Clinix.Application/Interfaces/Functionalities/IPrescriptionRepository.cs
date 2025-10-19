//using Clinix.Domain.Entities.FollowUp;
//namespace Clinix.Application.Interfaces.Functionalities;
///// <summary>
///// Repository abstraction for prescription access.
///// </summary>
//public interface IPrescriptionRepository
//    {
//    Task<Prescription?> GetByAppointmentIdAsync(long appointmentId, CancellationToken ct = default);
//    Task<IEnumerable<Prescription>> GetLatestForPatientAsync(long patientId, int limit = 5, CancellationToken ct = default);
//    }