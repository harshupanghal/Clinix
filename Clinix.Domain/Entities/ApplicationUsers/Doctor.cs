using Clinix.Domain.Entities.Appointments;
using System;
using System.Collections.Generic;

namespace Clinix.Domain.Entities.ApplicationUsers
    {
    public class Doctor
        {
        public long DoctorId { get; set; } // PK separate from UserId

        // 🔗 Core Identity Link
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // 🧠 Professional Details
        public string? Degree { get; set; } // e.g., "MBBS, MD"
        public string? Specialty { get; set; } // e.g., "Cardiology, Pediatrics"
        public string? LicenseNumber { get; set; } // Medical registration number
        public int? ExperienceYears { get; set; }

        // 🏥 Hospital Association
        //public long? HospitalId { get; set; } // If multi-hospital setup
        public long? DepartmentId { get; set; } // e.g., Cardiology Department
        public string? RoomNumber { get; set; } // OPD room or consulting room
        public bool IsOnDuty { get; set; } = true; // Mark availability for scheduling

        // ⏰ Scheduling
        // Keep as simple JSON for MVP — can migrate to relational tables later
        public string WorkHoursJson { get; set; } = string.Empty; // e.g. [{"Day":"Mon","Start":"09:00","End":"17:00"}]
        public decimal? ConsultationFee { get; set; } // Useful for billing

        // 📞 Contact & Optional
        public string? ExtensionNumber { get; set; } // Internal phone if applicable
        public string? Notes { get; set; } // Admin/staff comments

        // 📊 Audit
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        // 🔁 Navigation Collections
        public ICollection<AppointmentSlot> Slots { get; set; } = new List<AppointmentSlot>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        }
    }
