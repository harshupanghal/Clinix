namespace Clinix.Application.Dtos;

/// <summary>
/// DTO for completing the patient profile (post-login).
/// </summary>
public class CompletePatientProfileRequest
    {
    /// <summary>User id (from auth claims).</summary>
    public long UserId { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ExistingConditions { get; set; }
    }

