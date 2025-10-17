using System;

namespace Clinix.Domain.Entities;

/// <summary>
/// Lightweight table to implement schedule locks for doctors.
/// Row keyed by DoctorId. We update LockedUntil to claim lock.
/// </summary>
public sealed class ScheduleLock
    {
    public long DoctorId { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public string? LockedBy { get; set; }
    }
