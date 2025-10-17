using System;

namespace Clinix.Domain.ValueObjects;

/// <summary>
/// Represents a contiguous time range (inclusive start, exclusive end).
/// </summary>
public sealed record TimeRange(DateTimeOffset Start, DateTimeOffset End)
    {
    public TimeSpan Duration => End - Start;

    public bool Overlaps(TimeRange other) => Start < other.End && other.Start < End;

    public void Deconstruct(out DateTimeOffset start, out DateTimeOffset end) => (start, end) = (Start, End);
    }

