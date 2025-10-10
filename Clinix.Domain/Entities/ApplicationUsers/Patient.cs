using System;
using System.Collections.Generic;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Entities.FollowUps;

namespace Clinix.Domain.Entities.ApplicationUsers
    {
    public class Patient
        {
        public long PatientId { get; set; } // Primary key (separate from UserId)

        // 🔗 Relation to core User entity
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // 🧬 Medical Identity
        public string? MedicalRecordNumber { get; set; } // Hospital-issued unique ID
        public string? BloodGroup { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // 🩺 Medical Background
        public string? KnownAllergies { get; set; } // e.g., "Penicillin, Peanuts"
        public string? ExistingConditions { get; set; } // e.g., "Diabetes, Hypertension"
        public string? MedicalHistory { get; set; } // Summarized or structured JSON

        // 🏥 Insurance and Emergency
        public string? InsuranceProvider { get; set; }
        public string? InsurancePolicyNumber { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactNumber { get; set; }

        // ⚙️ Operational / System
        public bool IsActive { get; set; } = true; // Soft delete or blocked status
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        // 📊 Audit / Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; } // staff username/id
        public string? UpdatedBy { get; set; }

        // 🔁 Navigation Collections (relations)
        public ICollection<Appointment>? Appointments { get; set; } = new List<Appointment>();

        public ICollection<FollowUp>? FollowUps { get; set; }
        //public ICollection<MedicalRecord>? MedicalRecords { get; set; }
        //public ICollection<Invoice>? Invoices { get; set; }
        }
    }
