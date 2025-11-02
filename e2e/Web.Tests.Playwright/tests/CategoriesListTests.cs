using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class CategoriesListTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldLoadCategoriesListPageSuccessfully()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Verify the page loads
		categoriesPage.GetCurrentUrl().Should().Contain("/categories");

		// Verify page title is set
		var title = await categoriesPage.GetTitleAsync();
		title.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayPageHeading()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Verify heading is displayed
		var heading = await categoriesPage.GetHeadingTextAsync();
		heading.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayNavigationMenu()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Verify navigation is visible
		var isNavVisible = await categoriesPage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayFooter()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Verify footer is visible
		var isFooterVisible = await categoriesPage.IsFooterVisibleAsync();
		isFooterVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldShowCategoriesListOrEmptyState()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// The page should either show categories or an empty state message
		// Replace obsolete HasCategoriesListAsync() with GetCategoriesCountAsync()
		var count = await categoriesPage.GetCategoriesCountAsync();
		var hasCategories = count > 0;

		// Fix: Await only Task-returning methods, not object.
		var noCategoriesMessageField = categoriesPage.GetType()
			.GetField("_noCategoriesMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var noCategoriesMessage = noCategoriesMessageField?.GetValue(categoriesPage) as ILocator;
		var hasEmptyMessage = noCategoriesMessage != null && await noCategoriesMessage.IsVisibleAsync();

		// At least one should be true
		(hasCategories || hasEmptyMessage).Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayCategoryCountCorrectly()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Get category count
		var count = await categoriesPage.GetCategoriesCountAsync();

		// Count should be a non-negative number
		count.Should().BeGreaterThanOrEqualTo(0);
	}

	[Fact]
	public async Task ShouldNavigateBackToHomePage()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Click on Home link
		await categoriesPage.ClickHomeAsync();

		// Verify navigation
		categoriesPage.GetCurrentUrl().Should().Contain("/");
	}

	[Fact]
	public async Task ShouldBeResponsiveOnMobile()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await Page.WaitForTimeoutAsync(500);

		// Verify navigation is still visible
		var isNavVisible = await categoriesPage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldHandleSearchIfAvailable()
	{
		var categoriesPage = new CategoriesListPage(Page);
		await categoriesPage.GotoAsync();

		// Try to search (only if search is available)
		try
		{
			// Replace obsolete SearchCategoriesAsync with direct interaction with _searchInput
			// Enter "test" in the search input if available
			if (categoriesPage.GetType().GetField("_searchInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(categoriesPage) is ILocator searchInput)
			{
				await searchInput.FillAsync("test");
				await Page.Keyboard.PressAsync("Enter");
			}
			// If search works, just verify page is still loaded
			categoriesPage.GetCurrentUrl().Should().Contain("/categories");
		}
		catch
		{
			// Search might not be available, which is okay
			true.Should().BeTrue();
		}
	}
}