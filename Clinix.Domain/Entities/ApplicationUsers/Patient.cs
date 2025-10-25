using System;
using System.Collections.Generic;

namespace Clinix.Domain.Entities.ApplicationUsers;

public class Patient
    {
    public long PatientId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string? MedicalRecordNumber { get; set; }
    public string? BloodGroup { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ExistingConditions { get; set; }
    public string? MedicalHistory { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsProfileComplete()
        {
        return !string.IsNullOrEmpty(Gender)
            && DateOfBirth.HasValue
            && !string.IsNullOrEmpty(EmergencyContactNumber);
        }

    // Navigation: patient can have many appointments
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }

