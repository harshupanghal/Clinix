using System;

namespace Clinix.Domain.Entities;

public class OutboxMessage
    {
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string PayloadJson { get; set; } = null!;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAtUtc { get; set; }
    public int AttemptCount { get; set; } = 0;

    public string? Channel { get; set; }
    }

