namespace DomainRelay.EFCore.Outbox.Dispatching;

internal static class OutboxBackoff
{
    public static TimeSpan ComputeDelay(int attempt, TimeSpan baseDelay, TimeSpan maxDelay)
    {
        // Exponential: base * 2^(attempt-1), clamped, with small jitter
        var exp = Math.Pow(2, Math.Max(0, attempt - 1));
        var ms = baseDelay.TotalMilliseconds * exp;

        ms = Math.Min(ms, maxDelay.TotalMilliseconds);

        // jitter +/- 15%
        var jitter = 1.0 + (Random.Shared.NextDouble() * 0.30 - 0.15);
        ms *= jitter;

        return TimeSpan.FromMilliseconds(Math.Max(0, ms));
    }
}
