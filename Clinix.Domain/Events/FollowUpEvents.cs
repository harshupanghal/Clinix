namespace Clinix.Domain.Events;

using Clinix.Domain.Abstractions;


public sealed class FollowUpCreated : IDomainEvent
    {
    public long FollowUpId { get; }
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    // ✅ Fixed: 'id', 'apptId' → 'followUpId', 'appointmentId'
    public FollowUpCreated(long followUpId, long appointmentId)
        {
        FollowUpId = followUpId;
        AppointmentId = appointmentId;
        }
    }

public sealed class FollowUpCompleted : IDomainEvent
    {
    public long FollowUpId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    // ✅ Fixed: 'id' → 'followUpId'
    public FollowUpCompleted(long followUpId)
        => FollowUpId = followUpId;
    }
