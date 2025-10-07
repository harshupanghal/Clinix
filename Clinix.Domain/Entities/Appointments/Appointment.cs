using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Domain.Entities.Appointments;

public enum AppointmentStatus { Pending, Confirmed, Completed, Cancelled }

public class Appointment
    {
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int AppointmentSlotId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Doctor Doctor { get; set; } = null!;
    // Patient reference omitted to avoid coupling; use your existing User entity mapping if needed
    public AppointmentSlot AppointmentSlot { get; set; } = null!;
    }