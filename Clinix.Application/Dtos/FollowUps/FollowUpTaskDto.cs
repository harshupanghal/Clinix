using System;
using Clinix.Domain.Enums;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class FollowUpTaskDto
    {
    public long Id { get; init; }
    public long PatientId { get; init; }
    public FollowUpTaskType Type { get; init; }   // ✅ strongly typed, prevents string bugs
    public string Description { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public DateTimeOffset ScheduledAt { get; init; }
    public FollowUpTaskStatus Status { get; init; }
    }
