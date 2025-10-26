namespace Clinix.Domain.Events;

using Clinix.Domain.Abstractions;

public sealed class AppointmentScheduled : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public AppointmentScheduled(long appointmentId) => AppointmentId = appointmentId;
    }

public sealed class AppointmentRescheduled : IDomainEvent
    {
    public long AppointmentId { get; }
    public DateTimeOffset PreviousStart { get; }
    public DateTimeOffset NewStart { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public AppointmentRescheduled(long id, DateTimeOffset prev, DateTimeOffset next) { AppointmentId = id; PreviousStart = prev; NewStart = next; }
    }

public sealed class AppointmentCancelled : IDomainEvent
    {
    public long AppointmentId { get; }
    public string? Reason { get; }
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public AppointmentCancelled(long id, string? reason) { AppointmentId = id; Reason = reason; }
    }
