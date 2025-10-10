using Clinix.Domain.Entities.ApplicationUsers;

public class Appointment
    {
    public long Id { get; set; }

    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public long PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public long? AppointmentSlotId { get; set; } // optional
    public AppointmentSlot? AppointmentSlot { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "Scheduled"; // Scheduled, Rescheduled, Cancelled, Completed
    public string? Reason { get; set; }
    public string? Type { get; set; } // Consultation, Follow-up, Emergency

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
