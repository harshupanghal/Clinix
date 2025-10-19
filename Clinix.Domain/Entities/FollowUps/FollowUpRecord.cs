using System;
using System.Collections.Generic;
using System.Linq;
using Clinix.Domain.Entities.Appointments;
using Clinix.Domain.Enums;

namespace Clinix.Domain.Entities.FollowUps;

public sealed class FollowUpRecord
    {
    public long Id { get; init; }
    public long PatientId { get; init; }
    public long? AppointmentId { get; init; }
    public long? DoctorId { get; init; }

    public string? DiagnosisSummary { get; private set; }
    public string? Notes { get; private set; }
    public long? PrescriptionId { get; private set; }

    public List<FollowUpPrescriptionSnapshot> MedicationSnapshots { get; private set; } = new();

    public FollowUpStatus Status { get; private set; } = FollowUpStatus.Pending;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public byte[]? RowVersion { get; set; }

    public List<(DateTimeOffset When, string By, string Action, string? Meta)> Audit { get; } = new();

    public Appointment? Appointment { get; set; }

    // inside FollowUpRecord class, add:
    public List<FollowUpTask> Tasks { get; private set; } = new();

    // Optionally add helper to add tasks in-domain (used by services if desired)
    public void AddTasks(IEnumerable<FollowUpTask> tasks)
        {
        if (tasks == null) return;
        Tasks.AddRange(tasks);
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((DateTimeOffset.UtcNow, "system", "tasks-added", $"count={tasks.Count()}"));
        }

    public FollowUpRecord(long patientId,
                          long? appointmentId = null,
                          long? doctorId = null,
                          string? diagnosisSummary = null,
                          string? notes = null,
                          long? prescriptionId = null)
        {
        if (patientId <= 0) throw new ArgumentException("PatientId must be provided.", nameof(patientId));
        PatientId = patientId;
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        DiagnosisSummary = diagnosisSummary;
        Notes = notes;
        PrescriptionId = prescriptionId;
        Audit.Add((DateTimeOffset.UtcNow, "system", "created", $"appointmentId={appointmentId}"));
        }

    public void AddMedicationSnapshots(IEnumerable<MedicationItem> medications)
        {
        if (medications == null) return;
        var list = medications.Select(m => new FollowUpPrescriptionSnapshot
            {
            MedicineName = m.Name,
            Dosage = m.Dosage,
            Frequency = m.Frequency,
            Duration = m.Duration,
            Notes = m.Notes,
            CreatedAt = DateTimeOffset.UtcNow
            }).ToList();

        MedicationSnapshots.AddRange(list);
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((DateTimeOffset.UtcNow, "system", "medication-snapshot-added", $"count={list.Count}"));
        }

    public void MarkActive(string actor = "system")
        {
        Status = FollowUpStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "activated", null));
        }

    public void Complete(string actor = "system")
        {
        Status = FollowUpStatus.Completed;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "completed", null));
        }

    public void Cancel(string actor, string? reason = null)
        {
        Status = FollowUpStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "cancelled", reason));
        }

    public void Archive(string actor = "system")
        {
        Status = FollowUpStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "archived", null));
        }

    public void AddNote(string actor, string note)
        {
        if (string.IsNullOrWhiteSpace(note)) return;
        Notes = string.IsNullOrWhiteSpace(Notes) ? note : $"{Notes}\n[{DateTimeOffset.UtcNow:o}] {actor}: {note}";
        UpdatedAt = DateTimeOffset.UtcNow;
        Audit.Add((UpdatedAt.Value, actor, "note-added", null));
        }
    }

