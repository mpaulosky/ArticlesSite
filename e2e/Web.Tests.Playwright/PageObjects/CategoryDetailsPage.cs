using Microsoft.Playwright;

using System.Threading.Tasks;

namespace Web.Tests.Playwright.PageObjects;

[ExcludeFromCodeCoverage]
public class CategoryDetailsPage : BasePage
{

	public CategoryDetailsPage(IPage page) : base(page) { }

	public async Task GotoAsync(string categoryId)
	{
		await Page.GotoAsync($"/categories/details/{categoryId}");
	}

	public async Task<string> GetHeadingTextAsync()
	{
		var heading = await Page.Locator("h1, h2").First.TextContentAsync();

		return heading ?? string.Empty;
	}

	public async Task<string> GetCategoryNameAsync()
	{
		var name = await Page.Locator("h2.text-blue-300").TextContentAsync();

		return name ?? string.Empty;
	}

	public async Task<string> GetStatusBadgeAsync()
	{
		var badge = await Page.Locator(".badge").TextContentAsync();

		return badge ?? string.Empty;
	}

	public async Task<bool> IsEditButtonDisabledAsync()
	{
		var editButton = Page.Locator("button:has-text('Edit')");

		return await editButton.IsDisabledAsync();
	}

	public async Task ClickBackToListAsync()
	{
		await Page.Locator("button:has-text('Back to List')").ClickAsync();
	}

	public string GetCurrentUrl() => Page.Url;

}