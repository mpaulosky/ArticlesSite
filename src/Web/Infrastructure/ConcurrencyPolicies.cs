using System;
using System.Linq;
using System.Threading.Tasks;
using Polly;
using Shared.Abstractions;
using Web.Components.Features.Articles.Entities;

namespace Web.Infrastructure;

/// <summary>
/// Factory for concurrency retry policies used by handlers that update articles.
/// Centralizes the Polly policy creation so it can be reused or swapped for testing/DI.
/// </summary>
public static class ConcurrencyPolicies
{
    /// <summary>
    /// Creates a Polly policy that retries on optimistic concurrency failures (<see cref="ResultErrorCode.Concurrency"/>).
    /// The policy uses exponential backoff with jitter based on the provided options.
    /// </summary>
    public static IAsyncPolicy<Result<Article>> CreatePolicy(ConcurrencyOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));

        var maxRetries = Math.Max(1, options.MaxRetries);
        var baseMs = options.BaseDelayMilliseconds;
        var capMs = options.MaxDelayMilliseconds;
        var jitterMs = options.JitterMilliseconds;

        var delays = Enumerable.Range(1, maxRetries).Select(attempt =>
        {
            var exponential = baseMs * (int)Math.Pow(2, attempt - 1);
            var delay = Math.Min(capMs, exponential);
            var jitter = Random.Shared.Next(0, jitterMs);
            return TimeSpan.FromMilliseconds(delay + jitter);
        }).ToArray();

        return Policy<Result<Article>>
            .HandleResult(r => r.Failure && r.ErrorCode == ResultErrorCode.Concurrency)
            .WaitAndRetryAsync(delays, onRetryAsync: async (outcome, timespan, retryCount, context) =>
            {
                // expose retryCount in the context for metrics
                context["retryCount"] = (int)retryCount;

                // Handler will provide an onRetryAction in the Polly Context to reload state
                if (context.TryGetValue("onRetryAction", out var obj) && obj is Func<Task> action)
                {
                    await action();
                }
            });
    }
}
