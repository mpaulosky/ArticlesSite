using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class HomeTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldLoadHomePageSuccessfully()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Verify the page loads
		homePage.GetCurrentUrl().Should().Contain("/");

		// Verify page title is set
		var title = await homePage.GetTitleAsync();
		title.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayNavigationMenu()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Verify navigation is visible
		var isNavVisible = await homePage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayFooter()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Verify footer is visible
		var isFooterVisible = await homePage.IsFooterVisibleAsync();
		isFooterVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldHaveMainHeading()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Verify heading is displayed
		var heading = await homePage.GetHeadingTextAsync();
		heading.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldNavigateToAboutPageFromHome()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Click on About link
		await homePage.ClickAboutAsync();

		// Verify navigation
		homePage.GetCurrentUrl().Should().Contain("/about");
	}

	[Fact]
	public async Task ShouldNavigateToContactPageFromHome()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Click on Contact link
		await homePage.ClickContactAsync();

		// Verify navigation
		homePage.GetCurrentUrl().Should().Contain("/contact");
	}

	[Fact]
	public async Task ShouldBeResponsive()
	{
		var homePage = new HomePage(Page);
		await homePage.GotoAsync();

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await Page.WaitForTimeoutAsync(500);

		// Verify navigation still works
		var isNavVisible = await homePage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}
}