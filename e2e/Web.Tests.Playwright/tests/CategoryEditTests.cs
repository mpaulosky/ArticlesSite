using FluentAssertions;

using Web.Tests.Playwright.PageObjects;

using Microsoft.Playwright;

using Xunit;

namespace Web.Tests.Playwright.Tests;

public class CategoryEditTests : PlaywrightTestBase
{

	[Fact]
	public async Task ShouldLoadCategoryEditPageSuccessfully()
	{
		var editPage = new CategoryEditPage(Page);
		await editPage.GotoAsync("some-category-id"); // Replace with a valid category id in your test setup

		// Verify the page loads
		editPage.GetCurrentUrl().Should().Contain("/categories/edit/");

		// Verify heading is displayed
		var heading = await editPage.GetHeadingTextAsync();
		heading.Should().Contain("Edit Category");
	}

	[Fact]
	public async Task ShouldDisplayCategoryEditFormCorrectly()
	{
		var editPage = new CategoryEditPage(Page);
		await editPage.GotoAsync("some-category-id"); // Replace with a valid category id in your test setup

		// Verify category name input is displayed
		var name = await editPage.GetCategoryNameInputAsync();
		name.Should().NotBeNullOrEmpty();

		// Verify archive checkbox is displayed
		var isArchiveVisible = await editPage.IsArchiveCheckboxVisibleAsync();
		isArchiveVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisableSaveButtonWhileSubmitting()
	{
		var editPage = new CategoryEditPage(Page);
		await editPage.GotoAsync("some-category-id");

		await editPage.StartSubmittingAsync();
		var isSaveDisabled = await editPage.IsSaveButtonDisabledAsync();
		isSaveDisabled.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldNavigateBackToList()
	{
		var editPage = new CategoryEditPage(Page);
		await editPage.GotoAsync("some-category-id");

		await editPage.ClickCancelAsync();
		editPage.GetCurrentUrl().Should().Contain("/categories");
	}

}
