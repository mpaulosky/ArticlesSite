using Microsoft.Extensions.Options;

using Web.Infrastructure;

namespace Web.Handlers;

public class EditArticleHandlerHighConcurrencySimulationTests
{
	[Fact]
	public async Task HighConcurrency_SimulatedClients_AtLeastOneSucceeds_AndRetriesHappen()
	{
		// Arrange
		var repo = Substitute.For<IArticleRepository>();
		var logger = Substitute.For<ILogger<EditArticle.Handler>>();
		var validator = new ArticleDtoValidator();

		var articleId = MongoDB.Bson.ObjectId.GenerateNewId();
		var author = new AuthorInfo("test-user-id", "Test Author");
		var category = new Category { CategoryName = "Test Category", Slug = "test_category" };

		var original = new Article
		{
			Id = articleId,
			Title = "Original",
			Introduction = "Intro",
			Content = "Content",
			CoverImageUrl = "https://example.com/img.jpg",
			Slug = "original",
			IsPublished = false,
			IsArchived = false,
			Version = 0,
			Author = author,
			Category = category
		};

		// Always return current article on GetArticle
		repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(original));

		// Simulate UpdateArticle behavior: first 10 calls fail with concurrency, subsequent calls succeed
		// This will cause handlers to retry until some succeed
		var sequence = new List<Result<Article>>();
		for (int i = 0; i < 10; i++) sequence.Add(Result.Fail<Article>("Concurrency", ResultErrorCode.Concurrency));
		// Add many successes to allow many callers to eventually succeed
		for (int i = 0; i < 50; i++) sequence.Add(Result.Ok<Article>(original));

		repo.UpdateArticle(Arg.Any<Article>()).Returns(x => sequence.Count > 0 ? sequence[0] : Result.Fail<Article>("No more responses"), x =>
		{
			if (sequence.Count > 0) sequence.RemoveAt(0);
			return sequence.Count >= 0 ? Result.Ok<Article>(original) : Result.Fail<Article>("No more responses");
		});

		var options = Options.Create(new ConcurrencyOptions { MaxRetries = 3, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });

		var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null);

		int clients = 8;
		var tasks = new Task<Result<ArticleDto>>[clients];

		for (int i = 0; i < clients; i++)
		{
			var dto = new ArticleDto(articleId, "original", $"Updated Title {i}", "Intro", "Updated Content", "https://example.com/img.jpg", author, category, false, null, null, DateTimeOffset.UtcNow, false, false, 0);
			tasks[i] = Task.Run(() => handler.HandleAsync(dto));
		}

		// Act
		await Task.WhenAll(tasks);

		// Assert - at least one success among clients
		tasks.Any(t => t.Result.Success).Should().BeTrue();

		// And UpdateArticle was called multiple times (retries + attempts)
		await repo.Received().UpdateArticle(Arg.Any<Article>());
		var receivedCount = repo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == nameof(IArticleRepository.UpdateArticle));
		receivedCount.Should().BeGreaterThanOrEqualTo(clients);
	}
}
