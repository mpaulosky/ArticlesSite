namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Error Page Object Model
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorPage : BasePage
{
	private readonly ILocator _errorHeading;
	private readonly ILocator _errorMessage;
	private readonly ILocator _errorCode;
	private readonly ILocator _homeLink;
	private readonly ILocator _backButton;

	public ErrorPage(IPage page) : base(page)
	{
		_errorHeading = page.Locator("h1:has-text('Error'), h1:has-text('Not Found'), h1:has-text('404')");
		_errorMessage = page.Locator(".error-message, .error-description, p:below(h1)").First;
		_errorCode = page.Locator("text=/404|500|error/i");
		_homeLink = page.Locator("a[href='/']");
		_backButton = page.Locator("button:has-text('Back'), a:has-text('Go Back')");
	}

	/// <summary>
	/// Navigate to a non-existent page to trigger 404
	/// </summary>
	public async Task GotoNonExistentPageAsync()
	{
		await base.GotoAsync($"/this-page-does-not-exist-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
	}

	/// <summary>
	/// Get the error heading text
	/// </summary>
	public async Task<string> GetErrorHeadingTextAsync()
	{
		try
		{
			return await _errorHeading.TextContentAsync() ?? string.Empty;
		}
		catch
		{
			return string.Empty;
		}
	}

	/// <summary>
	/// Check if error page is displayed
	/// </summary>
	[Obsolete]
	public async Task<bool> IsErrorPageDisplayedAsync()
	{
		try
		{
			return await _errorHeading.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 }) ||
						 await _errorCode.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Check if home link is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasHomeLinkAsync()
	{
		try
		{
			return await _homeLink.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Click on home link to return to home page
	/// </summary>
	public async Task ClickHomeLinkAsync()
	{
		await _homeLink.First.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Click on back button
	/// </summary>
	[Obsolete]
	public async Task ClickBackButtonAsync()
	{
		if (await _backButton.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
		{
			await _backButton.ClickAsync();
			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		}
	}
}