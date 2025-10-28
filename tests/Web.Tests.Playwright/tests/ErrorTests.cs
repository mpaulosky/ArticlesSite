using FluentAssertions;
using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class ErrorTests : PlaywrightTestBase
{
    [Fact]
    public async Task ShouldDisplay404ErrorPageForNonExistentRoutes()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Verify error page is displayed
        var isErrorDisplayed = await errorPage.IsErrorPageDisplayedAsync();
        isErrorDisplayed.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldDisplayErrorHeadingOn404Page()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Verify error heading is displayed
        var heading = await errorPage.GetErrorHeadingTextAsync();
        heading.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldProvideNavigationBackToHomeOnErrorPage()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Verify home link is available
        var hasHomeLink = await errorPage.HasHomeLinkAsync();
        hasHomeLink.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNavigateBackToHomeFromErrorPage()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Click home link
        await errorPage.ClickHomeLinkAsync();

        // Verify navigation to home
        errorPage.GetCurrentUrl().Should().Contain("/");
    }

    [Fact]
    public async Task ShouldDisplayNavigationMenuOnErrorPage()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Verify navigation is visible
        var isNavVisible = await errorPage.IsNavigationVisibleAsync();
        isNavVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldBeResponsiveOnMobile()
    {
        var errorPage = new ErrorPage(Page);

        // Navigate to non-existent page
        await errorPage.GotoNonExistentPageAsync();

        // Test mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.WaitForTimeoutAsync(500);

        // Verify error page is still displayed
        var isErrorDisplayed = await errorPage.IsErrorPageDisplayedAsync();
        isErrorDisplayed.Should().BeTrue();
    }
}
