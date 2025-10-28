namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Home Page Object Model
/// </summary>
public class HomePage : BasePage
{
    private readonly ILocator _pageHeading;
    private readonly ILocator _welcomeMessage;
    private readonly ILocator _recentArticles;
    private readonly ILocator _featuredContent;

    public HomePage(IPage page) : base(page)
    {
        _pageHeading = page.Locator("h1, h2").First;
        _welcomeMessage = page.Locator("text=/welcome/i").First;
        _recentArticles = page.Locator("[data-testid='recent-articles'], .recent-articles");
        _featuredContent = page.Locator("[data-testid='featured-content'], .featured-content");
    }

    /// <summary>
    /// Navigate to home page
    /// </summary>
    public override async Task GotoAsync(string path = "/")
    {
        await base.GotoAsync("/");
    }

    /// <summary>
    /// Check if a welcome message is visible
    /// </summary>
    public async Task<bool> HasWelcomeMessageAsync()
    {
        try
        {
            return await _welcomeMessage.IsVisibleAsync(new() { Timeout = 5000 });
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the main heading text
    /// </summary>
    public async Task<string> GetHeadingTextAsync()
    {
        return await _pageHeading.TextContentAsync() ?? string.Empty;
    }

    /// <summary>
    /// Check if recent articles section is visible
    /// </summary>
    public async Task<bool> HasRecentArticlesAsync()
    {
        try
        {
            return await _recentArticles.IsVisibleAsync(new() { Timeout = 5000 });
        }
        catch
        {
            return false;
        }
    }
}