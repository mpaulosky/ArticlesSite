namespace Web.Infrastructure;

/// <summary>
/// Options for optimistic concurrency retry policy.
/// </summary>
public sealed class ConcurrencyOptions
{
    /// <summary>
    /// Maximum number of retry attempts before giving up.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff.
    /// </summary>
    public int BaseDelayMilliseconds { get; set; } = 100;

    /// <summary>
    /// Maximum delay cap in milliseconds for exponential backoff.
    /// </summary>
    public int MaxDelayMilliseconds { get; set; } = 2000;

    /// <summary>
    /// Maximum jitter in milliseconds to add to backoff.
    /// </summary>
    public int JitterMilliseconds { get; set; } = 100;
}
