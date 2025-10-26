namespace Clinix.Application.Dtos.Patient;

/// <summary>
/// Data sent to the patient dashboard UI. Appointment/FollowUp lists are commented out for now.
/// </summary>
public class PatientDashboardDto
    {
    // User core
    public long UserId { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public bool IsProfileCompleted { get; set; }

    // Patient-specific
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ExistingConditions { get; set; }

    public DateTime RegisteredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // public IEnumerable<AppointmentDto> UpcomingAppointments { get; set; } = Enumerable.Empty<AppointmentDto>();
    // public IEnumerable<FollowUpDto> FollowUps { get; set; } = Enumerable.Empty<FollowUpDto>();
    }

