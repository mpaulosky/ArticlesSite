namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class ArticleEditTests : PlaywrightTestBase
{

	[Fact]
	public async Task ShouldDisplayModernEditCardLayout()
	{
		await Page.GotoAsync("/articles/edit/507f1f77bcf86cd799439011");
		await Page.WaitForSelectorAsync(".modern-card");
		var card = await Page.QuerySelectorAsync(".modern-card");
		card.Should().NotBeNull();
		var title = await Page.InnerTextAsync("#title");
		title.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task ShouldShowErrorAlert_WhenArticleNotFound()
	{
		await Page.GotoAsync("/articles/edit/invalid-id");
		await Page.WaitForSelectorAsync(".alert-danger");
		var alert = await Page.QuerySelectorAsync(".alert-danger");
		alert.Should().NotBeNull();
		var alertText = await Page.InnerTextAsync(".alert-danger");
		alertText.Should().Contain("Unable to load article");
	}

	[Fact]
	public async Task ShouldShowLoadingComponent_WhenLoading()
	{
		await Page.GotoAsync("/articles/edit/507f1f77bcf86cd799439011");
		await Page.WaitForSelectorAsync(".loading-spinner,.LoadingComponent");
		var loading = await Page.QuerySelectorAsync(".loading-spinner,.LoadingComponent");
		loading.Should().NotBeNull();
	}

	[Fact]
	public async Task ShouldEditAndSaveArticle()
	{
		await Page.GotoAsync("/articles/edit/507f1f77bcf86cd799439011");
		await Page.WaitForSelectorAsync("#title");
		await Page.FillAsync("#title", "Updated Title");
		await Page.ClickAsync("button[type='submit']");
		await Page.WaitForURLAsync(url => url.Contains("/articles/details/507f1f77bcf86cd799439011"));
		var newTitle = await Page.InnerTextAsync(".card-title");
		newTitle.Should().Be("Updated Title");
	}

}