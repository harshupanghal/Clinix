using System;

namespace Clinix.Application.Dtos.FollowUps;

public sealed class AdminFollowUpUpdateRequest
    {
    public long FollowUpId { get; init; }
    public string? DiagnosisSummary { get; init; }
    public string? Notes { get; init; }
    public long? AssignDoctorId { get; init; } // optional reassign
    public string ActorRole { get; init; } = null!; // should be "Admin"
    public long ActorUserId { get; init; }
    }

