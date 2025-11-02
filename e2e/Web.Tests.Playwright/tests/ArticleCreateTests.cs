using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Playwright;

using Xunit;

namespace Web.Tests.Playwright.Tests;

public class ArticleCreateTests : PlaywrightTestBase
{

	[Fact]
	public async Task ShouldDisplayModernCreateCardLayout()
	{
		await Page.GotoAsync("/articles/create");
		await Page.WaitForSelectorAsync(".modern-card");
		var card = await Page.QuerySelectorAsync(".modern-card");
		card.Should().NotBeNull();
		var title = await Page.InnerTextAsync("#title");
		title.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task ShouldShowErrorAlert_WhenCreateFails()
	{
		// Simulate error by submitting empty form
		await Page.GotoAsync("/articles/create");
		await Page.ClickAsync("button[type='submit']");
		await Page.WaitForSelectorAsync(".alert-danger");
		var alert = await Page.QuerySelectorAsync(".alert-danger");
		alert.Should().NotBeNull();
		var alertText = await Page.InnerTextAsync(".alert-danger");
		alertText.Should().Contain("Unable to create article");
	}

	[Fact]
	public async Task ShouldShowLoadingComponent_WhenLoading()
	{
		await Page.GotoAsync("/articles/create");
		await Page.WaitForSelectorAsync(".loading-spinner,.LoadingComponent");
		var loading = await Page.QuerySelectorAsync(".loading-spinner,.LoadingComponent");
		loading.Should().NotBeNull();
	}

	[Fact]
	public async Task ShouldCreateArticleAndRedirect()
	{
		await Page.GotoAsync("/articles/create");
		await Page.FillAsync("#title", "New Article Title");
		await Page.FillAsync("#introduction", "Intro");
		await Page.FillAsync("#content", "Content");
		await Page.FillAsync("#coverImageUrl", "https://example.com/image.jpg");
		await Page.FillAsync("input[placeholder='Category']", "Tech");
		await Page.FillAsync("input[placeholder='Author']", "Test Author");
		await Page.ClickAsync("button[type='submit']");
		await Page.WaitForURLAsync(url => url.Contains("/articles/details"));
		var newTitle = await Page.InnerTextAsync(".card-title");
		newTitle.Should().Be("New Article Title");
	}

}
