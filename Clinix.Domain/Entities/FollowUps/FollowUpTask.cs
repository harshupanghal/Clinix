// inside namespace Clinix.Domain.Entities.FollowUps
using Clinix.Domain.Entities.FollowUps;
using Clinix.Domain.Enums;

public sealed class FollowUpTask
    {
    public long Id { get; init; }
    public long FollowUpRecordId { get; init; }

    public FollowUpTaskType TaskType { get; private set; }
    public string Payload { get; private set; } = null!;
    public DateTimeOffset ScheduledAt { get; private set; }
    public bool IsClaimed { get; private set; } = false;
    public DateTimeOffset? ClaimedAt { get; private set; }
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public FollowUpTaskStatus Status { get; private set; } = FollowUpTaskStatus.Pending;
    public DateTimeOffset? LastAttemptAt { get; private set; }
    public string? ResultMetadata { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public byte[]? RowVersion { get; set; }

    public List<(DateTimeOffset When, string By, string Action, string? Meta)> Audit { get; } = new();

    // Navigation back to parent
    public FollowUpRecord? FollowUpRecord { get; set; }

    // ctor
    public FollowUpTask(long followUpRecordId,
                        FollowUpTaskType taskType,
                        string payload,
                        DateTimeOffset scheduledAt,
                        int maxAttempts = 3)
        {
        if (followUpRecordId <= 0) throw new ArgumentException("FollowUpRecordId must be provided", nameof(followUpRecordId));
        FollowUpRecordId = followUpRecordId;
        TaskType = taskType;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        ScheduledAt = scheduledAt;
        MaxAttempts = Math.Max(1, maxAttempts);
        Audit.Add((DateTimeOffset.UtcNow, "system", "task-created", $"type={taskType};scheduledAt={scheduledAt:o}"));
        }

    // existing methods kept...
    public void MarkInProgress(string actor = "worker")
        {
        Status = FollowUpTaskStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "in-progress", null));
        IsClaimed = true;
        ClaimedAt = DateTimeOffset.UtcNow;
        }

    public void MarkCompleted(string actor = "worker", string? metadata = null)
        {
        Status = FollowUpTaskStatus.Completed;
        LastAttemptAt = DateTimeOffset.UtcNow;
        AttemptCount++;
        ResultMetadata = metadata;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "completed", metadata));
        }

    public void MarkFailed(string actor = "worker", string? metadata = null)
        {
        AttemptCount++;
        LastAttemptAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        ResultMetadata = metadata;

        if (AttemptCount >= MaxAttempts)
            {
            Status = FollowUpTaskStatus.DeadLettered;
            Audit.Add((UpdatedAt.Value, actor, "deadlettered", metadata));
            }
        else
            {
            Status = FollowUpTaskStatus.Failed;
            Audit.Add((UpdatedAt.Value, actor, "failed", metadata));
            }
        }

    public void Cancel(string actor = "admin", string? reason = null)
        {
        Status = FollowUpTaskStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "cancelled", reason));
        }

    /// <summary>
    /// Domain method to reschedule a task safely.
    /// </summary>
    public void Reschedule(DateTimeOffset newScheduledAt, string actor = "admin")
        {
        if (newScheduledAt <= DateTimeOffset.UtcNow.AddMinutes(-5))
            throw new InvalidOperationException("Rescheduled time must be in the future (or slightly in the past for small adjustments).");

        ScheduledAt = newScheduledAt;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "rescheduled", $"new={newScheduledAt:o}"));
        // Reset status to Pending if previously failed/cancelled (business rule)
        if (Status == FollowUpTaskStatus.Failed || Status == FollowUpTaskStatus.DeadLettered)
            {
            Status = FollowUpTaskStatus.Pending;
            AttemptCount = 0;
            }
        }

    /// <summary>
    /// Mark claimed by scheduler (used by repository during Claim).
    /// </summary>
    public void Claim(string actor = "scheduler")
        {
        IsClaimed = true;
        ClaimedAt = DateTimeOffset.UtcNow;
        Audit.Add((DateTimeOffset.UtcNow, actor, "claimed", null));
        }
    }
