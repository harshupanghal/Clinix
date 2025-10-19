using System;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class FollowUpListItemDto
    {
    public long Id { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string DoctorName { get; init; } = string.Empty;
    public DateTimeOffset AppointmentDate { get; init; }
    public DateTimeOffset? NextFollowUp { get; init; }
    public string Status { get; init; } = string.Empty;
    }
