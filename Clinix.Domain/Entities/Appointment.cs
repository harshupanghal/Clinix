namespace Clinix.Domain.Entities;
using Clinix.Domain.Abstractions;
using Clinix.Domain.Enums;
using Clinix.Domain.Events;
using Clinix.Domain.ValueObjects;

public sealed class Appointment : Entity
    {
    public long PatientId { get; private set; }
    public long ProviderId { get; private set; }
    public AppointmentType Type { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateRange When { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public List<FollowUp> FollowUps { get; private set; } = new();

    private Appointment() { }

    private Appointment(long patientId, long providerId, AppointmentType type, DateRange when, string? notes)
        {
        PatientId = patientId;
        ProviderId = providerId;
        Type = type;
        When = when;
        Notes = notes;
        Status = AppointmentStatus.Scheduled;
        // ✅ Don't raise event here - will be raised after save when ID is assigned
        }

    public static Appointment Schedule(long patientId, long providerId, AppointmentType type, DateRange when, string? notes = null)
        => new Appointment(patientId, providerId, type, when, notes);

    // ✅ Method to raise event after save (called from application service)
    public void RaiseScheduledEvent()
        {
        if (Id == 0) throw new InvalidOperationException("Cannot raise scheduled event before appointment is saved");
        Raise(new AppointmentScheduled(Id));
        }

    public void Reschedule(DateRange newWhen)
        {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.Rejected)
            throw new InvalidOperationException("Cannot reschedule in current state.");
        var prev = When.Start;
        When = newWhen;
        UpdatedAt = DateTimeOffset.UtcNow;
        Raise(new AppointmentRescheduled(Id, prev, newWhen.Start));
        }

    public void Cancel(string? reason = null)
        {
        if (Status == AppointmentStatus.Cancelled) return;
        Status = AppointmentStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        Raise(new AppointmentCancelled(Id, reason));
        }

    public void Complete()
        {
        if (Status != AppointmentStatus.Scheduled) return;
        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
        Raise(new AppointmentCompleted(Id));
        }

    public void Approve()
        {
        if (Status is AppointmentStatus.Rejected or AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot approve in current state.");
        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTimeOffset.UtcNow;
        Raise(new AppointmentApproved(Id));
        }

    public void Reject(string? reason = null)
        {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot reject a completed appointment.");
        Status = AppointmentStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddNote(string.IsNullOrWhiteSpace(reason) ? "Rejected." : $"Rejected: {reason}");
        Raise(new AppointmentRejected(Id, reason));
        }

    public void AddNote(string note)
        {
        if (string.IsNullOrWhiteSpace(note)) return;
        Notes = string.IsNullOrWhiteSpace(Notes) ? note.Trim() : $"{Notes}\n{note.Trim()}";
        UpdatedAt = DateTimeOffset.UtcNow;
        }

    public FollowUp CreateFollowUp(DateTimeOffset dueBy, string? reason = null)
        {
        var fu = FollowUp.Create(Id, dueBy, reason);
        FollowUps.Add(fu);
        return fu;
        }
    }
