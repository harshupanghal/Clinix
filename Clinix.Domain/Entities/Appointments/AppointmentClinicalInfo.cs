using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Clinix.Domain.Entities.Appointments;

/// <summary>
/// Holds clinical details produced during or immediately after an appointment.
/// Persisted with medications serialized as JSON (MedicationsJson).
/// </summary>
public sealed class AppointmentClinicalInfo
    {
    public long Id { get; init; }
    public long AppointmentId { get; init; }

    public string? DiagnosisSummary { get; private set; }
    public string? IllnessDescription { get; private set; }

    // Backing property persisted as JSON by EF mapping
    public string? MedicationsJson { get; private set; }

    // Not persisted directly; used by application/domain. Convert to/from MedicationsJson in repositories or EF converters.
    public List<MedicationItem> Medications { get; set; } = new();

    public string? DoctorNotes { get; private set; }
    public DateTimeOffset? NextFollowUpDate { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation
    public Appointment Appointment { get; set; } = null!;

    public AppointmentClinicalInfo(long appointmentId,
                                   string? diagnosis = null,
                                   string? illness = null,
                                   IEnumerable<MedicationItem>? medications = null,
                                   string? doctorNotes = null,
                                   DateTimeOffset? nextFollowUpDate = null)
        {
        if (appointmentId <= 0) throw new ArgumentException("AppointmentId must be provided.", nameof(appointmentId));
        AppointmentId = appointmentId;
        DiagnosisSummary = diagnosis;
        IllnessDescription = illness;
        Medications = medications?.ToList() ?? new List<MedicationItem>();
        MedicationsJson = SerializeMedications(Medications);
        DoctorNotes = doctorNotes;
        NextFollowUpDate = nextFollowUpDate;
        }

    public void Update(string? diagnosis,
                       string? illness,
                       IEnumerable<MedicationItem>? medications,
                       string? doctorNotes,
                       DateTimeOffset? nextFollowUpDate)
        {
        DiagnosisSummary = diagnosis;
        IllnessDescription = illness;
        Medications = medications?.ToList() ?? new();
        MedicationsJson = SerializeMedications(Medications);
        DoctorNotes = doctorNotes;
        NextFollowUpDate = nextFollowUpDate;
        UpdatedAt = DateTimeOffset.UtcNow;
        }

    private static string SerializeMedications(IEnumerable<MedicationItem> meds)
        => JsonSerializer.Serialize(meds ?? Array.Empty<MedicationItem>());

    public static List<MedicationItem> DeserializeMedications(string? json)
        => string.IsNullOrWhiteSpace(json) ? new List<MedicationItem>() : JsonSerializer.Deserialize<List<MedicationItem>>(json) ?? new List<MedicationItem>();
    }

public sealed class MedicationItem
    {
    public string Name { get; init; } = null!;
    public string Dosage { get; init; } = null!;
    public string Frequency { get; init; } = null!;
    public string Duration { get; init; } = null!;
    public string? Notes { get; init; }

    public MedicationItem() { } // EF and serializers
    public MedicationItem(string name, string dosage, string frequency, string duration, string? notes = null)
        {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Dosage = dosage ?? throw new ArgumentNullException(nameof(dosage));
        Frequency = frequency ?? throw new ArgumentNullException(nameof(frequency));
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        Notes = notes;
        }

    public override string ToString() => $"{Name} | {Dosage} | {Frequency} | {Duration}";
    }

