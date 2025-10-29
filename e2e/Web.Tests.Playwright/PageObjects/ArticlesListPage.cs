namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Articles List Page Object Model
/// </summary>
public class ArticlesListPage : BasePage
{
	private readonly ILocator _pageHeading;
	private readonly ILocator _articlesList;
	private readonly ILocator _articleItems;
	private readonly ILocator _createButton;
	private readonly ILocator _searchInput;
	private readonly ILocator _filterDropdown;
	private readonly ILocator _noArticlesMessage;

	public ArticlesListPage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_articlesList = page.Locator("[data-testid='articles-list'], .articles-list, .article-grid");
		_articleItems = page.Locator("[data-testid='article-item'], .article-item, .article-card");
		_createButton = page.Locator("a[href*='/articles/create'], button:has-text('Create'), button:has-text('New Article')");
		_searchInput = page.Locator("input[type='search'], input[placeholder*='Search']");
		_filterDropdown = page.Locator("select[name='filter'], select[id*='filter']");
		_noArticlesMessage = page.Locator("text=/no articles/i, .empty-state");
	}

	/// <summary>
	/// Navigate to articles list page
	/// </summary>
	public override async Task GotoAsync(string path = "/")
	{
		await base.GotoAsync("/articles");
	}

	/// <summary>
	/// Get the page heading text
	/// </summary>
	public async Task<string> GetHeadingTextAsync()
	{
		return await _pageHeading.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Get the count of articles displayed
	/// </summary>
	public async Task<int> GetArticlesCountAsync()
	{
		return await _articleItems.CountAsync();
	}

	/// <summary>
	/// Check if articles list is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasArticlesListAsync()
	{
		try
		{
			return await _articlesList.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
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
			LocatorIsVisibleOptions options = new LocatorIsVisibleOptions() ;
			options.Timeout = 5000;

			return await _createButton.IsVisibleAsync(options);
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Click on create new article button
	/// </summary>
	public async Task ClickCreateButtonAsync()
	{
		await _createButton.First.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Click on a specific article by index
	/// </summary>
	public async Task ClickArticleAsync(int index = 0)
	{
		await _articleItems.Nth(index).ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Search for articles
	/// </summary>
	[Obsolete]
	public async Task SearchArticlesAsync(string searchTerm)
	{
		if (await _searchInput.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 2000 }))
		{
			await _searchInput.FillAsync(searchTerm);
			await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
		}
	}

	/// <summary>
	/// Check if no articles message is displayed
	/// </summary>
	[Obsolete]
	public async Task<bool> HasNoArticlesMessageAsync()
	{
		try
		{
			return await _noArticlesMessage.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	// Add this public property to expose _searchInput safely
	public ILocator? SearchInput => _searchInput;
}