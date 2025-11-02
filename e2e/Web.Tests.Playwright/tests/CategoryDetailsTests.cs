using Web.Tests.Playwright.PageObjects;

using Microsoft.Playwright;

using Xunit;

namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class CategoryDetailsTests : PlaywrightTestBase
{

	[Fact]
	public async Task ShouldLoadCategoryDetailsPageSuccessfully()
	{
		var detailsPage = new CategoryDetailsPage(Page);
		await detailsPage.GotoAsync("some-category-id"); // Replace with a valid category id in your test setup

		// Verify the page loads
		detailsPage.GetCurrentUrl().Should().Contain("/categories/details/");

		// Verify heading is displayed
		var heading = await detailsPage.GetHeadingTextAsync();
		heading.Should().Contain("Category Details");
	}

	[Fact]
	public async Task ShouldDisplayCategoryDetailsCorrectly()
	{
		var detailsPage = new CategoryDetailsPage(Page);
		await detailsPage.GotoAsync("some-category-id"); // Replace with a valid category id in your test setup

		// Verify category name is displayed
		var name = await detailsPage.GetCategoryNameAsync();
		name.Should().NotBeNullOrEmpty();

		// Verify status badge is displayed
		var status = await detailsPage.GetStatusBadgeAsync();
		(status == "Active" || status == "Archived").Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisableEditButtonIfArchived()
	{
		var detailsPage = new CategoryDetailsPage(Page);
		await detailsPage.GotoAsync("archived-category-id"); // Replace with an archived category id in your test setup

		var isEditDisabled = await detailsPage.IsEditButtonDisabledAsync();
		isEditDisabled.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldNavigateBackToList()
	{
		var detailsPage = new CategoryDetailsPage(Page);
		await detailsPage.GotoAsync("some-category-id");

		await detailsPage.ClickBackToListAsync();
		detailsPage.GetCurrentUrl().Should().Contain("/categories");
	}

}