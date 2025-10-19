using System;
using System.Collections.Generic;
using Clinix.Application.Dtos.FollowUps;

namespace Clinix.Application.Dtos.FollowUps;
public sealed class CreateManualFollowUpRequest
    {
    public long PatientId { get; init; }
    public long? AppointmentId { get; init; }
    public long? DoctorId { get; init; }
    public string? Diagnosis { get; init; }
    public string? Notes { get; init; }
    public IEnumerable<CreateFollowUpTaskDto>? Tasks { get; init; }
    public IEnumerable<MedicationDto>? Medications { get; init; }
    }
public sealed class MedicationDto { public string Name { get; init; } = string.Empty; public string Dosage { get; init; } = string.Empty; public string Frequency { get; init; } = string.Empty; public string Duration { get; init; } = string.Empty; public string? Notes { get; init; } }
