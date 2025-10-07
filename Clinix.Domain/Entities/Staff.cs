using System;

namespace Clinix.Domain.Entities;

public class Staff
    {
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string Position { get; set; } = null!; // "Receptionist", "Chemist", etc.
    public string? Department { get; set; }
    public string? ShiftInfo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

