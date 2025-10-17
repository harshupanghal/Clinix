using System;
using System.Collections.Generic;

namespace Clinix.Domain.Entities.Appointments;

/// <summary>
/// Defines a doctor's weekly working hours (e.g. Mon-Fri 09:00-17:00).
/// Stored as local time range per day-of-week.
/// </summary>
public sealed class DoctorWorkingHours
    {
    public long Id { get; init; } = default!;
    // Key: DayOfWeek (0..6), Value: list of time ranges for that day (local time)
    public Dictionary<DayOfWeek, List<(TimeSpan Start, TimeSpan End)>> WeeklyHours { get; init; } = new();

    public bool IsWorkingOn(DateTimeOffset dateTime)
        {
        var local = dateTime.ToLocalTime();
        var dow = local.DayOfWeek;
        if (!WeeklyHours.TryGetValue(dow, out var ranges)) return false;
        var t = local.TimeOfDay;
        foreach (var r in ranges)
            {
            if (t >= r.Start && t < r.End) return true;
            }
        return false;
        }
    }
