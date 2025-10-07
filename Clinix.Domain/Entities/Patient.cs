using System;

namespace Clinix.Domain.Entities;

public class Patient
    {
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContact { get; set; }
    public string? MedicalHistory { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

