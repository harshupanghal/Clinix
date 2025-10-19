using System;
using Clinix.Domain.Enums;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class CreateFollowUpTaskDto
{
    public FollowUpTaskType Type { get; init; } = FollowUpTaskType.MedicationReminder; // ✅ default safe
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; init; }
    public int MaxAttempts { get; init; } = 3;
}
