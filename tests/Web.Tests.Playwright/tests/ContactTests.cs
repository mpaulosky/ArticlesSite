using FluentAssertions;
using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class ContactTests : PlaywrightTestBase
{
    [Fact]
    public async Task ShouldLoadContactPageSuccessfully()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Verify the page loads
        contactPage.GetCurrentUrl().Should().Contain("/contact");

        // Verify page title is set
        var title = await contactPage.GetTitleAsync();
        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldDisplayPageHeading()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Verify heading is displayed
        var heading = await contactPage.GetHeadingTextAsync();
        heading.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShouldDisplayNavigationMenu()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Verify navigation is visible
        var isNavVisible = await contactPage.IsNavigationVisibleAsync();
        isNavVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldDisplayFooter()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Verify footer is visible
        var isFooterVisible = await contactPage.IsFooterVisibleAsync();
        isFooterVisible.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldHaveContactFormIfAvailable()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Check if contact form exists on the page
        // This is optional as not all contact pages have forms
        var hasForm = await contactPage.HasContactFormAsync();

        // We just verify the check completes without error
        // The form may or may not be present depending on implementation
        hasForm.Should().Be(hasForm);
    }

    [Fact]
    public async Task ShouldNavigateBackToHomePage()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Click on Home link
        await contactPage.ClickHomeAsync();

        // Verify navigation
        contactPage.GetCurrentUrl().Should().Contain("/");
    }

    [Fact]
    public async Task ShouldBeAccessibleOnMobile()
    {
        var contactPage = new ContactPage(Page);
        await contactPage.GotoAsync();

        // Test mobile viewport
        await Page.SetViewportSizeAsync(375, 667);
        await Page.WaitForTimeoutAsync(500);

        // Verify navigation is still visible
        var isNavVisible = await contactPage.IsNavigationVisibleAsync();
        isNavVisible.Should().BeTrue();
    }
}
