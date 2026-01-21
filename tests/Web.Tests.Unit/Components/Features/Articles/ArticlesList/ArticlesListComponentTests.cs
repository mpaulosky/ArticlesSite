namespace Web.Tests.Unit.Components.Features.Articles.ArticlesList;

using Bunit;
[ExcludeFromCodeCoverage]
public class ArticlesListComponentTests : BunitContext
{

	[Fact]
	public async Task Should_Display_Articles_When_Handler_Returns_Success()
	{
		// Arrange
		var articles = new[]
		{
				new ArticleDto(ObjectId.GenerateNewId(), "slug1", "Title1", "Intro1", "Content1", "",
						new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "User One"), null, true, null, null, null, false, true),
				new ArticleDto(ObjectId.GenerateNewId(), "slug2", "Title2", "Intro2", "Content2", "",
						new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user2", "User Two"), null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			var titleElements = cut.FindAll("h2");
			titleElements.Should().HaveCount(2);
			titleElements[0].TextContent.Should().Be("Title1");
			titleElements[1].TextContent.Should().Be("Title2");
		});
	}

	[Fact]
	public async Task Should_Call_Handler_With_IncludeArchived_When_Checkbox_Checked()
	{
		// Arrange
		var articles = new[]
		{
				new ArticleDto(ObjectId.GenerateNewId(), "slug1", "Title1", "Intro1", "Content1", "",
						null, null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Find and check the 'Include Archived' checkbox
		var checkboxes = cut.FindAll("input[type='checkbox']");
		var includeArchivedCheckbox = checkboxes.FirstOrDefault(c =>
				c.Parent?.TextContent.Contains("Include Archived") == true);

		includeArchivedCheckbox.Should().NotBeNull();
		await includeArchivedCheckbox.ChangeAsync(true);

		await cut.WaitForStateAsync(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called twice: once on init (includeArchived=false), once after change (includeArchived=true)
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>());
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), false, true);
	}

	[Fact]
	public async Task Should_Call_Handler_With_FilterByUser_When_Checkbox_Checked()
	{
		// Arrange
		var articles = new[]
		{
				new ArticleDto(ObjectId.GenerateNewId(), "slug1", "Title1", "Intro1", "Content1", "",
						new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "User One"), null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Find and check the 'Show My Articles Only' checkbox
		var checkboxes = cut.FindAll("input[type='checkbox']");
		var myArticlesCheckbox = checkboxes.FirstOrDefault(c =>
				c.Parent?.TextContent.Contains("Show My Articles Only") == true);

		myArticlesCheckbox.Should().NotBeNull();
		await myArticlesCheckbox.ChangeAsync(true);

		await cut.WaitForStateAsync(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called twice: once on init (filterByUser=false), once after change (filterByUser=true)
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>());
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true);
	}

	[Fact]
	public async Task Should_Display_Empty_State_When_No_Articles()
	{
		// Arrange
		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok(Enumerable.Empty<ArticleDto>()));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			var emptyMessage = cut.Find("p");
			emptyMessage.TextContent.Should().Match("*No*articles available yet*");
		});
	}

	[Fact]
	public async Task Should_Display_Error_When_Handler_Returns_Failure()
	{
		// Arrange
		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Fail<IEnumerable<ArticleDto>>("Database connection failed"));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Error loading articles");
			cut.Markup.Should().Contain("Database connection failed");
		});
	}

	[Theory]
	[InlineData("Admin")]
	[InlineData("Author")]
	[InlineData("User")]
	[InlineData("Admin", "Author")]
	public async Task Should_Display_Create_Button_Based_On_Role(params string[] roles)
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", roles);
		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok(Enumerable.Empty<ArticleDto>()));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<CascadingAuthenticationState>(parameters =>
			parameters.AddChildContent<Web.Components.Features.Articles.ArticlesList.ArticlesList>());

		// Wait for the top bar to render (h1 'All Articles')
		await cut.WaitForStateAsync(() => cut.FindAll("h1").Any(h => h.TextContent.Contains("All Articles")), timeout: TimeSpan.FromSeconds(2));

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			if (roles.Contains("Admin") || roles.Contains("Author"))
			{
				var createButton = cut.FindAll("button.btn-primary").FirstOrDefault(b => b.TextContent.Contains("Create"));
				createButton.Should().NotBeNull();
			}
			else
			{
				var buttons = cut.FindAll("button.btn-primary").Where(b => b.TextContent.Contains("Create"));
				buttons.Should().BeEmpty();
			}
		}, timeout: TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Should_Reload_Articles_When_Both_Filters_Change()
	{
		// Arrange
		var articles = new[]
		{
				new ArticleDto(ObjectId.GenerateNewId(), "slug1", "Title1", "Intro1", "Content1", "",
						new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "User One"), null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Wait for initial render
		await cut.WaitForStateAsync(() => cut.FindAll("input[type='checkbox']").Count == 2, timeout: TimeSpan.FromSeconds(2));

		// Find checkboxes
		var checkboxes = cut.FindAll("input[type='checkbox']");
		var myArticlesCheckbox = checkboxes.FirstOrDefault(c =>
				c.Parent?.TextContent.Contains("Show My Articles Only") == true);
		var includeArchivedCheckbox = checkboxes.FirstOrDefault(c =>
				c.Parent?.TextContent.Contains("Include Archived") == true);

		myArticlesCheckbox.Should().NotBeNull();
		includeArchivedCheckbox.Should().NotBeNull();

		// Trigger the first checkbox change
		await cut.InvokeAsync(() => myArticlesCheckbox.Change(true));
		await cut.WaitForStateAsync(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));

		// Re-find checkboxes after re-render
		checkboxes = cut.FindAll("input[type='checkbox']");
		includeArchivedCheckbox = checkboxes.FirstOrDefault(c =>
				c.Parent?.TextContent.Contains("Include Archived") == true);

		// Trigger the second checkbox change
		await cut.InvokeAsync(() => includeArchivedCheckbox!.Change(true));
		await cut.WaitForStateAsync(() => mockHandler.ReceivedCalls().Count() >= 3, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called three times with different parameters
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>()); // Initial load
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true);  // After the first checkbox
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true, true);   // After the second checkbox
	}

}
