## 2026-03-26: ServiceDefaults must not reference AppHost packages
**By:** Matthew (mpaulosky) — resolved by Squad
**What:** `src/ServiceDefaults/ServiceDefaults.csproj` MUST NOT reference `Aspire.Hosting.AppHost`, `Aspire.Hosting.MongoDB`, or `Aspire.Hosting.Redis`. These are AppHost SDK concerns only. ServiceDefaults is a shared telemetry/health library — it only needs standard Microsoft.Extensions.* packages.
**Why:** These incorrect references caused NU1605 package downgrade conflicts (MongoDB.Driver 3.5.2 vs required 3.6.0, StackExchange.Redis 2.10.1 vs required 2.11.0) and a NU1010 error for the non-existent `Aspire.Hosting.AppHost` NuGet package.

## 2026-03-26: Architecture test allowedComponentNames must be kept current
**By:** Matthew (mpaulosky) — resolved by Squad
**What:** When a new Razor component with `Component` suffix is added to `Web.Components`, it MUST be added to `allowedComponentNames` in `Architecture.Tests/NamingConventionTests.cs`. Otherwise the architecture test fails.
**Why:** Added `RecentComponent` to allowed list after it was added to `Web.Components` without updating the test.

## 2026-03-26: Article loading and logging moved from Home to RecentComponent
**By:** Matthew (mpaulosky) — resolved by Squad
**What:** `Home.razor` no longer handles article loading or logging. This was moved to `RecentComponent`. Tests that verify article loading behavior (logging on failure/error) must test `RecentComponent`'s `ILogger<RecentComponent>`, not `ILogger<Home>`.
**Why:** Fixes `HomePage_LogsWarning_WhenHandlerFails` and `HomePage_LogsError_WhenHandlerThrows` which were checking the wrong logger type.
