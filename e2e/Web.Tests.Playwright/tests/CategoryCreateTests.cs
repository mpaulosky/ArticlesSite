using FluentAssertions;

using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class CategoryCreateTests : PlaywrightTestBase
{
	[Fact]
	public async Task ShouldLoadCategoryCreatePageSuccessfully()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Verify the page loads
		createPage.GetCurrentUrl().Should().Contain("/categories/create");

		// Verify page title is set
		var title = await createPage.GetTitleAsync();
		title.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task ShouldDisplayPageHeading()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Verify heading is displayed
		var heading = await createPage.GetHeadingTextAsync();
		heading.Should().Contain("Create");
		heading.Should().Contain("Category");
	}

	[Fact]
	public async Task ShouldDisplayNavigationMenu()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Verify navigation is visible
		var isNavVisible = await createPage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldDisplayFooter()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Verify footer is visible
		var isFooterVisible = await createPage.IsFooterVisibleAsync();
		isFooterVisible.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldHaveSlugFieldDisabled()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Verify slug field is disabled/read-only
		var isDisabled = await createPage.IsSlugDisabledAsync();
		isDisabled.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldAutoGenerateSlugFromCategoryName()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Fill in category name
		await createPage.FillCategoryNameAsync("Test Category");
		await createPage.BlurCategoryNameAsync();

		// Wait for slug to be updated
		await Page.WaitForTimeoutAsync(1000);

		// Verify slug is auto-generated
		var slugValue = await createPage.GetSlugValueAsync();
		slugValue.Should().Be("test_category");
	}

	[Fact]
	public async Task ShouldShowValidationForEmptyCategoryName()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Try to submit without filling category name
		await createPage.ClickCreateButtonAsync();

		// Verify validation message appears
		var hasValidation = await createPage.HasCategoryNameValidationAsync() || await createPage.HasValidationSummaryAsync();
		hasValidation.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldShowValidationOnBlur()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Fill and clear category name
		await createPage.FillCategoryNameAsync("Test");
		await createPage.FillCategoryNameAsync("");
		await createPage.BlurCategoryNameAsync();

		// Verify validation appears on blur
		await Page.WaitForTimeoutAsync(1000);
		var hasValidation = await createPage.HasCategoryNameValidationAsync();
		hasValidation.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldCreateCategorySuccessfully()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Generate unique category name
		var categoryName = $"Test Category {Guid.NewGuid().ToString()[..8]}";

		// Fill in the form
		await createPage.FillCategoryNameAsync(categoryName);

		// Submit the form
		await createPage.ClickCreateButtonAsync();

		// Verify redirect to categories list
		await Page.WaitForURLAsync("**/categories", new PageWaitForURLOptions { Timeout = 10000 });
		createPage.GetCurrentUrl().Should().Contain("/categories");
		createPage.GetCurrentUrl().Should().NotContain("/create");
	}

	[Fact]
	public async Task ShouldCreateArchivedCategorySuccessfully()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Generate unique category name
		var categoryName = $"Archived Category {Guid.NewGuid().ToString()[..8]}";

		// Fill in the form with archived checkbox
		await createPage.FillCategoryNameAsync(categoryName);
		await createPage.CheckIsArchivedAsync();

		// Verify checkbox is checked
		var isChecked = await createPage.IsArchivedCheckedAsync();
		isChecked.Should().BeTrue();

		// Submit the form
		await createPage.ClickCreateButtonAsync();

		// Verify redirect to categories list
		await Page.WaitForURLAsync("**/categories", new PageWaitForURLOptions { Timeout = 10000 });
		createPage.GetCurrentUrl().Should().Contain("/categories");
		createPage.GetCurrentUrl().Should().NotContain("/create");
	}

	[Fact]
	public async Task ShouldCancelAndReturnToCategoriesList()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Fill in some data
		await createPage.FillCategoryNameAsync("Test Cancel");

		// Click cancel
		await createPage.ClickCancelButtonAsync();

		// Verify redirect to categories list
		createPage.GetCurrentUrl().Should().Contain("/categories");
		createPage.GetCurrentUrl().Should().NotContain("/create");
	}

	[Fact]
	public async Task ShouldValidateMaximumCategoryNameLength()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Fill in category name exceeding max length (80 characters)
		var longName = new string('A', 85);
		await createPage.FillCategoryNameAsync(longName);
		await createPage.BlurCategoryNameAsync();

		// Try to submit
		await createPage.ClickCreateButtonAsync();

		// Verify validation message appears
		await Page.WaitForTimeoutAsync(1000);
		var hasValidation = await createPage.HasCategoryNameValidationAsync() || await createPage.HasValidationSummaryAsync();
		hasValidation.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldPreserveFormStateOnValidationError()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Check archived checkbox
		await createPage.CheckIsArchivedAsync();

		// Try to submit without category name
		await createPage.ClickCreateButtonAsync();

		// Verify checkbox is still checked after validation error
		await Page.WaitForTimeoutAsync(1000);
		var isStillChecked = await createPage.IsArchivedCheckedAsync();
		isStillChecked.Should().BeTrue();
	}

	[Fact]
	public async Task ShouldTestAllFieldsInForm()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Test category name field
		await createPage.FillCategoryNameAsync("Test All Fields");
		var slugValue = await createPage.GetSlugValueAsync();

		// Test slug is auto-generated and read-only
		var isSlugDisabled = await createPage.IsSlugDisabledAsync();
		isSlugDisabled.Should().BeTrue();

		// Test IsArchived checkbox
		await createPage.CheckIsArchivedAsync();
		var isChecked = await createPage.IsArchivedCheckedAsync();
		isChecked.Should().BeTrue();

		// Uncheck and verify
		await createPage.UncheckIsArchivedAsync();
		var isUnchecked = await createPage.IsArchivedCheckedAsync();
		isUnchecked.Should().BeFalse();
	}

	[Fact]
	public async Task ShouldBeResponsiveOnMobile()
	{
		var createPage = new CategoryCreatePage(Page);
		await createPage.GotoAsync();

		// Test mobile viewport
		await Page.SetViewportSizeAsync(375, 667);
		await Page.WaitForTimeoutAsync(500);

		// Verify form is still usable
		await createPage.FillCategoryNameAsync("Mobile Test");
		
		// Verify navigation is still visible
		var isNavVisible = await createPage.IsNavigationVisibleAsync();
		isNavVisible.Should().BeTrue();
	}
}
