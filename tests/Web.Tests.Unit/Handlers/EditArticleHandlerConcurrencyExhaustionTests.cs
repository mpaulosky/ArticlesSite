// Removed redundant usings: moved to GlobalUsings.cs
namespace Web.Handlers;

public class EditArticleHandlerConcurrencyExhaustionTests
{
	[Fact]
	public async Task HandleAsync_ReturnsFailure_AfterMaxRetriesOnConcurrencyConflict()
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

		// GetArticleByIdAsync always returns the same article (so retries will still see a changed DB state simulated by UpdateArticle failures)
		repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(originalArticle));

		// UpdateArticle always fails with concurrency conflict
		repo.UpdateArticle(Arg.Any<Article>()).Returns(Result.Fail<Article>("Concurrency conflict: article was modified by another process", ResultErrorCode.Concurrency));

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
			false, // canEdit
			0 // version
		);

		// Act
		var result = await handler.HandleAsync(dto);

		// Assert - handler should ultimately return a failure with concurrency message
		result.Success.Should().BeFalse();
		result.Error.Should().Contain("Concurrency conflict");
		result.ErrorCode.Should().Be(Shared.Abstractions.ResultErrorCode.Concurrency);

		// UpdateArticle should have been attempted 4 times (1 initial + 3 maxRetries)
		await repo.Received(4).UpdateArticle(Arg.Any<Article>());
	}
}
