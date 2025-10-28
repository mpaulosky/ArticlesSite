# Quick Start Guide - C# Playwright Tests

## ✅ Conversion Complete!

Your Web.Tests.Playwright project has been successfully converted to C# with xUnit. All 52 tests from the TypeScript version have been migrated.

## 🚀 Getting Started (3 Steps)

### 1. Build the Project
```powershell
cd tests\Web.Tests.Playwright
dotnet build
```

### 2. Install Playwright Browsers (One-time setup)
```powershell
# After building, run this from the project directory:
pwsh bin\Debug\net9.0\playwright.ps1 install

# Or install just Chromium:
pwsh bin\Debug\net9.0\playwright.ps1 install chromium
```

### 3. Run the Tests
```powershell
# Make sure your application is running first!
# In another terminal:
dotnet run --project src\AppHost

# Then run the tests:
dotnet test
```

## 📊 Test Summary

**Total Tests Converted: 52**

- ✅ SmokeTests: 1 test
- ✅ HomeTests: 7 tests
- ✅ AboutTests: 7 tests
- ✅ ContactTests: 7 tests
- ✅ ArticlesListTests: 9 tests
- ✅ CategoriesListTests: 9 tests
- ✅ ErrorTests: 6 tests
- ✅ NavigationTests: 4 tests
- ✅ FooterTests: 3 tests

## 📁 Project Structure

```
Web.Tests.Playwright/
├── PageObjects/          # Page Object Models (7 files)
│   ├── BasePage.cs
│   ├── HomePage.cs
│   ├── AboutPage.cs
│   ├── ContactPage.cs
│   ├── ArticlesListPage.cs
│   ├── CategoriesListPage.cs
│   └── ErrorPage.cs
├── Tests/                # Test Classes (9 files)
│   ├── SmokeTests.cs
│   ├── HomeTests.cs
│   ├── AboutTests.cs
│   ├── ContactTests.cs
│   ├── ArticlesListTests.cs
│   ├── CategoriesListTests.cs
│   ├── ErrorTests.cs
│   ├── NavigationTests.cs
│   └── FooterTests.cs
├── Fixtures/
│   └── PlaywrightTestBase.cs   # Base test class
├── GlobalUsings.cs
├── Web.Tests.Playwright.csproj
├── README.md               # Full documentation
├── MIGRATION.md            # Detailed migration notes
└── QUICKSTART.md          # This file
```

## 🔧 Common Commands

```powershell
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~HomeTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# List all tests
dotnet test --list-tests

# Run in parallel
dotnet test --parallel

# Set custom base URL
$env:BASE_URL = "http://localhost:8080"
dotnet test
```

## 🐛 Debugging in Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Right-click any test → Debug
4. Set breakpoints in your test code
5. Step through with F10/F11

To see the browser while debugging, edit `Fixtures\PlaywrightTestBase.cs`:
```csharp
_browser = await _playwright.Chromium.LaunchAsync(new()
{
    Headless = false,  // Changed from true
    SlowMo = 100       // Slow down to see actions
});
```

## 📖 Documentation

- **README.md** - Complete documentation with examples
- **MIGRATION.md** - Detailed migration information
- [Playwright for .NET Docs](https://playwright.dev/dotnet/)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Docs](https://fluentassertions.com/)

## 💡 Tips

### Running Tests Without Starting App Manually
You can configure the base URL in your test setup or use environment variables:
```powershell
$env:BASE_URL = "http://localhost:5000"
dotnet test
```

### Adding New Tests
1. Create a new test class extending `PlaywrightTestBase`
2. Create page objects as needed in `PageObjects/`
3. Use FluentAssertions for assertions
4. Follow existing naming conventions

Example:
```csharp
public class MyNewTests : PlaywrightTestBase
{
    [Fact]
    public async Task ShouldDoSomething()
    {
        var page = new HomePage(Page);
        await page.GotoAsync();
        
        var heading = await page.GetHeadingTextAsync();
        heading.Should().NotBeNullOrEmpty();
    }
}
```

### Browser Selection
Default is Chromium. To use Firefox or WebKit, modify `PlaywrightTestBase.cs`:
```csharp
// Use Firefox
_browser = await _playwright.Firefox.LaunchAsync(...);

// Use WebKit (Safari)
_browser = await _playwright.Webkit.LaunchAsync(...);
```

## ❓ Troubleshooting

### "Playwright driver not found"
**Solution:** Run the browser install command after building:
```powershell
pwsh bin\Debug\net9.0\playwright.ps1 install
```

### "Connection refused" or timeouts
**Solution:** Make sure your application is running:
```powershell
dotnet run --project src\AppHost
```

### Tests fail with "element not found"
**Solution:** The application might not be fully loaded. Playwright has built-in auto-waiting, but you may need to adjust timeouts in page objects if needed.

## 🎉 You're All Set!

The C# Playwright tests are ready to use. You can now manage and update them alongside your C# application code without needing TypeScript or Node.js knowledge.

For detailed information, see **README.md** in this directory.

---

**Need Help?** Check the README.md or MIGRATION.md files for more details.
