using System;

namespace Clinix.Domain.Entities.FollowUps;

public sealed class FollowUpPrescriptionSnapshot
    {
    public long Id { get; init; }
    public long FollowUpRecordId { get; init; }

    public string MedicineName { get; init; } = null!;
    public string Dosage { get; init; } = null!;
    public string Frequency { get; init; } = null!;
    public string Duration { get; init; } = null!;
    public string? Notes { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    // Navigation
    public FollowUpRecord? FollowUpRecord { get; set; }
    }

