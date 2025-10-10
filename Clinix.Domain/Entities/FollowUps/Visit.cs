namespace Clinix.Domain.Entities.FollowUps;

public class Visit
    {
    public long Id { get; set; }
    public long PatientUserId { get; set; }      // links to Patient.UserId
    public long? DoctorUserId { get; set; }      // optional
    public DateTime VisitDateUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // Should we auto-schedule follow-up? Set by UI or business rule.
    public bool AutoScheduleFollowUp { get; set; } = true;
    public int FollowUpDaysAfterVisit { get; set; } = 3; // default 3 days

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

