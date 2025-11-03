# Smoke Tests vs Full E2E Tests

## Overview

The E2E test suite is designed to intelligently handle scenarios where the AppHost/web server is not available. Tests are categorized into two types:

1. **Smoke Tests** - Always run regardless of server availability
2. **Full E2E Tests** - Only run when the server is available

## How It Works

### Server Availability Check

The `PlaywrightTestBase` class automatically checks if the server is available at the configured `BaseUrl` (default: `http://localhost:5057`) before initializing browser resources.

- **Server Available**: Full E2E tests initialize browser and run normally
- **Server Unavailable**: Full E2E tests fail quickly with connection errors, smoke tests continue to run

### Creating Smoke Tests

To create a smoke test class:

```csharp
public class MySmokeTests : PlaywrightTestBase
{
    /// <summary>
    /// Mark this class as containing smoke tests
    /// </summary>
    protected override bool IsSmokeTest => true;

    [Fact]
    [SmokeTest] // Optional: adds trait for filtering
    public void MyTest()
    {
        // Test logic that doesn't require server
    }
}
```

### Running Tests

**Run all tests (including smoke tests):**
```bash
dotnet test
```

**Run only smoke tests:**
```bash
dotnet test --filter "FullyQualifiedName~SmokeTests"
```

**Filter by trait:**
```bash
dotnet test --filter "Category=SmokeTest"
```

## Benefits

1. **CI/CD Flexibility**: Run smoke tests in environments where the full application isn't deployed
2. **Fast Feedback**: Smoke tests verify test infrastructure without waiting for server startup
3. **Graceful Degradation**: Full E2E tests fail quickly if server is unavailable, rather than hanging
4. **Developer Experience**: Tests provide clear feedback about server availability

## Configuration

The base URL can be configured via environment variable:

```bash
$env:BASE_URL = "http://localhost:5000"
dotnet test
```

Or in `appsettings.json`:

```json
{
  "WebAppBaseUrl": "http://localhost:5057"
}
```

## Examples

### Example Smoke Test

```csharp
[Fact]
[SmokeTest]
public void ShouldBeAbleToRunABasicTest()
{
    // Verify test infrastructure works
    true.Should().BeTrue();
}

[Fact]
[SmokeTest]
public async Task ShouldBeAbleToCheckServerAvailability()
{
    // Check if server is available (doesn't fail if it's not)
    var isAvailable = await IsServerAvailableAsync(BaseUrl);
    _ = isAvailable; // Use the result
    true.Should().BeTrue();
}
```

### Example Full E2E Test

```csharp
public class HomeTests : PlaywrightTestBase
{
    // IsSmokeTest is false by default
    
    [Fact]
    public async Task ShouldLoadHomePageSuccessfully()
    {
        var homePage = new HomePage(Page);
        await homePage.GotoAsync();
        
        homePage.GetCurrentUrl().Should().Contain("/");
    }
}
```
