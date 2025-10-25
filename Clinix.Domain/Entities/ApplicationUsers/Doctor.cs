// Domain/Entities/ApplicationUsers/Doctor.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Clinix.Domain.Entities.ApplicationUsers;

public class Doctor
    {
    public long DoctorId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string? Degree { get; set; }
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public int? ExperienceYears { get; set; }
    public string? RoomNumber { get; set; }
    public bool IsOnDuty { get; set; } = true;
    public decimal? ConsultationFee { get; set; }
    public string? ExtensionNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // NEW: Link to Provider for appointment scheduling
    public long ProviderId { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();

    public string DisplayName => User?.FullName ?? $"Doctor {DoctorId}";
    }


public class DoctorSchedule
    {
    public long Id { get; set; }
    public long DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    public string? Notes { get; set; }
    }

