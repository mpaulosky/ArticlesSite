using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class NavigationTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldDisplayNavigationMenuOnAllPages()
	{
		var basePage = new BasePage(Page);

		// Test home page
		await basePage.GotoAsync("/");
		(await basePage.IsNavigationVisibleAsync()).Should().BeTrue();

		// Test about page
		await basePage.GotoAsync("/about");
		(await basePage.IsNavigationVisibleAsync()).Should().BeTrue();

		// Test contact page
		await basePage.GotoAsync("/contact");
		(await basePage.IsNavigationVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task ShouldNavigateBetweenPagesUsingNavigationLinks()
	{
		var basePage = new BasePage(Page);

		// Start at home
		await basePage.GotoAsync("/");

		// Navigate to About
		await basePage.ClickAboutAsync();
		basePage.GetCurrentUrl().Should().Contain("/about");

		// Navigate to Contact
		await basePage.ClickContactAsync();
		basePage.GetCurrentUrl().Should().Contain("/contact");

		// Navigate back to Home
		await basePage.ClickHomeAsync();
		basePage.GetCurrentUrl().Should().Contain("/");
	}

	[Fact]
	public async Task ShouldBeResponsiveOnMobileDevices()
	{
		var basePage = new BasePage(Page);

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await basePage.GotoAsync("/");
		await Page.WaitForTimeoutAsync(500);

		// Verify navigation is still accessible
		(await basePage.IsNavigationVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task ShouldBeResponsiveOnTabletDevices()
	{
		var basePage = new BasePage(Page);

		// Test tablet viewport
		await Page.SetViewportSizeAsync(768, 1024);
		await basePage.GotoAsync("/");
		await Page.WaitForTimeoutAsync(500);

		// Verify navigation is still accessible
		(await basePage.IsNavigationVisibleAsync()).Should().BeTrue();
	}
}