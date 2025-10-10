namespace Clinix.Application.Dtos;

/// <summary>
/// Request DTO used by patients to update their profile from the dashboard.
/// Note: phone changes are intentionally omitted for safety (handle phone changes via support or a separate flow).
/// </summary>
public class PatientUpdateProfileRequest
    {
    public long UserId { get; set; }

    // Basic profile (optional)
    public string? FullName { get; set; }
    public string? Email { get; set; }

    // Medical / profile details
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ExistingConditions { get; set; }
    }

