namespace Clinix.Domain.Events;

using Clinix.Domain.Abstractions;

public sealed class AppointmentScheduled : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public AppointmentScheduled(long appointmentId)
        => AppointmentId = appointmentId;
    }

public sealed class AppointmentRescheduled : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset PreviousStart { get; }
    public DateTimeOffset NewStart { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public AppointmentRescheduled(long appointmentId, DateTimeOffset previousStart, DateTimeOffset newStart)
        {
        AppointmentId = appointmentId;
        PreviousStart = previousStart;
        NewStart = newStart;
        }
    }

public sealed class AppointmentCancelled : IDomainEvent
    {
    public long AppointmentId { get; }
    public string? Reason { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public AppointmentCancelled(long appointmentId, string? reason)
        {
        AppointmentId = appointmentId;
        Reason = reason;
        }
    }

public sealed class AppointmentCompleted : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public AppointmentCompleted(long appointmentId)
        => AppointmentId = appointmentId;
    }

public sealed class AppointmentApproved : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public AppointmentApproved(long appointmentId)
        => AppointmentId = appointmentId;
    }

public sealed class AppointmentRejected : IDomainEvent
    {
    public long AppointmentId { get; }
    public string? Reason { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;

    public AppointmentRejected(long appointmentId, string? reason)
        {
        AppointmentId = appointmentId;
        Reason = reason;
        }
    }

