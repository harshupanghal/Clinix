// Application/Options/ReminderOptions.cs
namespace Clinix.Application.Options;

public sealed class ReminderOptions
    {
    public int IntervalSeconds { get; set; } = 60;
    public int LookAheadMinutes { get; set; } = 15;
    }
