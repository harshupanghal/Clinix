using Clinix.Domain.Entities.Appointments;

namespace Clinix.Domain.Entities.ApplicationUsers;

public class Doctor
    {
    public long UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Degree { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public int? ExperienceYears { get; set; }
    public string? ClinicAddress { get; set; }
    public string WorkHoursJson { get; set; } = string.Empty;
    public ICollection<AppointmentSlot> Slots { get; set; } = new List<AppointmentSlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

