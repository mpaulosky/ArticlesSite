namespace Web.Infrastructure;

/// <summary>
/// Simple metrics publisher used to record concurrency/retry related counts.
/// Implementations may forward metrics to telemetry (OTel, AppInsights) or keep in-memory for tests.
/// </summary>
public interface IMetricsPublisher
{
    void IncrementAttempt();
    void IncrementRetry();
    void IncrementSuccess();
    void IncrementConflict();
    void RecordRetryCount(int retryCount);
    void RecordRequestLatency(string operation, System.TimeSpan duration);
}

/// <summary>
/// Default no-op implementation used in production when no telemetry plumbing is configured.
/// </summary>
public sealed class NoOpMetricsPublisher : IMetricsPublisher
{
    public void IncrementAttempt() { }
    public void IncrementRetry() { }
    public void IncrementSuccess() { }
    public void IncrementConflict() { }
    public void RecordRetryCount(int retryCount) { }
    public void RecordRequestLatency(string operation, System.TimeSpan duration) { }
}
