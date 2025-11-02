# Web.Tests.Playwright (C#)

End-to-end (E2E) tests for the ArticlesSite Web project using Playwright with C# and xUnit.

## Overview

This test project provides comprehensive E2E testing coverage for the Blazor Server web application. Tests are written in C# using Microsoft.Playwright and xUnit test framework, following the Page Object Model (POM) pattern for maintainability.

## Prerequisites

- **.NET 9.0 SDK** or higher
- **Playwright browsers** (installed via PowerShell command)
- **Running Web Application** - The application must be running before executing tests

## Getting Started

### 1. Install Playwright Browsers

First time setup requires installing Playwright browsers:

```powershell
# From the test project directory
pwsh bin/Debug/net9.0/playwright.ps1 install

# Or install with system dependencies
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps
```

Alternatively, after building the project:

```powershell
playwright install chromium
```

### 2. Start the Application

Before running tests, ensure the web application is running:

```powershell
# From the repository root
dotnet run --project src/AppHost
```

The application typically runs on `http://localhost:5000` by default.

### 3. Run Tests

```powershell
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~HomeTests"

# Run tests in parallel
dotnet test --parallel
```

## Project Structure

```
Web.Tests.Playwright/
├── PageObjects/                 # Page Object Models
│   ├── BasePage.cs              # Base page class
│   ├── HomePage.cs              # Home page POM
│   ├── AboutPage.cs             # About page POM
│   ├── ContactPage.cs           # Contact page POM
│   ├── ArticlesListPage.cs      # Articles list POM
│   ├── CategoriesListPage.cs    # Categories list POM
│   └── ErrorPage.cs             # Error page POM
├── Tests/                       # Test classes
│   ├── SmokeTests.cs            # Basic smoke tests
│   ├── HomeTests.cs             # Home page tests
│   ├── AboutTests.cs            # About page tests
│   ├── ContactTests.cs          # Contact page tests
│   ├── ArticlesListTests.cs     # Articles tests
│   ├── CategoriesListTests.cs   # Categories tests
│   ├── ErrorTests.cs            # Error page tests
│   ├── NavigationTests.cs       # Navigation component tests
│   └── FooterTests.cs           # Footer component tests
├── Fixtures/
│   └── PlaywrightTestBase.cs   # Base test class with setup/teardown
├── Web.Tests.Playwright.csproj  # Project file
├── GlobalUsings.cs              # Global using statements
└── README.md                    # This file
```

## Test Coverage

### Pages
- ✅ Home page - Layout, navigation, content
- ✅ About page - Content, navigation
- ✅ Contact page - Form, navigation
- ✅ Articles list - Display, search, filtering
- ✅ Categories list - Display, search, filtering
- ✅ Error pages - 404, error handling

### Components
- ✅ Navigation menu - Links, responsive behavior
- ✅ Footer - Display across pages, responsive
- ✅ Page headers - Consistent display
- ✅ Error alerts - Error message handling

### User Interactions
- ✅ Navigation between pages
- ✅ Form interactions (where applicable)
- ✅ Responsive behavior (mobile, tablet, desktop)
- ✅ Error handling and recovery

## Configuration

### Base URL

The base URL for tests can be configured via environment variable:

```powershell
$env:BASE_URL = "http://localhost:8080"
dotnet test
```

Default is `http://localhost:5000`.

### Browser Selection

By default, tests run in Chromium headless mode. You can modify the browser in `PlaywrightTestBase.cs`:

```csharp
// Use Firefox
_browser = await _playwright.Firefox.LaunchAsync(...);

// Use WebKit (Safari)
_browser = await _playwright.Webkit.LaunchAsync(...);

// Run headed mode for debugging
_browser = await _playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    SlowMo = 100  // Slow down by 100ms
});
```

## Writing Tests

### Basic Test Structure

Tests use xUnit and the Page Object Model pattern:

```csharp
using FluentAssertions;
using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class MyTests : PlaywrightTestBase
{
    [Fact]
    public async Task ShouldDoSomething()
    {
        var homePage = new HomePage(Page);
        await homePage.GotoAsync();

        var heading = await homePage.GetHeadingTextAsync();
        heading.Should().NotBeNullOrEmpty();
    }
}
```

### Creating New Page Objects

Extend `BasePage` for new pages:

```csharp
namespace Web.Tests.Playwright.PageObjects;

public class MyNewPage : BasePage
{
    private readonly ILocator _myElement;

    public MyNewPage(IPage page) : base(page)
    {
        _myElement = page.Locator("#my-element");
    }

    public override async Task GotoAsync(string path = "/")
    {
        await base.GotoAsync("/my-path");
    }

    public async Task<string> GetMyElementTextAsync()
    {
        return await _myElement.TextContentAsync() ?? string.Empty;
    }
}
```

## Debugging Tests

### Run Tests in Headed Mode

Modify `PlaywrightTestBase.cs` to run in headed mode:

```csharp
_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
{
    Headless = false,
    SlowMo = 100
});
```

### Visual Studio Integration

1. Open Test Explorer in Visual Studio
2. Right-click any test and select "Debug"
3. Set breakpoints in your test code
4. Use F10/F11 to step through code

### Take Screenshots

Add screenshots to your tests for debugging:

```csharp
await Page.ScreenshotAsync(new() { Path = "screenshot.png" });
```

### Video Recording

Enable video recording in `PlaywrightTestBase.cs`:

```csharp
Context = await _browser.NewContextAsync(new()
{
    BaseURL = BaseUrl,
    RecordVideoDir = "videos/",
    RecordVideoSize = new() { Width = 1280, Height = 720 }
});
```

## CI/CD Integration

### GitHub Actions

Example workflow for running tests in CI:

```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'

- name: Restore dependencies
  run: dotnet restore

- name: Build
  run: dotnet build --no-restore

- name: Install Playwright Browsers
  run: pwsh tests/Web.Tests.Playwright/bin/Debug/net9.0/playwright.ps1 install --with-deps
  
- name: Run Tests
  run: dotnet test tests/Web.Tests.Playwright/Web.Tests.Playwright.csproj --no-build
  env:
    BASE_URL: http://localhost:5000
```

## Best Practices

1. **Use Page Object Models** - Keep page structure separate from test logic
2. **Async/Await Properly** - Always await async operations
3. **Wait for Elements** - Use proper wait strategies instead of fixed timeouts
4. **Isolate Tests** - Each test should be independent
5. **Descriptive Names** - Use clear, descriptive test method names
6. **FluentAssertions** - Use FluentAssertions for readable assertions
7. **Test Real Scenarios** - Focus on user workflows, not implementation details

## Troubleshooting

### Tests Timeout

- Ensure the application is running before tests
- Check the `BASE_URL` configuration
- Increase timeout in individual test methods if needed

### Browser Installation Fails

Install browsers manually:

```powershell
playwright install chromium
```

### Tests Fail Inconsistently

- Check for race conditions
- Add proper wait conditions using `WaitForSelectorAsync`
- Ensure test isolation

### Port Already in Use

If the application port is already in use, set a different port:

```powershell
$env:BASE_URL = "http://localhost:8080"
dotnet test
```

## Comparison with TypeScript Version

This C# version provides the same functionality as the TypeScript version with these benefits:

- **Native .NET Integration** - Works seamlessly with other .NET test projects
- **Type Safety** - Strong typing with C# compiler checking
- **Familiar Tooling** - Use Visual Studio, Rider, or VS Code with C# extensions
- **Single Language** - No need to context-switch between TypeScript and C#
- **Better Debugging** - Full Visual Studio debugging support

## Resources

- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Page Object Model Pattern](https://playwright.dev/dotnet/docs/pom)

## Contributing

When adding new tests:

1. Create appropriate Page Object Models in `PageObjects/`
2. Write test classes in `Tests/` extending `PlaywrightTestBase`
3. Follow existing naming conventions
4. Use FluentAssertions for assertions
5. Ensure tests pass locally before committing
6. Update this README if adding significant functionality

## License

This project is part of ArticlesSite and follows the same MIT License.
