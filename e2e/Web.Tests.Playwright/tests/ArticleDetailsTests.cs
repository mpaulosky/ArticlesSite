namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class ArticleDetailsTests : PlaywrightTestBase
{

	[Fact]
	public async Task ShouldDisplayModernCardLayout()
	{
		// Arrange
		await Page.GotoAsync("/articles/details/507f1f77bcf86cd799439011");
		await Page.WaitForSelectorAsync(".modern-card");

		// Assert
		var card = await Page.QuerySelectorAsync(".modern-card");
		card.Should().NotBeNull();
		var title = await Page.InnerTextAsync(".card-title");
		title.Should().NotBeNullOrWhiteSpace();
		var author = await Page.InnerTextAsync(".bi-person-circle");
		author.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task ShouldShowErrorAlert_WhenArticleNotFound()
	{
		// Arrange
		await Page.GotoAsync("/articles/details/invalid-id");
		await Page.WaitForSelectorAsync(".alert-danger");

		// Assert
		var alert = await Page.QuerySelectorAsync(".alert-danger");
		alert.Should().NotBeNull();
		var alertText = await Page.InnerTextAsync(".alert-danger");
		alertText.Should().Contain("Unable to load article");
	}

	[Fact]
	public async Task ShouldShowLoadingComponent_WhenLoading()
	{
		// Arrange
		await Page.GotoAsync("/articles/details/507f1f77bcf86cd799439011");

		// Simulate loading by waiting for the loading component
		await Page.WaitForSelectorAsync(".loading-spinner,.LoadingComponent");

		// Assert
		var loading = await Page.QuerySelectorAsync(".loading-spinner,.LoadingComponent");
		loading.Should().NotBeNull();
	}

}