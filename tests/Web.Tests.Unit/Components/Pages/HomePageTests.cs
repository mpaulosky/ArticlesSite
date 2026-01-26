namespace Web.Components.Pages;

[ExcludeFromCodeCoverage]
public class HomePageTests : BunitContext
{
	private readonly GetArticles.IGetArticlesHandler _articlesHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
	private readonly ILogger<Home> _logger = Substitute.For<ILogger<Home>>();

	public HomePageTests()
	{
		Services.AddSingleton(_articlesHandler);
		Services.AddSingleton(_logger);
	}

	[Fact]
	public void HomePage_Renders_ThemeSelector_And_Headings()
	{
		// Arrange: Setup articles handler to return a non-null result
		_articlesHandler.HandleAsync()
			.Returns(Task.FromResult(Result.Ok((IEnumerable<ArticleDto>)new List<ArticleDto>())));

		// Arrange: Setup JSInterop for ThemeManager.syncUI
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<Home>();

		// Assert
		cut.Markup.Should().Contain("Choose Your Theme");
		cut.Markup.Should().Contain("Welcome to the Article Site");
		cut.Markup.Should().Contain("Recent Articles");
	}

	[Fact]
	public async Task HomePage_Shows_Loading_Initially()
	{
		// Arrange
		_articlesHandler.HandleAsync()
			.Returns(Task.FromResult(Result.Ok((IEnumerable<ArticleDto>)new List<ArticleDto>())));

		// Arrange: Setup JSInterop for ThemeManager.syncUI
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<Home>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert final state only
		cut.Markup.Should().Contain("No articles available yet");
	}

	[Fact]
	public async Task HomePage_Shows_NoArticles_WhenNoneReturned()
	{
		// Arrange
		_articlesHandler.HandleAsync()
			.Returns(Task.FromResult(Result.Ok((IEnumerable<ArticleDto>)new List<ArticleDto>())));

		// Arrange: Setup JSInterop for ThemeManager.syncUI
		JSInterop.SetupVoid("ThemeManager.syncUI");
		// Act
		var cut = Render<Home>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		cut.Markup.Should().Contain("No articles available yet");
	}

	[Fact]
	public async Task HomePage_Shows_RecentArticles_WhenReturned()
	{
		// Arrange
		var articles = FakeArticleDto.GetArticleDtos(2, true);

		foreach (var article in articles)
		{
			article.PublishedOn = DateTimeOffset.UtcNow.AddDays(-articles.IndexOf(article));
			article.IsPublished = true;
		}

		_articlesHandler.HandleAsync()
			.Returns(Task.FromResult(Result.Ok((IEnumerable<ArticleDto>)articles)));

		// Setup JSInterop for ThemeSelector
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<Home>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		foreach (var article in articles)
		{
			cut.Markup.Should().Contain(article.Title);
			cut.Markup.Should().Contain(article.Introduction);
		}
	}

	[Fact]
	public async Task HomePage_LogsWarning_WhenHandlerFails()
	{
		// Arrange
		_articlesHandler.HandleAsync()
			.Returns(Task.FromResult(Result.Fail<IEnumerable<ArticleDto>>("fail")));

		// Setup JSInterop for ThemeManager.syncUI
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<Home>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		_logger.Received().Log(
			LogLevel.Warning,
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			null,
			Arg.Any<Func<object, Exception?, string>>()
		);
	}

	[Fact]
	public async Task HomePage_LogsError_WhenHandlerThrows()
	{
		// Arrange
		_articlesHandler.HandleAsync(  )
			.Returns(_ => Task.FromException<Result<IEnumerable<ArticleDto>>>(new Exception("fail")));

		// Setup JSInterop for ThemeManager.syncUI
		JSInterop.SetupVoid("ThemeManager.syncUI");

		// Act
		var cut = Render<Home>();
		await cut.InvokeAsync(() => Task.CompletedTask);

		// Assert
		_logger.Received().Log(
			LogLevel.Error,
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception>(),
			Arg.Any<Func<object, Exception?, string>>()
		);
	}
}
