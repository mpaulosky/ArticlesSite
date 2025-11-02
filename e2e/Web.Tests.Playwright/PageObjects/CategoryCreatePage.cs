namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Category Create Page Object Model
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryCreatePage : BasePage
{

	private readonly ILocator _pageHeading;

	private readonly ILocator _categoryNameInput;

	private readonly ILocator _slugInput;

	private readonly ILocator _isArchivedCheckbox;

	private readonly ILocator _createButton;

	private readonly ILocator _cancelButton;

	private readonly ILocator _validationSummary;


	private readonly ILocator _errorAlert;

	public CategoryCreatePage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_categoryNameInput = page.Locator("#categoryName, input[id='categoryName']");
		_slugInput = page.Locator("#slug, input[id='slug']");
		_isArchivedCheckbox = page.Locator("#isArchived, input[id='isArchived']");
		_createButton = page.Locator("button[type='submit']:has-text('Create')");
		_cancelButton = page.Locator("button:has-text('Cancel')");
		_validationSummary = page.Locator(".validation-summary-errors, ul.validation-errors");


		_errorAlert = page.Locator("[role='alert'], .alert-danger");
	}

	/// <summary>
	/// Navigate to category create page
	/// </summary>
	public override async Task GotoAsync(string path = "/")
	{
		await base.GotoAsync("/categories/create");
	}

	/// <summary>
	/// Get the page heading text
	/// </summary>
	public async Task<string> GetHeadingTextAsync()
	{
		return await _pageHeading.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Fill in the category name
	/// </summary>
	public async Task FillCategoryNameAsync(string categoryName)
	{
		await _categoryNameInput.FillAsync(categoryName);
	}

	/// <summary>
	/// Blur the category name field to trigger validation
	/// </summary>
	public async Task BlurCategoryNameAsync()
	{
		await _categoryNameInput.BlurAsync();
	}

	/// <summary>
	/// Get the slug field value
	/// </summary>
	public async Task<string> GetSlugValueAsync()
	{
		return await _slugInput.InputValueAsync();
	}

	/// <summary>
	/// Check if slug field is disabled
	/// </summary>
	public async Task<bool> IsSlugDisabledAsync()
	{
		return await _slugInput.IsDisabledAsync();
	}

	/// <summary>
	/// Check the IsArchived checkbox
	/// </summary>
	public async Task CheckIsArchivedAsync()
	{
		await _isArchivedCheckbox.CheckAsync();
	}

	/// <summary>
	/// Uncheck the IsArchived checkbox
	/// </summary>
	public async Task UncheckIsArchivedAsync()
	{
		await _isArchivedCheckbox.UncheckAsync();
	}

	/// <summary>
	/// Get IsArchived checkbox state
	/// </summary>
	public async Task<bool> IsArchivedCheckedAsync()
	{
		return await _isArchivedCheckbox.IsCheckedAsync();
	}

	/// <summary>
	/// Click the create button
	/// </summary>
	public async Task ClickCreateButtonAsync()
	{
		await _createButton.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Click the cancel button
	/// </summary>
	public async Task ClickCancelButtonAsync()
	{
		await _cancelButton.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Check if validation summary is visible
	/// </summary>
	public async Task<bool> HasValidationSummaryAsync()
	{
		await _validationSummary.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

		return await _validationSummary.IsVisibleAsync();
	}

	/// <summary>
	/// Check if category name validation message is visible
	/// </summary>
	public async Task<bool> HasCategoryNameValidationAsync()
	{
		var validationMessages = await Page.Locator(".validation-message").AllAsync();

		foreach (var message in validationMessages)
		{
			if (await message.IsVisibleAsync())
			{
				var text = await message.TextContentAsync();

				if (!string.IsNullOrEmpty(text) && text.Contains("name", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Get validation message text
	/// </summary>
	public async Task<string> GetValidationMessageAsync()
	{
		var validationMessages = await Page.Locator(".validation-message").AllAsync();

		foreach (var message in validationMessages)
		{
			if (await message.IsVisibleAsync())
			{
				return await message.TextContentAsync() ?? string.Empty;
			}
		}

		return string.Empty;
	}

	/// <summary>
	/// Check if error alert is visible
	/// </summary>
	public async Task<bool> HasErrorAlertAsync()
	{
		await _errorAlert.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

		return await _errorAlert.IsVisibleAsync();
	}

	/// <summary>
	/// Get error alert text
	/// </summary>
	public async Task<string> GetErrorAlertTextAsync()
	{
		return await _errorAlert.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Submit the form with all fields
	/// </summary>
	public async Task SubmitFormAsync(string categoryName, bool isArchived = false)
	{
		await FillCategoryNameAsync(categoryName);

		if (isArchived)
		{
			await CheckIsArchivedAsync();
		}

		await ClickCreateButtonAsync();
	}

}