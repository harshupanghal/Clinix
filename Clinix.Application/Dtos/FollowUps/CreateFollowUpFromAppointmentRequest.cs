using System;

namespace Clinix.Application.Dtos.FollowUps;

/// <summary>
/// Request to create a follow-up from an appointment.
/// The handler will query appointment and clinical info from repositories.
/// </summary>
public sealed class CreateFollowUpFromAppointmentRequest
    {
    public long AppointmentId { get; init; }
    public long CreatedByUserId { get; init; } // admin/doctor/system that initiated creation
    public string? InitiatorNote { get; init; }
    public bool EnqueueMedicationReminders { get; init; } = true;
    public bool EnqueueRevisitReminder { get; init; } = true;
    }

