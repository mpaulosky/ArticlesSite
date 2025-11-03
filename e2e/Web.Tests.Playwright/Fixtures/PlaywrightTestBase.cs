namespace Web.Tests.Playwright.Fixtures;

/// <summary>
/// Base class for Playwright tests providing common setup and teardown
/// </summary>
[ExcludeFromCodeCoverage]
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

	/// <summary>
	/// Cached result of server availability check to avoid multiple checks
	/// </summary>
	private static bool? _isServerAvailable;

	private static readonly object _lock = new();

	/// <summary>
	/// Checks if the server is available at the base URL
	/// </summary>
	protected static async Task<bool> IsServerAvailableAsync(string baseUrl)
	{
		lock (_lock)
		{
			if (_isServerAvailable.HasValue)
			{
				return _isServerAvailable.Value;
			}
		}

		try
		{
			using var httpClient = new HttpClient();
			httpClient.Timeout = TimeSpan.FromSeconds(5);
			var response = await httpClient.GetAsync(baseUrl);
			var isAvailable = response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound;

			lock (_lock)
			{
				_isServerAvailable = isAvailable;
			}

			return isAvailable;
		}
		catch
		{
			lock (_lock)
			{
				_isServerAvailable = false;
			}

			return false;
		}
	}

	/// <summary>
	/// Determines if the test is a smoke test (always runs regardless of server availability)
	/// Override this in derived classes to specify test type
	/// </summary>
	protected virtual bool IsSmokeTest => false;

	public async Task InitializeAsync()
	{
		// Smoke tests always initialize (they don't need the browser/server)
		if (IsSmokeTest)
		{
			return;
		}

		// For non-smoke tests, check if server is available
		var serverAvailable = await IsServerAvailableAsync(BaseUrl);

		if (!serverAvailable)
		{
			// Mark as inconclusive - test cannot proceed without server
			Assert.Fail("Server is not available. Cannot run E2E tests without AppHost running.");

			return;
		}

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