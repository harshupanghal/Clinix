using Clinix.Domain.Entities.Appointments;
using System.Collections.Generic;

namespace Clinix.Domain.Entities.ApplicationUsers
    {
    public class Doctor
        {
        public long DoctorId { get; set; }
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public string? Degree { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public int? ExperienceYears { get; set; }
        public long? DepartmentId { get; set; }
        public string? RoomNumber { get; set; }
        public bool IsOnDuty { get; set; } = true;
        public string WorkHoursJson { get; set; } = string.Empty;
        public decimal? ConsultationFee { get; set; }
        public string? ExtensionNumber { get; set; }
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public byte[]? RowVersion { get; set; }

        // 🔗 Relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        }
    }
