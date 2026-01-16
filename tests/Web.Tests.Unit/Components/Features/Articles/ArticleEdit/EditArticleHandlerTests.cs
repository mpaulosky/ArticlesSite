using Web.Components.Features.Articles.ArticleEdit;


namespace Web.Tests.Unit.Components.Features.Articles.ArticleEdit;

[ExcludeFromCodeCoverage]
public class EditArticleHandlerTests
{

	[Fact]
	public async Task HandleAsync_WithInvalidDtoFields_ShouldReturnFailure()
	{
		var objectId = ObjectId.GenerateNewId();
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var invalidDto = new ArticleDto(
				objectId,
				"invalid-article",
				"", // Empty title
				"", // Empty introduction
				"", // Empty content
				"https://example.com/invalid.jpg",
				author,
				category,
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow.AddDays(-10),
				null,
				false,
				false
		);

		// Simulate repository returning a valid article for the ID
		var existingArticle =
				new Article("Old Title", "Intro", "Content", string.Empty, author, category) { Id = objectId };

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result.Ok<Article?>(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result.Ok(new Article())));

		var result = await _handler.HandleAsync(invalidDto);

		// The handler should throw or fail due to invalid fields (depending on business logic)
		// If validation is inside Article.Update, it should throw ArgumentException
		result.Success.Should().BeFalse();
		result.Error.Should().NotBeNull();
	}


	private readonly IArticleRepository _mockRepository;

	private readonly EditArticle.Handler _handler;

	public EditArticleHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		var mockLogger = Substitute.For<ILogger<EditArticle.Handler>>();
		_handler = new EditArticle.Handler(_mockRepository, mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
	{
		var objectId = ObjectId.GenerateNewId();
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var existingArticle =
				new Article("Old Title", "Intro", "Content", string.Empty, author, category) { Id = objectId };

		var articleDto = new ArticleDto(
				objectId,
				"updated-article",
				"Updated Article",
				"Updated Intro",
				"Updated Content",
				"https://example.com/updated.jpg",
				author,
				category,
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow.AddDays(-10),
				null,
				false,
				false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result.Ok<Article?>(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result.Ok(new Article())));
		var result = await _handler.HandleAsync(articleDto);
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).GetArticleByIdAsync(objectId);

		await _mockRepository.Received(1).UpdateArticle(Arg.Is<Article>(a =>
				a.Title == "Updated Article" &&
				a.Introduction == "Updated Intro" &&
				a.Slug == "updated_article"
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullRequest_ShouldReturnFailure()
	{
		ArticleDto? nullDto = null;
		var result = await _handler.HandleAsync(nullDto);
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Article data cannot be null");
		await _mockRepository.DidNotReceive().GetArticleByIdAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task HandleAsync_WhenArticleNotFound_ShouldReturnFailure()
	{
		var objectId = ObjectId.GenerateNewId();

		var articleDto = new ArticleDto(
				objectId,
				"test-article",
				"Test Article",
				"Test Intro",
				"Test Content",
				"",
				null,
				null,
				false,
				null,
				DateTimeOffset.UtcNow,
				null,
				false,
				false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result.Fail<Article?>("Article not found")));
		var result = await _handler.HandleAsync(articleDto);
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Article not found");
		await _mockRepository.DidNotReceive().UpdateArticle(Arg.Any<Article>());
	}

	[Fact]
	public async Task HandleAsync_WhenUpdateFails_ShouldReturnFailure()
	{
		var objectId = ObjectId.GenerateNewId();
		var existingArticle = new Article { Id = objectId, Title = "Old Title" };

		var articleDto = new ArticleDto(
				objectId,
				"test-article",
				"Test Article",
				"Test Intro",
				"Test Content",
				"",
				null,
				null,
				false,
				null,
				DateTimeOffset.UtcNow,
				null,
				false,
				false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result.Ok<Article?>(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Fail("Update failed")));
		var result = await _handler.HandleAsync(articleDto);
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Update failed");
	}

	[Fact]
	public async Task HandleAsync_ShouldUpdateAllProperties()
	{
		var objectId = ObjectId.GenerateNewId();
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var author = new AuthorInfo("user1", "Test Author");
		var categoryId = ObjectId.GenerateNewId();
		var category = new Category { Id = categoryId, CategoryName = "Tech" };
		var existingArticle = new Article { Id = objectId, Title = "Old Title" };

		var articleDto = new ArticleDto(
				objectId,
				"updated-slug",
				"Updated Title",
				"Updated Intro",
				"Updated Content",
				"https://example.com/updated.jpg",
				author,
				category,
				true,
				publishedOn,
				DateTimeOffset.UtcNow.AddDays(-10),
				null,
				false,
				false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result.Ok<Article?>(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result.Ok(new Article())));
		var result = await _handler.HandleAsync(articleDto);
		result.Success.Should().BeTrue();

		await _mockRepository.Received(1).UpdateArticle(Arg.Is<Article>(a =>
				a.Title == "Updated Title" &&
				a.Introduction == "Updated Intro" &&
				a.Content == "Updated Content" &&
				a.Slug == "updated_title" &&
				a.CoverImageUrl == "https://example.com/updated.jpg" &&
				a.Author == author &&
				a.Category == category &&
				a.IsPublished == true &&
				a.PublishedOn == publishedOn &&
				a.IsArchived == false &&
				a.ModifiedOn != null
		));
	}

}