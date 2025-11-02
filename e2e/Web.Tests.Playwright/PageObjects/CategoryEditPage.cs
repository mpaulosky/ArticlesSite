using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Web.Tests.Playwright.PageObjects;

[ExcludeFromCodeCoverage]
public class CategoryEditPage : BasePage
{
	public CategoryEditPage(IPage page) : base(page) { }

	public async Task GotoAsync(string categoryId)
	{
		await Page.GotoAsync($"/categories/edit/{categoryId}");
	}

	public async Task<string> GetHeadingTextAsync()
	{
		var heading = await Page.Locator("h1, h2").First.TextContentAsync();
		return heading ?? string.Empty;
	}

	public async Task<string> GetCategoryNameInputAsync()
	{
		var input = await Page.Locator("input#categoryName").InputValueAsync();
		return input ?? string.Empty;
	}

	public async Task<bool> IsArchiveCheckboxVisibleAsync()
	{
		var checkbox = Page.Locator("input#isArchived");
		return await checkbox.IsVisibleAsync();
	}

	public async Task StartSubmittingAsync()
	{
		await Page.Locator("button[type='submit']").ClickAsync();
		await Page.WaitForTimeoutAsync(500); // Simulate waiting for submit
	}

	public async Task<bool> IsSaveButtonDisabledAsync()
	{
		var saveButton = Page.Locator("button[type='submit']");
		return await saveButton.IsDisabledAsync();
	}

	public async Task ClickCancelAsync()
	{
		await Page.Locator("button:has-text('Cancel')").ClickAsync();
	}

	public string GetCurrentUrl() => Page.Url;
}