namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// About Page Object Model
/// </summary>
[ExcludeFromCodeCoverage]
public class AboutPage : BasePage
{
	private readonly ILocator _pageHeading;
	private readonly ILocator _pageContent;

	public AboutPage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_pageContent = page.Locator("main, article, .content");
	}

	/// <summary>
	/// Navigate to about page
	/// </summary>
	public override async Task GotoAsync(string path = "/")
	{
		await base.GotoAsync("/about");
	}

	/// <summary>
	/// Get the page heading text
	/// </summary>
	public async Task<string> GetHeadingTextAsync()
	{
		return await _pageHeading.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Check if page content is visible
	/// </summary>
	public async Task<bool> HasContentAsync()
	{
		return await _pageContent.IsVisibleAsync();
	}
}