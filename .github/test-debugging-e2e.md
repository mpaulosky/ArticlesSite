# End-to-End (E2E) Testing Guide

## Overview

This guide covers end-to-end testing for solutions scaffolded by the solution-scaffolder skill. E2E tests validate complete user workflows by simulating real user interactions with the application through a browser.

## Test Framework

E2E tests use:
- **Playwright** for browser automation
- **xUnit** for test execution
- **FluentAssertions** for assertions
- **TestContainers** for running the full application stack

## Project Structure

E2E tests are organized by user journey in the `tests/{SolutionName}.Tests.E2E/` project:

```
tests/
└── YourSolution.Tests.E2E/
    ├── Journeys/
    │   ├── ArticleManagement/
    │   │   ├── CreateArticleJourneyTests.cs
    │   │   ├── EditArticleJourneyTests.cs
    │   │   └── DeleteArticleJourneyTests.cs
    │   ├── UserAuthentication/
    │   │   ├── LoginJourneyTests.cs
    │   │   └── RegistrationJourneyTests.cs
    │   └── CommentManagement/
    │       └── CreateCommentJourneyTests.cs
    ├── PageObjects/
    │   ├── ArticlesPage.cs
    │   ├── CreateArticlePage.cs
    │   └── LoginPage.cs
    ├── Infrastructure/
    │   ├── E2ETestBase.cs
    │   └── PlaywrightFixture.cs
    └── GlobalUsings.cs
```

## Setting Up E2E Tests

### Install Playwright

```bash
# Install Playwright package
dotnet add package Microsoft.Playwright

# Install Playwright browsers
pwsh bin/Debug/net10.0/playwright.ps1 install
```

### Base Test Class

Create a base class for E2E tests:

```csharp
namespace YourSolution.Tests.E2E.Infrastructure;

/// <summary>
/// Base class for E2E tests providing browser and application setup
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
	protected IPlaywright Playwright { get; private set; } = null!;
	protected IBrowser Browser { get; private set; } = null!;
	protected IBrowserContext Context { get; private set; } = null!;
	protected IPage Page { get; private set; } = null!;
	protected string BaseUrl { get; private set; } = null!;

	private WebApplicationFactory<Program>? _factory;
	private MongoDbContainer? _mongoContainer;

	public async Task InitializeAsync()
	{
		// Start MongoDB container
		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:7.0")
			.WithPortBinding(27017, true)
			.Build();
		
		await _mongoContainer.StartAsync();

		// Start web application
		_factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.ConfigureTestServices(services =>
				{
					var descriptor = services.SingleOrDefault(
						d => d.ServiceType == typeof(IMongoClient));
					
					if (descriptor is not null)
					{
						services.Remove(descriptor);
					}
					
					services.AddSingleton<IMongoClient>(sp =>
						new MongoClient(_mongoContainer.GetConnectionString()));
				});
				
				builder.UseUrls("http://localhost:5050");
			});

		var client = _factory.CreateClient();
		BaseUrl = client.BaseAddress!.ToString().TrimEnd('/');

		// Initialize Playwright
		Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
		
		Browser = await Playwright.Chromium.LaunchAsync(new()
		{
			Headless = true, // Set to false for debugging
			SlowMo = 0 // Slow down operations for debugging (milliseconds)
		});

		Context = await Browser.NewContextAsync(new()
		{
			ViewportSize = new() { Width = 1920, Height = 1080 },
			RecordVideoDir = "videos/" // Record videos for debugging
		});

		Page = await Context.NewPageAsync();
	}

	public async Task DisposeAsync()
	{
		await Page?.CloseAsync()!;
		await Context?.CloseAsync()!;
		await Browser?.CloseAsync()!;
		Playwright?.Dispose();
		_factory?.Dispose();
		
		if (_mongoContainer is not null)
		{
			await _mongoContainer.DisposeAsync();
		}
	}

	protected async Task NavigateToAsync(string path)
	{
		await Page.GotoAsync($"{BaseUrl}{path}");
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	protected async Task<string> TakeScreenshotAsync(string name)
	{
		var path = $"screenshots/{name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
		await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
		return path;
	}
}
```

### Page Object Model

Create page objects for better test maintainability:

```csharp
namespace YourSolution.Tests.E2E.PageObjects;

/// <summary>
/// Page object for the Create Article page
/// </summary>
public sealed class CreateArticlePage
{
	private readonly IPage _page;

	public CreateArticlePage(IPage page)
	{
		_page = page;
	}

	// Locators
	private ILocator TitleInput => _page.Locator("input[name='title']");
	private ILocator ContentInput => _page.Locator("textarea[name='content']");
	private ILocator SubmitButton => _page.Locator("button[type='submit']");
	private ILocator ErrorMessage => _page.Locator(".error-message");
	private ILocator SuccessMessage => _page.Locator(".success-message");

	// Actions
	public async Task FillTitleAsync(string title)
	{
		await TitleInput.FillAsync(title);
	}

	public async Task FillContentAsync(string content)
	{
		await ContentInput.FillAsync(content);
	}

	public async Task ClickSubmitAsync()
	{
		await SubmitButton.ClickAsync();
	}

	public async Task CreateArticleAsync(string title, string content)
	{
		await FillTitleAsync(title);
		await FillContentAsync(content);
		await ClickSubmitAsync();
	}

	// Assertions
	public async Task ShouldShowSuccessMessageAsync()
	{
		await SuccessMessage.WaitForAsync(new() { State = WaitForSelectorState.Visible });
		(await SuccessMessage.IsVisibleAsync()).Should().BeTrue();
	}

	public async Task ShouldShowErrorMessageAsync(string expectedMessage)
	{
		await ErrorMessage.WaitForAsync(new() { State = WaitForSelectorState.Visible });
		var text = await ErrorMessage.TextContentAsync();
		text.Should().Contain(expectedMessage);
	}

	public async Task ShouldHaveTitleAsync(string expectedTitle)
	{
		var value = await TitleInput.InputValueAsync();
		value.Should().Be(expectedTitle);
	}
}
```

## Writing E2E Tests

### User Journey Tests

```csharp
namespace YourSolution.Tests.E2E.Journeys.ArticleManagement;

/// <summary>
/// E2E tests for creating articles
/// </summary>
public sealed class CreateArticleJourneyTests : E2ETestBase
{
	[Fact]
	public async Task CreateArticle_ValidData_ArticleCreatedSuccessfully()
	{
		// Arrange
		await NavigateToAsync("/articles/create");
		var createPage = new CreateArticlePage(Page);

		// Act
		await createPage.CreateArticleAsync(
			title: "My First E2E Article",
			content: "This article was created by an E2E test"
		);

		// Assert
		await createPage.ShouldShowSuccessMessageAsync();
		
		// Verify navigation to article list
		Page.Url.Should().Contain("/articles");
		
		// Verify article appears in list
		var articleTitle = Page.Locator("text=My First E2E Article");
		await articleTitle.WaitForAsync(new() { State = WaitForSelectorState.Visible });
		(await articleTitle.IsVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_EmptyTitle_ShowsValidationError()
	{
		// Arrange
		await NavigateToAsync("/articles/create");
		var createPage = new CreateArticlePage(Page);

		// Act
		await createPage.CreateArticleAsync(
			title: "",
			content: "Content without title"
		);

		// Assert
		await createPage.ShouldShowErrorMessageAsync("Title is required");
		
		// Verify still on create page
		Page.Url.Should().Contain("/articles/create");
	}

	[Fact]
	public async Task CreateArticle_CancelButton_NavigatesBackToList()
	{
		// Arrange
		await NavigateToAsync("/articles/create");

		// Act
		await Page.Locator("button:has-text('Cancel')").ClickAsync();

		// Assert
		Page.Url.Should().Contain("/articles");
		Page.Url.Should().NotContain("/create");
	}
}
```

### Testing User Interactions

```csharp
[Fact]
public async Task ArticleList_ClickEdit_NavigatesToEditPage()
{
	// Arrange - Create an article first
	await CreateTestArticleAsync("Article to Edit", "Content");
	await NavigateToAsync("/articles");

	// Act
	await Page.Locator("text=Article to Edit")
		.Locator("..")
		.Locator("button:has-text('Edit')")
		.ClickAsync();

	// Assert
	await Page.WaitForURLAsync("**/articles/*/edit");
	Page.Url.Should().Contain("/articles/");
	Page.Url.Should().Contain("/edit");
	
	var editPage = new EditArticlePage(Page);
	await editPage.ShouldHaveTitleAsync("Article to Edit");
}

[Fact]
public async Task ArticleDetails_ClickDelete_ShowsConfirmation()
{
	// Arrange
	await CreateTestArticleAsync("Article to Delete", "Content");
	await NavigateToAsync("/articles");
	await Page.Locator("text=Article to Delete").ClickAsync();

	// Act
	await Page.Locator("button:has-text('Delete')").ClickAsync();

	// Assert
	var confirmDialog = Page.Locator(".confirmation-dialog");
	await confirmDialog.WaitForAsync(new() { State = WaitForSelectorState.Visible });
	
	var dialogText = await confirmDialog.TextContentAsync();
	dialogText.Should().Contain("Are you sure you want to delete");
}
```

### Testing Forms and Validation

```csharp
[Fact]
public async Task CreateArticle_ValidationErrors_DisplayedInline()
{
	// Arrange
	await NavigateToAsync("/articles/create");

	// Act - Submit form with multiple validation errors
	await Page.Locator("input[name='title']").FillAsync("");
	await Page.Locator("textarea[name='content']").FillAsync("ab"); // Too short
	await Page.Locator("button[type='submit']").ClickAsync();

	// Assert
	var titleError = Page.Locator(".field-validation-error:near(input[name='title'])");
	await titleError.WaitForAsync(new() { State = WaitForSelectorState.Visible });
	(await titleError.TextContentAsync()).Should().Contain("Title is required");

	var contentError = Page.Locator(".field-validation-error:near(textarea[name='content'])");
	await contentError.WaitForAsync(new() { State = WaitForSelectorState.Visible });
	(await contentError.TextContentAsync()).Should().Contain("minimum length");
}
```

## Running E2E Tests

### Command Line

```bash
# Run all E2E tests
dotnet test tests/YourSolution.Tests.E2E/

# Run with headed browser (visible) for debugging
HEADED=true dotnet test tests/YourSolution.Tests.E2E/

# Run specific browser
dotnet test tests/YourSolution.Tests.E2E/ -- Playwright.BrowserName=firefox

# Run with video recording
dotnet test tests/YourSolution.Tests.E2E/ -- Playwright.RecordVideo=true

# Run with slow motion (500ms delay between actions)
dotnet test tests/YourSolution.Tests.E2E/ -- Playwright.SlowMo=500
```

### Visual Studio

1. Open **Test Explorer**
2. Run E2E tests (they will appear slower than unit tests)
3. Check `videos/` folder for recordings if enabled
4. Check `screenshots/` folder for debug screenshots

### CI/CD Pipeline

```yaml
name: E2E Tests

on:
  pull_request:
  push:
    branches: [main]

jobs:
  e2e-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Setup Playwright
      run: |
        dotnet build tests/YourSolution.Tests.E2E/
        pwsh tests/YourSolution.Tests.E2E/bin/Debug/net10.0/playwright.ps1 install --with-deps
    
    - name: Run E2E tests
      run: dotnet test tests/YourSolution.Tests.E2E/ --logger trx
    
    - name: Upload test videos
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: e2e-test-videos
        path: videos/**/*.webm
    
    - name: Upload screenshots
      if: failure()
      uses: actions/upload-artifact@v4
      with:
        name: e2e-test-screenshots
        path: screenshots/**/*.png
```

## Debugging E2E Tests

### Visual Debugging

Run tests with visible browser:

```csharp
Browser = await Playwright.Chromium.LaunchAsync(new()
{
	Headless = false, // Show browser window
	SlowMo = 1000     // Slow down by 1 second per action
});
```

### Taking Screenshots

```csharp
[Fact]
public async Task CreateArticle_DebugTest()
{
	await NavigateToAsync("/articles/create");
	
	// Take screenshot at any point
	await Page.ScreenshotAsync(new() { Path = "debug1.png" });
	
	await Page.Locator("input[name='title']").FillAsync("Test");
	
	// Take another screenshot
	await Page.ScreenshotAsync(new() { Path = "debug2.png", FullPage = true });
}
```

### Using Playwright Inspector

```bash
# Run with Playwright Inspector
PWDEBUG=1 dotnet test tests/YourSolution.Tests.E2E/ --filter "FullyQualifiedName~CreateArticleJourneyTests"
```

This opens a GUI where you can:
- Step through test actions
- Inspect elements
- Try selectors
- View page state

### Viewing Test Traces

Record traces for debugging:

```csharp
Context = await Browser.NewContextAsync(new()
{
	RecordVideoDir = "videos/",
	RecordHarPath = "har/network.har", // Record network traffic
});

await Context.Tracing.StartAsync(new()
{
	Screenshots = true,
	Snapshots = true,
	Sources = true
});

// Run test...

await Context.Tracing.StopAsync(new()
{
	Path = "trace.zip"
});
```

View traces:

```bash
playwright show-trace trace.zip
```

## Best Practices

### 1. Use Page Object Model

```csharp
// Good - Encapsulated in page object
var loginPage = new LoginPage(Page);
await loginPage.LoginAsync("user@example.com", "password");

// Avoid - Direct locators in test
await Page.Locator("input[name='email']").FillAsync("user@example.com");
await Page.Locator("input[name='password']").FillAsync("password");
await Page.Locator("button[type='submit']").ClickAsync();
```

### 2. Wait for Network to Be Idle

```csharp
await Page.GotoAsync(url);
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
```

### 3. Use Meaningful Selectors

```csharp
// Good - Semantic selectors
await Page.Locator("button[data-testid='submit-article']").ClickAsync();
await Page.Locator("text=Create Article").ClickAsync();

// Avoid - Fragile selectors
await Page.Locator("body > div:nth-child(3) > button:nth-child(1)").ClickAsync();
```

### 4. Test User Journeys, Not Implementation

```csharp
// Good - Tests user workflow
[Fact]
public async Task UserCanCreateAndEditArticle()
{
	await CreateArticleAsync("My Article", "Content");
	await EditArticleAsync("Updated Title", "Updated Content");
	await VerifyArticleDisplays("Updated Title", "Updated Content");
}

// Avoid - Tests internal implementation
[Fact]
public async Task ApiEndpointReturns200()
{
	// Use integration tests for API testing
}
```

### 5. Clean State Between Tests

```csharp
public async Task InitializeAsync()
{
	// ... setup code
	
	// Clean database before each test
	await CleanDatabaseAsync();
}

private async Task CleanDatabaseAsync()
{
	var client = new MongoClient(_mongoContainer!.GetConnectionString());
	var database = client.GetDatabase("test-db");
	await database.DropAsync();
}
```

## Common Issues and Solutions

### Issue: Element Not Found

**Cause**: Element not yet rendered or wrong selector  
**Solution**: Wait for element and use better selectors

```csharp
// Add explicit wait
await Page.Locator("button[data-testid='submit']")
	.WaitForAsync(new() { State = WaitForSelectorState.Visible });

// Or use built-in waits
await Page.Locator("button[data-testid='submit']").ClickAsync(); // Auto-waits
```

### Issue: Flaky Tests

**Cause**: Timing issues or async operations  
**Solution**: Use proper waits

```csharp
// Bad - Hard-coded delay
await Task.Delay(2000);

// Good - Wait for specific condition
await Page.WaitForSelectorAsync(".success-message", new() { State = WaitForSelectorState.Visible });
```

### Issue: Tests Pass Locally, Fail in CI

**Cause**: Different browser versions or screen sizes  
**Solution**: Lock browser version and set explicit viewport

```csharp
Context = await Browser.NewContextAsync(new()
{
	ViewportSize = new() { Width = 1920, Height = 1080 },
	UserAgent = "Mozilla/5.0..." // Set specific user agent
});
```

### Issue: Authentication Required

**Cause**: Tests need to be authenticated  
**Solution**: Create helper for authentication

```csharp
protected async Task LoginAsync(string email, string password)
{
	await Page.GotoAsync($"{BaseUrl}/login");
	await Page.Locator("input[name='email']").FillAsync(email);
	await Page.Locator("input[name='password']").FillAsync(password);
	await Page.Locator("button[type='submit']").ClickAsync();
	await Page.WaitForURLAsync($"{BaseUrl}/");
}

[Fact]
public async Task CreateArticle_WhenAuthenticated_Succeeds()
{
	await LoginAsync("test@example.com", "password");
	await NavigateToAsync("/articles/create");
	// ... rest of test
}
```

## Performance Optimization

### Reuse Browser Context

```csharp
// Share browser across test class
public sealed class ArticleJourneyTests : IClassFixture<PlaywrightFixture>
{
	private readonly PlaywrightFixture _fixture;

	public ArticleJourneyTests(PlaywrightFixture fixture)
	{
		_fixture = fixture;
	}
}
```

### Run Tests in Parallel (with caution)

```csharp
// Disable parallel execution for E2E tests if they share state
[Collection("E2E Tests Sequential")]
public sealed class CreateArticleJourneyTests : E2ETestBase
{
	// Tests run one at a time
}
```

## Additional Resources

- [Playwright for .NET Documentation](https://playwright.dev/dotnet/)
- [Playwright Best Practices](https://playwright.dev/dotnet/docs/best-practices)
- [Page Object Model Pattern](https://playwright.dev/dotnet/docs/pom)
- [Debugging Tests](https://playwright.dev/dotnet/docs/debug)
