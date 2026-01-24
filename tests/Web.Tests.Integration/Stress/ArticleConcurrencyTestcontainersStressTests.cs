using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

namespace Web.Tests.Integration.Stress;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleConcurrencyTestcontainersStressTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly MongoDbFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;

    public ArticleConcurrencyTestcontainersStressTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        // Don't use WithWebHostBuilder - just let the factory use the environment variables set by the fixture
        _factory = factory;
    }

	[Fact(Timeout = 120_000, Skip = "MongoDB container connection issue in test harness - article inserted into fixture's DB but app cannot find it in its connection. Requires further infrastructure investigation.")]
	public async Task Stress_ManyConcurrentApiWrites_UsingSharedTestFixture()
	{
		// Arrange - seed the database
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Version = 0;
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Verify the article was inserted by querying it back
		var verifyCollection = _fixture.Database.GetCollection<Article>("Articles");
		var insertedArticle = await verifyCollection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		if (insertedArticle == null)
		{
			throw new InvalidOperationException($"Article was not inserted! ID: {article.Id}, Database: {verifyCollection.Database.DatabaseNamespace.DatabaseName}");
		}

		using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

		const int clients = 100; // heavier stress
		var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
		var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>(clients);

		for (int i = 0; i < clients; i++)
		{
			var dto = new ArticleDto(
					article.Id,
					article.Slug,
					$"Stress Edit {i}",
					article.Introduction,
					article.Content,
					article.CoverImageUrl,
					article.Author,
					article.Category,
					article.IsPublished,
					article.PublishedOn,
					article.CreatedOn,
					DateTimeOffset.UtcNow,
					article.IsArchived,
					true,
					article.Version
			);

			tasks.Add(client.PutAsJsonAsync($"/api/articles/{article.Id}", dto, cts.Token));
		}

		// Act
		await Task.WhenAll(tasks);

		var responses = tasks.Select(t => t.Result).ToList();
		var successCount = responses.Count(r => r.IsSuccessStatusCode);
		var conflictCount = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Conflict);

		// Assert
		successCount.Should().BeGreaterThanOrEqualTo(1, "at least one of the concurrent updates should succeed");
		conflictCount.Should().BeGreaterThanOrEqualTo(1, "concurrent contention should produce at least one 409 conflict");

		// Verify final DB state
		var saved = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(cts.Token);
		saved.Should().NotBeNull();
		saved!.Version.Should().BeGreaterThanOrEqualTo(0);

		// If any conflict responses contained structured details, ensure serverVersion is present
		foreach (var resp in responses.Where(r => r.StatusCode == System.Net.HttpStatusCode.Conflict))
		{
			var conflict = await resp.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
			conflict.Should().NotBeNull();
			conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
		}
	}
}

