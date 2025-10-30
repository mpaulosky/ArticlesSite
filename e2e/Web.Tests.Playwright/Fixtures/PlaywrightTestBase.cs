namespace Web.Tests.Playwright.Fixtures;

/// <summary>
/// Base class for Playwright tests providing common setup and teardown
/// </summary>
public class PlaywrightTestBase : IAsyncLifetime
{

	private IPlaywright? _playwright;

	private IBrowser? _browser;

	protected IPage Page { get; private set; } = null!;

	protected IBrowserContext Context { get; private set; } = null!;

	/// <summary>
	/// Gets the base URL for tests from environment variable or uses default
	/// </summary>
	protected string BaseUrl => Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5057";

	public async Task InitializeAsync()
	{
		_playwright = await Microsoft.Playwright.Playwright.CreateAsync();

		// Launch browser (use Chromium by default)
		_browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
				Headless = true,

				// Set to false for debugging
				// Headless = false,
				// SlowMo = 100
		});

		// Create a new context with base URL
		Context = await _browser.NewContextAsync(new BrowserNewContextOptions
		{
				BaseURL = BaseUrl,
				ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
				ScreenSize = new ScreenSize { Width = 1280, Height = 720 },
		});

		// Create a new page
		Page = await Context.NewPageAsync();
	}

	public async Task DisposeAsync()
	{
		if (Page != null)
		{
			await Page.CloseAsync();
		}

		if (Context != null)
		{
			await Context.CloseAsync();
		}

		if (_browser != null)
		{
			await _browser.CloseAsync();
		}

		_playwright?.Dispose();
	}

	// Explicit interface implementations for xUnit v3 compatibility
	ValueTask IAsyncLifetime.InitializeAsync() => new(InitializeAsync());

	ValueTask IAsyncDisposable.DisposeAsync() => new(DisposeAsync());

}
