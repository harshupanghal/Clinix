using System;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class AdminTaskActionRequest
    {
    public long TaskId { get; init; }
    public long ActorUserId { get; init; }
    public string ActorRole { get; init; } = null!;
    public string? Reason { get; init; }
    }

