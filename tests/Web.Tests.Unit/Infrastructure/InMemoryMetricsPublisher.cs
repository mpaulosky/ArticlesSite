using System;
using System.Collections.Concurrent;
using Web.Infrastructure;

namespace Web.Tests.Unit.Infrastructure;

/// <summary>
/// Simple in-memory metrics publisher used by unit tests to assert metrics were recorded.
/// </summary>
public sealed class InMemoryMetricsPublisher : IMetricsPublisher
{
    private readonly ConcurrentDictionary<string, long> _counts = new();

    public void IncrementAttempt() => _counts.AddOrUpdate("attempt", 1, (_, v) => v + 1);
    public void IncrementRetry() => _counts.AddOrUpdate("retry", 1, (_, v) => v + 1);
    public void IncrementSuccess() => _counts.AddOrUpdate("success", 1, (_, v) => v + 1);
    public void IncrementConflict() => _counts.AddOrUpdate("conflict", 1, (_, v) => v + 1);
    public void RecordRetryCount(int retryCount) => _counts.AddOrUpdate("retryCount", retryCount, (_, v) => v + retryCount);

    public void RecordRequestLatency(string operation, TimeSpan duration)
    {
        _counts.AddOrUpdate($"latency_{operation}", duration.Ticks, (_, v) => v + duration.Ticks);
    }

    public long GetCount(string key) => _counts.TryGetValue(key, out var v) ? v : 0;
}
