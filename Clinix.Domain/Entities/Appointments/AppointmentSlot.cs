using Clinix.Domain.Entities.ApplicationUsers;
using Clinix.Domain.Entities.Appointments;

public enum SlotStatus
    {
    Available,
    Booked,
    Blocked
    }

public class AppointmentSlot
    {
    public long Id { get; set; }
    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }

    public SlotStatus Status { get; set; } = SlotStatus.Available;

    public Appointment? Appointment { get; set; } // one-to-one
    public byte[]? RowVersion { get; set; } // for concurrency control
    }