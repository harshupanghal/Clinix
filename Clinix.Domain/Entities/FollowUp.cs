namespace Clinix.Domain.Entities;
using Clinix.Domain.Abstractions;
using Clinix.Domain.Enums;
using Clinix.Domain.Events;

public sealed class FollowUp : Entity
    {
    public long AppointmentId { get; private set; }
    public DateTimeOffset DueBy { get; private set; }
    public FollowUpStatus Status { get; private set; }
    public string? Reason { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? LastRemindedAt { get; private set; }

    private FollowUp() { }

    private FollowUp(long appointmentId, DateTimeOffset dueBy, string? reason)
        {
        AppointmentId = appointmentId;
        DueBy = dueBy;
        Reason = reason;
        Status = FollowUpStatus.Pending;
        Raise(new FollowUpCreated(Id, appointmentId));
        }

    public static FollowUp Create(long appointmentId, DateTimeOffset dueBy, string? reason = null)
        => new FollowUp(appointmentId, dueBy, reason);

    public void Reschedule(DateTimeOffset newDueBy)
        {
        if (Status != FollowUpStatus.Pending) return;
        DueBy = newDueBy;
        UpdatedAt = DateTimeOffset.UtcNow;
        }

    public void Complete(string? notes = null)
        {
        if (Status != FollowUpStatus.Pending) return;
        Status = FollowUpStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        Notes = notes ?? Notes;
        Raise(new FollowUpCompleted(Id));
        }

    public void Cancel(string? notes = null)
        {
        if (Status == FollowUpStatus.Completed) return;
        Status = FollowUpStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        Notes = notes ?? Notes;
        }

    public void MarkRemindedNow() => LastRemindedAt = DateTimeOffset.UtcNow;
    }
