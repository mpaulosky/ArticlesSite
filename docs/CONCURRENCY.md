# Optimistic Concurrency & Retry Policy

This document describes the centralized Polly retry policy used for optimistic concurrency when updating `Article` entities, where updates are performed with a version check (optimistic locking).

## Overview

- Updates are performed by `ArticleRepository.UpdateArticle(Article post)` and use a filter on `Id` + `Version`.
- When the repository detects a mismatch (no document matched the id+version), it returns a typed failure with `ResultErrorCode.Concurrency` and a structured `Details` payload (`ConcurrencyConflictInfo`) that includes the server's current version, a snapshot of the server `ArticleDto`, and a list of changed fields.
- The `EditArticle.Handler` uses a centralized Polly policy to automatically retry when a concurrency conflict occurs. On each retry the handler reloads the latest article, reapplies the user's changes, and tries again. If retries are exhausted the handler returns the typed concurrency failure (with `Details`).

## Centralized Policy

- The policy factory is `Web.Infrastructure.ConcurrencyPolicies.CreatePolicy(ConcurrencyOptions)`.
- The policy is registered once in DI and provided as `IAsyncPolicy<Result<Article>>` so handlers can consume it rather than building per-call policies.
- Registration example (done in `Program.cs`):

```csharp
// Concurrency options from configuration
builder.Services.Configure<Web.Infrastructure.ConcurrencyOptions>(builder.Configuration.GetSection("ConcurrencyOptions"));

// Register centralized Polly policy
builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ConcurrencyOptions>>().Value;
    return ConcurrencyPolicies.CreatePolicy(options);
});
```

## Configuration (defaults)

The retry behavior is configurable via `ConcurrencyOptions` (registered in DI). Default values are:

- `MaxRetries`: 3
- `BaseDelayMilliseconds`: 100
- `MaxDelayMilliseconds`: 2000
- `JitterMilliseconds`: 100

Example `appsettings.json` snippet to override:

```json
"ConcurrencyOptions": {
  "MaxRetries": 5,
  "BaseDelayMilliseconds": 200,
  "MaxDelayMilliseconds": 5000,
  "JitterMilliseconds": 150
}
```

## How it behaves

- The policy retries when `Result.ErrorCode == ResultErrorCode.Concurrency`.
- Delay for attempt N is `min(MaxDelayMilliseconds, BaseDelayMilliseconds * 2^(N-1)) + jitter`.
- On each retry the handler reloads the latest article and reapplies user changes. This makes the retry safe for common races.
- If the handler cannot reload the article (e.g., it was deleted), the handler returns the terminal failure immediately.

## API & UI implications

- The minimal API `PUT /api/articles/{id}` returns HTTP 409 with a concrete `ConcurrencyConflictResponseDto` when the operation results in a concurrency conflict. The DTO includes `serverVersion`, `serverArticle`, and `changedFields` to help clients present a merge/refresh UI.
- The Blazor edit page displays a conflict panel with `Reload Latest`, `Force Overwrite`, and `Cancel` actions when a concurrency conflict is returned.

## Extensibility

- The centralized policy is registered as an `IAsyncPolicy<Result<Article>>`. You can swap this implementation for a different policy, add logging hooks, or use policy wrap with additional resilience features.
- To change policy behavior globally, update `ConcurrencyOptions` or replace the registered policy in DI.

## Files to review

- `src/Web/Infrastructure/ConcurrencyOptions.cs` — configurable options.
- `src/Web/Infrastructure/ConcurrencyPolicies.cs` — centralized Polly policy factory.
- `src/Web/Data/Repositories/ArticleRepository.cs` — optimistic concurrency implementation and `ConcurrencyConflictInfo` details.
- `src/Web/Components/Features/Articles/ArticleEdit/EditArticle.cs` — handler consuming the centralized policy (passes an `onRetryAction` in Polly Context to reload state).
- `src/Web/Program.cs` — DI registration and minimal API mapping that returns 409 with `ConcurrencyConflictResponseDto`.

If you want, I can:
- Register the policy as a named Polly policy and expose it via `IAsyncPolicyResolver`.
- Add an OpenAPI example object for the `ConcurrencyConflictResponseDto` in the API docs.
- Add Playwright E2E tests that exercise the full conflict flow in a real browser.

