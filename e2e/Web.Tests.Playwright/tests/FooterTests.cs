using FluentAssertions;

using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class FooterTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldDisplayFooterOnAllPages()
	{
		var basePage = new BasePage(Page);

		// Test home page
		await basePage.GotoAsync("/");
		(await basePage.IsFooterVisibleAsync()).Should().BeTrue();

		// Test about page
		await basePage.GotoAsync("/about");
		(await basePage.IsFooterVisibleAsync()).Should().BeTrue();

		// Test contact page
		await basePage.GotoAsync("/contact");
		(await basePage.IsFooterVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task ShouldBeVisibleOnMobileDevices()
	{
		var basePage = new BasePage(Page);

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await basePage.GotoAsync("/");
		await Page.WaitForTimeoutAsync(500);

		// Scroll to footer
		await Page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
		await Page.WaitForTimeoutAsync(500);

		// Verify footer is visible
		(await basePage.IsFooterVisibleAsync()).Should().BeTrue();
	}

	[Fact]
	public async Task ShouldBeVisibleOnDesktop()
	{
		var basePage = new BasePage(Page);

		// Test desktop viewport
		await Page.SetViewportSizeAsync(1920, 1080);
		await basePage.GotoAsync("/");
		await Page.WaitForTimeoutAsync(500);

		// Verify footer is visible
		(await basePage.IsFooterVisibleAsync()).Should().BeTrue();
	}
}
