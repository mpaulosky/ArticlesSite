using Microsoft.AspNetCore.Components;
using Web.Components.Features.Categories.CategoriesList;
using Web.Infrastructure;

namespace Web.Components.Features.Articles.ArticleEdit;

public class EditArticleConflictTests : BunitContext
{
	public EditArticleConflictTests()
	{
		JSInterop.SetupVoid("initialize", _ => true).SetVoidResult();
		Services.AddSingleton(Substitute.For<IFileStorage>());
	}

	[Fact]
	public async Task When_EditResultsInConcurrency_Then_ConflictPanelIsShown_And_ForceOverwriteSucceeds()
	{
		// Arrange
		var articleId = ObjectId.GenerateNewId();

		var initial = new ArticleDto(
				articleId,
				"initial_slug",
				"Initial Title",
				"Intro",
				"Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				0
		);

		var server = new ArticleDto(
				articleId,
				"initial_slug",
				"Server Title",
				"Intro",
				"Server Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				1
		);

		var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();

		// The first call returns initial, further calls return the server
		getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

		var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, [ "Title", "Content" ]);

		var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();

		// The first save attempt fails with concurrency, the second attempt (force overwrite) succeeds
		editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(
				Result.Fail<ArticleDto>("Concurrency conflict: article was modified by another process",
						ResultErrorCode.Concurrency, conflictInfo),
				Result.Ok(server)
		);

		var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>([]));

		// Register services in test DI
		Services.AddSingleton(getHandler);
		Services.AddSingleton(editHandler);
		Services.AddSingleton(categoriesHandler);

		// Provide a fake NavigationManager to observe navigation
		var navMan = Services.GetRequiredService<NavigationManager>();

		// Act - render component
		var cut = Render<Edit>(parameters =>
				parameters.Add(p => p.Id, articleId.ToString()));

		// Ensure the component finished an initial load
		await cut.WaitForAssertionAsync(() => cut.Find("form"));

		// Submit the form (click Save)
		var saveButton = cut.Find("button[type=submit]");
		await saveButton.ClickAsync();

		// Wait for the conflict UI to appear
		await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

		// Assert conflict UI present
		cut.Markup.Should().Contain("Concurrency Conflict");
		var reloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reload Latest"));
		var forceButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Force Overwrite"));
		reloadButton.Should().NotBeNull();
		forceButton.Should().NotBeNull();

		// Act - click Force Overwrite
		await forceButton.ClickAsync();

		// Wait for navigation to the details page after successful overwriting
		await Task.Delay(50); // small delay for navigation to occur in a component

		// Assert navigation occurred to the details page
		navMan.Uri.Should().Contain($"/articles/details/{articleId}");

		// Verify Edit handler was called twice (initial failure + retry/force)
		await editHandler.Received(2).HandleAsync(Arg.Any<ArticleDto>());
	}

	[Fact]
	public async Task When_EditResultsInConcurrency_Then_ReloadLatestLoadsServerArticle()
	{
		// Arrange
		var articleId = ObjectId.GenerateNewId();

		var initial = new ArticleDto(
				articleId,
				"initial_slug",
				"Initial Title",
				"Intro",
				"Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				0
		);

		var server = new ArticleDto(
				articleId,
				"initial_slug",
				"Server Title",
				"Intro",
				"Server Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				1
		);

		var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();
		getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

		var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, [ "Title", "Content" ]);
		var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();

		editHandler.HandleAsync(Arg.Any<ArticleDto>())
				.Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

		var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>([]));

		Services.AddSingleton(getHandler);
		Services.AddSingleton(editHandler);
		Services.AddSingleton(categoriesHandler);

		var cut = Render<Edit>(parameters =>
				parameters.Add(p => p.Id, articleId.ToString()));

		await cut.WaitForAssertionAsync(() => cut.Find("form"));

		// Submit the form to trigger conflict
		var saveButton = cut.Find("button[type=submit]");
		await saveButton.ClickAsync();

		// Wait for conflict UI
		await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

		// Click Reload Latest
		var reloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reload Latest"));
		reloadButton.Should().NotBeNull();
		await reloadButton.ClickAsync();

		// After reload, the edit model title should reflect the server title
		await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Server Title"));
		cut.Markup.Should().Contain("Server Title");

		// Ensure the edit handler was called once (initial attempt)
		await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
	}

	[Fact]
	public async Task When_SaveSucceeds_NavigatesToDetails()
	{
		// Arrange
		var articleId = ObjectId.GenerateNewId();

		var article = new ArticleDto(
				articleId,
				"slug",
				"Title",
				"Intro",
				"Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				0
		);

		var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();
		getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(article));

		var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
		editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Result.Ok(article));

		var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>([]));

		Services.AddSingleton(getHandler);
		Services.AddSingleton(editHandler);
		Services.AddSingleton(categoriesHandler);

		var navMan = Services.GetRequiredService<NavigationManager>();

		// Act
		var cut = Render<Edit>(parameters =>
				parameters.Add(p => p.Id, articleId.ToString()));

		await cut.WaitForAssertionAsync(() => cut.Find("form"));

		var saveButton = cut.Find("button[type=submit]");
		await saveButton.ClickAsync();

		// Wait a short time for navigation
		await Task.Delay(50);

		// Assert
		navMan.Uri.Should().Contain($"/articles/details/{articleId}");
		await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
	}

	[Fact]
	public async Task When_ConflictAndUserCancels_NavigatesToDetails_WithoutRetry()
	{
		// Arrange
		var articleId = ObjectId.GenerateNewId();

		var initial = new ArticleDto(
				articleId,
				"initial_slug",
				"Initial Title",
				"Intro",
				"Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				0
		);

		var server = new ArticleDto(
				articleId,
				"initial_slug",
				"Server Title",
				"Intro",
				"Server Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				1
		);

		var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();
		getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

		var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, [ "Title", "Content" ]);
		var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();

		editHandler.HandleAsync(Arg.Any<ArticleDto>())
				.Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

		var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>([]));

		Services.AddSingleton(getHandler);
		Services.AddSingleton(editHandler);
		Services.AddSingleton(categoriesHandler);

		var navMan = Services.GetRequiredService<NavigationManager>();

		var cut = Render<Edit>(parameters =>
				parameters.Add(p => p.Id, articleId.ToString()));

		await cut.WaitForAssertionAsync(() => cut.Find("form"));

		// Trigger save to produce conflict
		var saveButton = cut.Find("button[type=submit]");
		await saveButton.ClickAsync();

		await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

		// Find and click Cancel in the conflict panel
		var cancelButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancel"));
		cancelButton.Should().NotBeNull();
		await cancelButton.ClickAsync();

		// Wait briefly for navigation
		await Task.Delay(50);

		// Assert navigation to details and only one edit attempt
		navMan.Uri.Should().Contain($"/articles/details/{articleId}");
		await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
	}

}
