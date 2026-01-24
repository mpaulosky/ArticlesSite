// Removed redundant usings: moved to GlobalUsings.cs
namespace Web.Tests.Unit.Handlers;

public class EditArticleHandlerConcurrencyRetryTests
{
	[Fact]
	public async Task HandleAsync_RetriesOnConcurrencyConflict_AndEventuallySucceeds()
	{
		// Arrange
		var repo = Substitute.For<IArticleRepository>();
		var logger = Substitute.For<ILogger<EditArticle.Handler>>();

		var articleId = ObjectId.GenerateNewId();
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("test-user-id", "Test Author");
		var category = new Category { CategoryName = "Technology" };

		var originalArticle = new Article()
		{
			Id = articleId,
			Title = "Original Title",
			Introduction = "Intro",
			Content = "Content",
			CoverImageUrl = "https://example.com/image.jpg",
			Slug = "original_title",
			IsPublished = false,
			IsArchived = false,
			Version = 0
		};

		var latestArticle = new Article()
		{
			Id = articleId,
			Title = "Original Title",
			Introduction = "Intro",
			Content = "Content",
			CoverImageUrl = "https://example.com/image.jpg",
			Slug = "original_title",
			IsPublished = false,
			IsArchived = false,
			Version = 1
		};

		// GetArticleByIdAsync should return original first, then latest on retry
		repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(originalArticle), Result.Ok<Article?>(latestArticle));

		// UpdateArticle should fail first with concurrency conflict (typed), then succeed
		var successResult = Result.Ok<Article>(latestArticle);
		repo.UpdateArticle(Arg.Any<Article>()).Returns(
			Result.Fail<Article>("Concurrency conflict: article was modified by another process", ResultErrorCode.Concurrency),
			successResult
		);

		var handler = new EditArticle.Handler(repo, logger, null);

		var dto = new ArticleDto(
			articleId,
			"original_title",
			"Updated Title",
			"Intro",
			"Updated Content",
			"https://example.com/updated-image.jpg", // coverImageUrl
			author,
			category,
			false, // isPublished
			null, // publishedOn
			null, // createdOn
			DateTimeOffset.UtcNow, // modifiedOn
			false, // isArchived
			false // canEdit
		);

		// Act
		var result = await handler.HandleAsync(dto);

		// Assert
		result.Success.Should().BeTrue();
		// UpdateArticle should have been attempted at least twice (initial + retry)
		repo.Received(2).UpdateArticle(Arg.Any<Article>());
	}
}
