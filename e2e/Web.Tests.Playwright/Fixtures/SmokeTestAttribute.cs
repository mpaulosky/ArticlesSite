namespace Web.Tests.Playwright.Fixtures;

/// <summary>
/// Marks a test as a smoke test that should always run regardless of server availability
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
[ExcludeFromCodeCoverage]
public sealed class SmokeTestAttribute : Attribute { }
