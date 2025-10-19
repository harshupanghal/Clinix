
namespace Clinix.Application.Utilities;

/// <summary>
/// Small helper to compute backoff durations for retry attempts.
/// This is purely deterministic and safe for scheduling.
/// </summary>
public static class RetryBackoff
    {
    /// <summary>
    /// Compute next delay after a given attempt count (0-based).
    /// </summary>
    /// <param name="attempt">previous attempt count (0 for first attempt)</param>
    /// <param name="baseSeconds">base seconds multiplier (default 60s)</param>
    /// <param name="maxSeconds">max backoff seconds (default 86400 => 24h)</param>
    public static TimeSpan ComputeNextDelay(int attempt, int baseSeconds = 60, int maxSeconds = 86400)
        {
        // exponential backoff: base * 2^attempt, cap at maxSeconds
        try
            {
            var secs = Math.Min(maxSeconds, baseSeconds * Math.Pow(2, Math.Max(0, attempt)));
            return TimeSpan.FromSeconds(Math.Max(5, secs)); // minimum 5 sec
            }
        catch
            {
            return TimeSpan.FromSeconds(baseSeconds);
            }
        }
    }

