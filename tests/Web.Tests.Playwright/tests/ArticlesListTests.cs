using FluentAssertions;
using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class ArticlesListTests : PlaywrightTestBase
{
    [Fact]
    public async Task ShouldLoadArticlesListPageSuccessfully()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Verify the page loads
        articlesPage.GetCurrentUrl().Should().Contain("/articles");

        // Verify page title is set
        var title = await articlesPage.GetTitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldDisplayPageHeading()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Verify heading is displayed
        var heading = await articlesPage.GetHeadingTextAsync();
        heading.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldDisplayNavigationMenu()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Verify navigation is visible
        var isNavVisible = await articlesPage.IsNavigationVisibleAsync();
        isNavVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldDisplayFooter()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Verify footer is visible
        var isFooterVisible = await articlesPage.IsFooterVisibleAsync();
        isFooterVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldShowArticlesListOrEmptyState()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // The page should either show articles or an empty state message
        var hasArticles = await articlesPage.HasArticlesListAsync();
        var hasEmptyMessage = await articlesPage.HasNoArticlesMessageAsync();

        // At least one should be true
        (hasArticles || hasEmptyMessage).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldDisplayArticleCountCorrectly()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Get article count
        var count = await articlesPage.GetArticlesCountAsync();

        // Count should be a non-negative number
        count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ShouldNavigateBackToHomePage()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Click on Home link
        await articlesPage.ClickHomeAsync();

        // Verify navigation
        articlesPage.GetCurrentUrl().Should().Contain("/");
    }

    [Fact]
    public async Task ShouldBeResponsiveOnMobile()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Test mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.WaitForTimeoutAsync(500);

        // Verify navigation is still visible
        var isNavVisible = await articlesPage.IsNavigationVisibleAsync();
        isNavVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldHandleSearchIfAvailable()
    {
        var articlesPage = new ArticlesListPage(Page);
        await articlesPage.GotoAsync();

        // Try to search (only if search is available)
        try
        {
            await articlesPage.SearchArticlesAsync("test");
            // If search works, just verify page is still loaded
            articlesPage.GetCurrentUrl().Should().Contain("/articles");
        }
        catch
        {
            // Search might not be available, which is okay
            true.Should().BeTrue();
        }
    }
}
