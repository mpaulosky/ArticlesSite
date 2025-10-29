using FluentAssertions;

using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class AboutTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldLoadAboutPageSuccessfully()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Verify the page loads
		aboutPage.GetCurrentUrl().Should().Contain("/about");

		// Verify page title is set
		var title = await aboutPage.GetTitleAsync();
		title.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayPageHeading()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Verify heading is displayed
		var heading = await aboutPage.GetHeadingTextAsync();
		heading.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayPageContent()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Verify content is visible
		var hasContent = await aboutPage.HasContentAsync();
		hasContent.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayNavigationMenu()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Verify navigation is visible
		var isNavVisible = await aboutPage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayFooter()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Verify footer is visible
		var isFooterVisible = await aboutPage.IsFooterVisibleAsync();
		isFooterVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldNavigateBackToHomePage()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Click on Home link
		await aboutPage.ClickHomeAsync();

		// Verify navigation
		aboutPage.GetCurrentUrl().Should().Contain("/");
	}

	[Fact]
	public async Task ShouldBeAccessibleOnMobile()
	{
		var aboutPage = new AboutPage(Page);
		await aboutPage.GotoAsync();

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await Page.WaitForTimeoutAsync(500);

		// Verify content is still visible
		var hasContent = await aboutPage.HasContentAsync();
		hasContent.Should().BeTrue();
	}
}
