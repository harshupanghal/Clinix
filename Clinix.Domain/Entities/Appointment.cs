// Domain/Entities/Appointment.cs
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
        Raise(new AppointmentScheduled(Id));
        }

    public static Appointment Schedule(long patientId, long providerId, AppointmentType type, DateRange when, string? notes = null)
        => new Appointment(patientId, providerId, type, when, notes);

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
        }

    public void Approve()
        {
        if (Status is AppointmentStatus.Rejected or AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot approve in current state.");
        UpdatedAt = DateTimeOffset.UtcNow;
        }

    public void Reject(string? reason = null)
        {
        if (Status == AppointmentStatus.Completed) throw new InvalidOperationException("Cannot reject a completed appointment.");
        Status = AppointmentStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
        AddNote(string.IsNullOrWhiteSpace(reason) ? "Rejected." : $"Rejected: {reason}");
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
