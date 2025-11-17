namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Categories List Page Object Model
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoriesListPage : BasePage
{
	private readonly ILocator _pageHeading;
	private readonly ILocator _categoriesTable;
	private readonly ILocator _categoryRows;
	private readonly ILocator _createButton;
	private readonly ILocator _searchInput;
	private readonly ILocator _noCategoriesMessage;
	private readonly ILocator _includeArchivedCheckbox;

	public CategoriesListPage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_categoriesTable = page.Locator(".quickgrid, table");
		_categoryRows = page.Locator(".quickgrid tbody tr, table tbody tr");
		_createButton = page.Locator("a[href*='/categories/create'], button:has-text('Create'), button:has-text('New Category')");
		_searchInput = page.Locator("input[type='search'], input[placeholder*='Search']");
		_noCategoriesMessage = page.Locator("text=/no categories/i, .empty-state");
		_includeArchivedCheckbox = page.Locator("input[type='checkbox']");
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
		return await _categoryRows.CountAsync();
	}

	/// <summary>
	/// Check if categories list is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasCategoriesListAsync()
	{
		try
		{
			var options = new LocatorIsVisibleOptions();
			return await _categoriesTable.IsVisibleAsync(options);
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
			var options = new LocatorIsVisibleOptions();
			return await _createButton.IsVisibleAsync(options);
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
		await _categoryRows.Nth(index).ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Search for categories
	/// </summary>
	[Obsolete]
	public async Task SearchCategoriesAsync(string searchTerm)
	{
		var options = new LocatorIsVisibleOptions();
		if (await _searchInput.IsVisibleAsync(options))
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
			var options = new LocatorIsVisibleOptions();
			return await _noCategoriesMessage.IsVisibleAsync(options);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Get the include archived checkbox locator
	/// </summary>
	public async Task<ILocator?> GetIncludeArchivedCheckboxAsync()
	{
		try
		{
			var options = new LocatorIsVisibleOptions();
			if (await _includeArchivedCheckbox.IsVisibleAsync(options))
			{
				return _includeArchivedCheckbox;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}
}