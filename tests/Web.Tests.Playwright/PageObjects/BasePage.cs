namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Base Page Object Model
/// Contains common methods and locators shared across all pages
/// </summary>
public class BasePage
{
    protected readonly IPage Page;
    protected readonly ILocator Navigation;
    protected readonly ILocator Footer;
    protected readonly ILocator HomeLink;
    protected readonly ILocator AboutLink;
    protected readonly ILocator ContactLink;
    protected readonly ILocator ArticlesLink;
    protected readonly ILocator CategoriesLink;
    protected readonly ILocator ProfileLink;
    protected readonly ILocator AdminLink;

    public BasePage(IPage page)
    {
        Page = page;
        Navigation = page.Locator("nav");
        Footer = page.Locator("footer");
        
        // Navigation links
        HomeLink = page.Locator("a[href='/']");
        AboutLink = page.Locator("a[href='/about']");
        ContactLink = page.Locator("a[href='/contact']");
        ArticlesLink = page.Locator("a[href*='/articles']");
        CategoriesLink = page.Locator("a[href*='/categories']");
        ProfileLink = page.Locator("a[href='/profile']");
        AdminLink = page.Locator("a[href='/admin']");
    }

    /// <summary>
    /// Navigate to a specific URL
    /// </summary>
    public virtual async Task GotoAsync(string path = "/")
    {
        await Page.GotoAsync(path);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Get the page title
    /// </summary>
    public async Task<string> GetTitleAsync()
    {
        return await Page.TitleAsync();
    }

    /// <summary>
    /// Check if the navigation is visible
    /// </summary>
    public async Task<bool> IsNavigationVisibleAsync()
    {
        return await Navigation.IsVisibleAsync();
    }

    /// <summary>
    /// Check if the footer is visible
    /// </summary>
    public async Task<bool> IsFooterVisibleAsync()
    {
        return await Footer.IsVisibleAsync();
    }

    /// <summary>
    /// Click on Home link
    /// </summary>
    public async Task ClickHomeAsync()
    {
        await HomeLink.First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Click on About link
    /// </summary>
    public async Task ClickAboutAsync()
    {
        await AboutLink.First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Click on Contact link
    /// </summary>
    public async Task ClickContactAsync()
    {
        await ContactLink.First.ClickAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Get the current URL
    /// </summary>
    public string GetCurrentUrl()
    {
        return Page.Url;
    }

    /// <summary>
    /// Wait for specific text to appear
    /// </summary>
    public async Task WaitForTextAsync(string text)
    {
        await Page.WaitForSelectorAsync($"text={text}", new() { Timeout = 10000 });
    }

    /// <summary>
    /// Check if error message is displayed
    /// </summary>
    public async Task<bool> HasErrorMessageAsync()
    {
        var errorLocator = Page.Locator("[role='alert'], .alert-danger, .error-message");
        return await errorLocator.CountAsync() > 0;
    }

    /// <summary>
    /// Get error message text
    /// </summary>
    public async Task<string> GetErrorMessageAsync()
    {
        var errorLocator = Page.Locator("[role='alert'], .alert-danger, .error-message");
        return await errorLocator.First.TextContentAsync() ?? string.Empty;
    }
}
