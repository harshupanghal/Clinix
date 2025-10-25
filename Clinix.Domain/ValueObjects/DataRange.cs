// Domain/ValueObjects/DateRange.cs
namespace Clinix.Domain.ValueObjects;

public sealed class DateRange
    {
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private DateRange() { }

    public DateRange(DateTimeOffset start, DateTimeOffset end)
        {
        if (end < start) throw new ArgumentException("End must be >= Start.");
        Start = start;
        End = end;
        }

    public TimeSpan Duration => End - Start;
    public bool Overlaps(DateRange other) => Start < other.End && other.Start < End;
    public DateRange WithNewStart(DateTimeOffset s) => new DateRange(s, End);
    public DateRange WithNewEnd(DateTimeOffset e) => new DateRange(Start, e);
    }
