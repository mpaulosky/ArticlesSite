namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Categories List Page Object Model
/// </summary>
public class CategoriesListPage : BasePage
{
	private readonly ILocator _pageHeading;
	private readonly ILocator _categoriesList;
	private readonly ILocator _categoryItems;
	private readonly ILocator _createButton;
	private readonly ILocator _searchInput;
	private readonly ILocator _noCategoriesMessage;

	public CategoriesListPage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_categoriesList = page.Locator("[data-testid='categories-list'], .categories-list, .category-grid");
		_categoryItems = page.Locator("[data-testid='category-item'], .category-item, .category-card");
		_createButton = page.Locator("a[href*='/categories/create'], button:has-text('Create'), button:has-text('New Category')");
		_searchInput = page.Locator("input[type='search'], input[placeholder*='Search']");
		_noCategoriesMessage = page.Locator("text=/no categories/i, .empty-state");
	}

	/// <summary>
	/// Navigate to categories list page
	/// </summary>
	public override async Task GotoAsync(string path = "/")
	{
		await base.GotoAsync("/categories");
	}

	/// <summary>
	/// Get the page heading text
	/// </summary>
	public async Task<string> GetHeadingTextAsync()
	{
		return await _pageHeading.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Get the count of categories displayed
	/// </summary>
	public async Task<int> GetCategoriesCountAsync()
	{
		return await _categoryItems.CountAsync();
	}

	/// <summary>
	/// Check if categories list is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasCategoriesListAsync()
	{
		try
		{
			return await _categoriesList.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Check if create button is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasCreateButtonAsync()
	{
		try
		{
			return await _createButton.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Click on create new category button
	/// </summary>
	public async Task ClickCreateButtonAsync()
	{
		await _createButton.First.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Click on a specific category by index
	/// </summary>
	public async Task ClickCategoryAsync(int index = 0)
	{
		await _categoryItems.Nth(index).ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Search for categories
	/// </summary>
	[Obsolete]
	public async Task SearchCategoriesAsync(string searchTerm)
	{
		if (await _searchInput.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
		{
			await _searchInput.FillAsync(searchTerm);
			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		}
	}

	/// <summary>
	/// Check if no categories message is displayed
	/// </summary>
	[Obsolete]
	public async Task<bool> HasNoCategoriesMessageAsync()
	{
		try
		{
			return await _noCategoriesMessage.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}
}