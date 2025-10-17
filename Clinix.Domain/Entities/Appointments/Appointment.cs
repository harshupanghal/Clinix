using System;
using System.Collections.Generic;
using Clinix.Domain.Enums;
using Clinix.Domain.ValueObjects;
using Clinix.Domain.Entities.ApplicationUsers;

namespace Clinix.Domain.Entities.Appointments
    {
    /// <summary>
    /// Aggregate root for appointments.
    /// Encapsulates domain logic for scheduling, approval, and lifecycle transitions.
    /// </summary>
    public sealed class Appointment
        {
        public long Id { get; init; } // PK
        public long DoctorId { get; init; }
        public long PatientId { get; init; }

        public DateTimeOffset StartAt { get; private set; }
        public DateTimeOffset EndAt { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Pending;
        public string? Reason { get; private set; }
        public string? Notes { get; private set; }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; private set; }
        public byte[]? RowVersion { get; set; }

        // 🔗 Navigation
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;

        // 📜 Domain audit
        public List<(DateTimeOffset When, string By, string Action, string? Meta)> Audit { get; } = new();

        // 🧱 Constructor
        public Appointment(long doctorId, long patientId, DateTimeOffset startAt, DateTimeOffset endAt, string? reason = null)
            {
            if (endAt <= startAt)
                throw new ArgumentException("End time must be after start time.");

            DoctorId = doctorId;
            PatientId = patientId;
            StartAt = startAt;
            EndAt = endAt;
            Reason = reason;

            Audit.Add((DateTimeOffset.UtcNow, "system", "created", $"{StartAt:o} -> {EndAt:o}"));
            }

        // ⚙️ Domain behaviors
        public void Approve(string actor)
            {
            Status = AppointmentStatus.Approved;
            UpdatedAt = DateTimeOffset.UtcNow;
            Audit.Add((UpdatedAt.Value, actor, "approved", null));
            }

        public void Reject(string actor, string? reason = null)
            {
            Status = AppointmentStatus.Rejected;
            UpdatedAt = DateTimeOffset.UtcNow;
            Audit.Add((UpdatedAt.Value, actor, "rejected", reason));
            }

        public void Cancel(string actor, string? reason = null)
            {
            Status = AppointmentStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
            Audit.Add((UpdatedAt.Value, actor, "cancelled", reason));
            }

        public void Reschedule(DateTimeOffset newStart, DateTimeOffset newEnd, string actor, string? note = null)
            {
            if (newEnd <= newStart)
                throw new ArgumentException("End time must be after start time.");

            var old = ($"{StartAt:o}", $"{EndAt:o}");
            StartAt = newStart;
            EndAt = newEnd;
            Status = AppointmentStatus.Rescheduled;
            UpdatedAt = DateTimeOffset.UtcNow;
            Audit.Add((UpdatedAt.Value, actor, "rescheduled", $"{old} -> {StartAt:o}:{EndAt:o}; note={note}"));
            }

        public TimeRange Range() => new(StartAt, EndAt);
        }
    }
