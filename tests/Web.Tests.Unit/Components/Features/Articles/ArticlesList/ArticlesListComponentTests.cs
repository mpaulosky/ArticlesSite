using Web.Components.Features.Articles.ArticlesList;

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
						new AuthorInfo("user1", "User One"), null, true, null, null, null, false, true),
				new ArticleDto(ObjectId.GenerateNewId(), "slug2", "Title2", "Intro2", "Content2", "", 
						new AuthorInfo("user2", "User Two"), null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Assert
		cut.WaitForAssertion(() =>
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
		includeArchivedCheckbox!.Change(true);
		
		cut.WaitForState(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called twice: once on init (includeArchived=false), once after change (includeArchived=true)
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), false, false);
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), false, true);
	}

	[Fact]
	public async Task Should_Call_Handler_With_FilterByUser_When_Checkbox_Checked()
	{
		// Arrange
		var articles = new[]
		{
				new ArticleDto(ObjectId.GenerateNewId(), "slug1", "Title1", "Intro1", "Content1", "",
						new AuthorInfo("user1", "User One"), null, true, null, null, null, false, true)
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
		myArticlesCheckbox!.Change(true);
		
		cut.WaitForState(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called twice: once on init (filterByUser=false), once after change (filterByUser=true)
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), false, false);
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true, false);
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
		cut.WaitForAssertion(() =>
		{
			var emptyMessage = cut.Find("p");
			emptyMessage.TextContent.Should().Contain("No articles available yet");
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
		cut.WaitForAssertion(() =>
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

		// Wait for top bar to render (h1 'All Articles')
		cut.WaitForState(() => cut.FindAll("h1").Any(h => h.TextContent.Contains("All Articles")), timeout: TimeSpan.FromSeconds(2));

		// Assert
		cut.WaitForAssertion(() =>
		{
			if (roles.Contains("Admin") || roles.Contains("Author"))
			{
				var createButton = cut.FindAll("button.btn-secondary").FirstOrDefault(b => b.TextContent.Contains("Create"));
				createButton.Should().NotBeNull();
			}
			else
			{
				var buttons = cut.FindAll("button.btn-secondary").Where(b => b.TextContent.Contains("Create"));
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
						new AuthorInfo("user1", "User One"), null, true, null, null, null, false, true)
		};

		var mockHandler = Substitute.For<GetArticles.IGetArticlesHandler>();
		mockHandler.HandleAsync(Arg.Any<ClaimsPrincipal?>(), Arg.Any<bool>(), Arg.Any<bool>())
				.Returns(Result.Ok<IEnumerable<ArticleDto>>(articles));

		Services.AddSingleton(mockHandler);

		// Act
		var cut = Render<Web.Components.Features.Articles.ArticlesList.ArticlesList>();

		// Wait for initial render
		cut.WaitForState(() => cut.FindAll("input[type='checkbox']").Count == 2, timeout: TimeSpan.FromSeconds(2));

		// Find checkboxes
		var checkboxes = cut.FindAll("input[type='checkbox']");
		var myArticlesCheckbox = checkboxes.FirstOrDefault(c => 
				c.Parent?.TextContent.Contains("Show My Articles Only") == true);
		var includeArchivedCheckbox = checkboxes.FirstOrDefault(c => 
				c.Parent?.TextContent.Contains("Include Archived") == true);

		myArticlesCheckbox.Should().NotBeNull();
		includeArchivedCheckbox.Should().NotBeNull();

		// Trigger first checkbox change
		await cut.InvokeAsync(() => myArticlesCheckbox!.Change(true));
		cut.WaitForState(() => mockHandler.ReceivedCalls().Count() >= 2, timeout: TimeSpan.FromSeconds(2));
		
		// Re-find checkboxes after re-render
		checkboxes = cut.FindAll("input[type='checkbox']");
		includeArchivedCheckbox = checkboxes.FirstOrDefault(c => 
				c.Parent?.TextContent.Contains("Include Archived") == true);
		
		// Trigger second checkbox change
		await cut.InvokeAsync(() => includeArchivedCheckbox!.Change(true));
		cut.WaitForState(() => mockHandler.ReceivedCalls().Count() >= 3, timeout: TimeSpan.FromSeconds(2));

		// Assert - Handler should be called three times with different parameters
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), false, false); // Initial load
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true, false);  // After first checkbox
		await mockHandler.Received(1).HandleAsync(Arg.Any<ClaimsPrincipal?>(), true, true);   // After second checkbox
	}

}