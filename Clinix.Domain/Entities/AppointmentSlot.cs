namespace Clinix.Domain.Entities;


public enum SlotStatus { Available, Booked, Blocked }


public class AppointmentSlot
    {
    public int Id { get; set; }
    public int DoctorId { get; set; }


    // store UTC
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }


    public SlotStatus Status { get; set; } = SlotStatus.Available;


    // EF Core rowversion for optimistic concurrency
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();


    public Doctor Doctor { get; set; } = null!;
    public Appointment? Appointment { get; set; }
    }