namespace Clinix.Domain.Entities.FollowUps;

public class CommunicationPreference
    {
    public long Id { get; set; }
    public long PatientUserId { get; set; }
    public bool EmailOptIn { get; set; } = true;
    public bool SmsOptIn { get; set; } = false;
    public string? PreferredChannel { get; set; } = "email"; // default

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

