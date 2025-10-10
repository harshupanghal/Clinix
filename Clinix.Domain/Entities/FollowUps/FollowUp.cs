namespace Clinix.Domain.Entities.FollowUps;

public enum FollowUpStatus { Scheduled = 0, Sent = 1, Cancelled = 2, Failed = 3 }

public class FollowUp
    {
    public long Id { get; set; }
    public long PatientUserId { get; set; }
    public long? VisitId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Scheduled;
    public string? TemplateName { get; set; }    // link to EmailTemplate.Name
    public string? JobId { get; set; }           // Hangfire job id (optional)
    public string? Channel { get; set; }         // "email", "sms", etc.

    // store last failure message if any
    public string? LastError { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

