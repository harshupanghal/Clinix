namespace Clinix.Domain.Events;

using Clinix.Domain.Abstractions;

public sealed class FollowUpCreated : IDomainEvent
    {
    public long FollowUpId { get; }
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public FollowUpCreated(long id, long apptId) { FollowUpId = id; AppointmentId = apptId; }
    }

public sealed class FollowUpCompleted : IDomainEvent
    {
    public long FollowUpId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public FollowUpCompleted(long id) { FollowUpId = id; }
    }
