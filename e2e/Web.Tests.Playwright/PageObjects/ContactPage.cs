namespace Web.Tests.Playwright.PageObjects;

/// <summary>
/// Contact Page Object Model
/// </summary>
[ExcludeFromCodeCoverage]
public class ContactPage : BasePage
{
	private readonly ILocator _pageHeading;
	private readonly ILocator _contactForm;
	private readonly ILocator _nameInput;
	private readonly ILocator _emailInput;
	private readonly ILocator _messageInput;
	private readonly ILocator _submitButton;
	private readonly ILocator _successMessage;

	public ContactPage(IPage page) : base(page)
	{
		_pageHeading = page.Locator("h1, h2").First;
		_contactForm = page.Locator("form");
		_nameInput = page.Locator("input[name='name'], input[id*='name']");
		_emailInput = page.Locator("input[name='email'], input[id*='email'], input[type='email']");
		_messageInput = page.Locator("textarea[name='message'], textarea[id*='message']");
		_submitButton = page.Locator("button[type='submit'], input[type='submit']");
		_successMessage = page.Locator(".alert-success, .success-message");
	}

	/// <summary>
	/// Navigate to contact page
	/// </summary>
	public override async Task GotoAsync(string path = "/")
	{
		await base.GotoAsync("/contact");
	}

	/// <summary>
	/// Get the page heading text
	/// </summary>
	public async Task<string> GetHeadingTextAsync()
	{
		return await _pageHeading.TextContentAsync() ?? string.Empty;
	}

	/// <summary>
	/// Check if contact form is visible
	/// </summary>
	[Obsolete]
	public async Task<bool> HasContactFormAsync()
	{
		try
		{
			return await _contactForm.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}

	/// <summary>
	/// Fill and submit contact form
	/// </summary>
	public async Task SubmitContactFormAsync(string name, string email, string message)
	{
		if (await _nameInput.IsVisibleAsync())
		{
			await _nameInput.FillAsync(name);
		}
		if (await _emailInput.IsVisibleAsync())
		{
			await _emailInput.FillAsync(email);
		}
		if (await _messageInput.IsVisibleAsync())
		{
			await _messageInput.FillAsync(message);
		}
		await _submitButton.ClickAsync();
		await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
	}

	/// <summary>
	/// Check if success message is displayed
	/// </summary>
	[Obsolete]
	public async Task<bool> HasSuccessMessageAsync()
	{
		try
		{
			return await _successMessage.IsVisibleAsync(new LocatorIsVisibleOptions { Timeout = 5000 });
		}
		catch
		{
			return false;
		}
	}
}