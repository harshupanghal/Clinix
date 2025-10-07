using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;

namespace Clinix.Application.Interfaces.RepoInterfaces;

public interface IAppointmentRepository
    {
    Task<AppointmentSlot?> GetSlotByIdAsync(int id, CancellationToken ct);
    Task<List<AppointmentSlot>> GetAvailableSlotsAsync(int doctorId, DateTime fromUtc, DateTime toUtc, CancellationToken ct);
    Task<BookSlotResult> TryBookSlotAsync(int slotId, Appointment appt, CancellationToken ct);
    Task<List<Doctor>> GetDoctorsBySpecialtyAsync(string specialty, CancellationToken ct);
    Task<Doctor?> GetDoctorByIdAsync(int id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    }

public record BookSlotResult(bool Success, string? ErrorMessage, Appointment? Appointment);