using System;
using System.Collections.Generic;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class FollowUpDto
    {
    public long Id { get; init; }
    public long PatientId { get; init; }
    public long? AppointmentId { get; init; }
    public long? DoctorId { get; init; }
    public string? DiagnosisSummary { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public IEnumerable<FollowUpPrescriptionSnapshotDto> Medications { get; init; } = Array.Empty<FollowUpPrescriptionSnapshotDto>();
    }

public sealed class FollowUpPrescriptionSnapshotDto
    {
    public string MedicineName { get; init; } = null!;
    public string Dosage { get; init; } = null!;
    public string Frequency { get; init; } = null!;
    public string Duration { get; init; } = null!;
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    }






//namespace Clinix.Application.Dtos.FollowUp;

///// <summary>
///// DTO used to request creation of a FollowUp from an appointment.
///// </summary>
//public class CreateFollowUpFromAppointmentRequest
//    {
//    /// <summary>Appointment id that is the source of this follow-up.</summary>
//    public long AppointmentId { get; init; }

//    /// <summary>User id creating the follow-up (system actor or clinician).</summary>
//    public long CreatedByUserId { get; init; }

//    /// <summary>Snapshot whether patient consent was given at creation time.</summary>
//    public bool ConsentGiven { get; init; }

//    /// <summary>Optional manual notes to include in the followup record.</summary>
//    public string? Notes { get; init; }

//    /// <summary>Optional suggested schedule in days from appointment (e.g. [3, 14] => two reminders).</summary>
//    public IReadOnlyList<int>? SuggestedScheduleDays { get; init; }
//    }

///// <summary>
///// Response DTO returned after creating a FollowUp.
///// </summary>
//public class CreateFollowUpFromAppointmentResponse
//    {
//    public long FollowUpId { get; init; }
//    public long PatientId { get; init; }
//    public long? AppointmentId { get; init; }
//    public string Status { get; init; } = default!;
//    }

///// <summary>
///// DTO representing a scheduled follow-up item (for UI or worker).
///// </summary>
//public class FollowUpItemDto
//    {
//    public long Id { get; init; }
//    public long FollowUpId { get; init; }
//    public string Type { get; init; } = default!;
//    public string Channel { get; init; } = default!;
//    public DateTime ScheduledAtUtc { get; init; }
//    public string Status { get; init; } = default!;
//    public int AttemptCount { get; init; }
//    public int MaxAttempts { get; init; }
//    public long? MessageTemplateId { get; init; }
//    }

///// <summary>
///// DTO used by the worker to process an item (enriched model).
///// </summary>
//public class FollowUpItemProcessingModel
//    {
//    public long ItemId { get; init; }
//    public long FollowUpId { get; init; }
//    public long PatientId { get; init; }
//    public long? AppointmentId { get; init; }
//    public string Channel { get; init; } = default!;
//    public DateTime ScheduledAtUtc { get; init; }
//    public int AttemptCount { get; set; }
//    public int MaxAttempts { get; init; }
//    public long? TemplateId { get; init; }
//    public string Type { get; init; } = default!;
//    public string? ToContactValue { get; init; }    // phone or email or device id
//    public IDictionary<string, object>? TemplateModel { get; init; }
//    }

///// <summary>
///// Result returned by the worker after attempting to send an item.
///// </summary>
//public class ProcessItemResult
//    {
//    public long ItemId { get; init; }
//    public bool Success { get; init; }
//    public bool IsTransientFailure { get; init; }
//    public string? ProviderMessageId { get; init; }
//    public string? FailureReason { get; init; }
//    }

