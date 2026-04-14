// Removed redundant usings: common namespaces are included in Web/GlobalUsings.cs
// Polly.Retry types (RetryStrategyOptions) are globally included via Web/GlobalUsings.cs

namespace Web.Infrastructure;

/// <summary>
/// Factory for concurrency retry pipelines used by handlers that update articles.
/// Centralizes the Polly v8 ResiliencePipeline creation so it can be reused or swapped for testing/DI.
/// </summary>
public static class ConcurrencyPolicies
{
	/// <summary>Typed context key for the action to invoke on each retry.</summary>
	public static readonly ResiliencePropertyKey<Func<Task>> OnRetryActionKey = new("onRetryAction");

	/// <summary>Typed context key for tracking the retry attempt count.</summary>
	public static readonly ResiliencePropertyKey<int> RetryCountKey = new("retryCount");

	/// <summary>
	/// Creates a generic Polly resilience pipeline that retries on optimistic concurrency failures (<see cref="ResultErrorCode.Concurrency"/>).
	/// Uses exponential backoff with jitter based on the provided options.
	/// </summary>
	public static ResiliencePipeline<Result<T>> CreatePolicy<T>(ConcurrencyOptions options) where T : class
	{
		if (options is null) throw new ArgumentNullException(nameof(options));

		var maxRetries = options.MaxRetries;
		var baseMs = options.BaseDelayMilliseconds;
		var capMs = options.MaxDelayMilliseconds;
		var jitterMs = options.JitterMilliseconds;

		if (maxRetries <= 0)
		{
			return new ResiliencePipelineBuilder<Result<T>>().Build();
		}

		var delays = Enumerable.Range(0, maxRetries).Select(i =>
		{
			var exponential = baseMs * (int)Math.Pow(2, i);
			var delay = Math.Min(capMs, exponential);
			var jitter = jitterMs > 0 ? Random.Shared.Next(0, jitterMs) : 0;
			return TimeSpan.FromMilliseconds(delay + jitter);
		}).ToArray();

		return new ResiliencePipelineBuilder<Result<T>>()
			.AddRetry(new RetryStrategyOptions<Result<T>>
			{
				ShouldHandle = new PredicateBuilder<Result<T>>()
					.HandleResult(r => r.Failure && r.ErrorCode == ResultErrorCode.Concurrency),
				MaxRetryAttempts = maxRetries,
				DelayGenerator = args =>
				{
					var idx = Math.Min(args.AttemptNumber, delays.Length - 1);
					return new ValueTask<TimeSpan?>(delays[idx]);
				},
				OnRetry = async args =>
				{
					args.Context.Properties.Set(RetryCountKey, args.AttemptNumber + 1);
					if (args.Context.Properties.TryGetValue(OnRetryActionKey, out var action) && action is not null)
						await action();
				}
			})
			.Build();
	}

	/// <summary>
	/// Creates a Polly resilience pipeline for Article concurrency retries.
	/// Delegates to the generic CreatePolicy method for backwards compatibility.
	/// </summary>
	public static ResiliencePipeline<Result<Article>> CreatePolicy(ConcurrencyOptions options)
	{
		return CreatePolicy<Article>(options);
	}
}
