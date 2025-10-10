namespace Clinix.Domain.Entities.FollowUps;

public class EmailTemplate
    {
    public long Id { get; set; }
    public string Name { get; set; } = null!; // unique key e.g. "followup_basic_3days"
    public string Subject { get; set; } = null!;
    public string BodyHtml { get; set; } = null!; // supports token replacement like {{PatientName}}
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

