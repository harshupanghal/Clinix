using System;

namespace Clinix.Domain.Entities;

public class OutboxMessage
    {
    public long Id { get; set; }

    // Event type, e.g. "AppointmentRescheduled", "AppointmentCreated"
    public string Type { get; set; } = null!;

    // JSON-serialized payload
    public string PayloadJson { get; set; } = null!;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    // Processing metadata
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedAtUtc { get; set; }
    public int AttemptCount { get; set; } = 0;

    // Optional destination routing (email, sms, push)
    public string? Channel { get; set; }
    }

