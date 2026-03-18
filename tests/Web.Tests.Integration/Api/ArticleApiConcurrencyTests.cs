using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

namespace Web.Tests.Integration.Api;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleApiConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly MongoDbFixture _fixture;
	private readonly WebApplicationFactory<Program> _factory;

	public ArticleApiConcurrencyTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
	{
		_fixture = fixture;

		// Configure the app to use the same MongoDB instance as the integration fixture
		_factory = factory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureAppConfiguration((context, conf) =>
					{
						var dict = new Dictionary<string, string?>
						{
							["MongoDb:ConnectionString"] = _fixture.ConnectionString,
							["MongoDb:Database"] = _fixture.Database.DatabaseNamespace.DatabaseName
						};
						conf.AddInMemoryCollection(dict);
					});

			// Override the IMongoDbContextFactory with the test fixture's factory so the app uses the Testcontainer MongoDB instance
			builder.ConfigureServices(services =>
					{
						// Remove the default factory registration
						var factoryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMongoDbContextFactory));
						if (factoryDescriptor != null)
						{
							services.Remove(factoryDescriptor);
						}

						// Register the test fixture's context factory
						services.AddSingleton(_fixture.ContextFactory);

						// For this integration test, disable retries so that concurrency conflicts are returned immediately to the client
						// This allows us to test the 409 conflict response behavior instead of having the handler retry on every conflict
						var optionsDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions>));
						if (optionsDescriptor != null)
						{
							services.Remove(optionsDescriptor);
						}
						services.Configure<Web.Infrastructure.ConcurrencyOptions>(opts =>
								{
									opts.MaxRetries = 0; // Disable retries for testing concurrent updates
								});
					});
		});
	}

	[Fact]
	public async Task ConcurrentApiUpdates_OneSucceeds_OtherReturnsConflict()
	{
		// Arrange - clear and insert article
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var client = _factory.CreateClient();

		var editDto1 = new ArticleDto(article.Id, article.Slug, "Edit Title A", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);
		var editDto2 = new ArticleDto(article.Id, article.Slug, "Edit Title B", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);

		// Act - send both updates concurrently
		var task1 = client.PutAsJsonAsync($"/api/articles/{article.Id}", editDto1, TestContext.Current.CancellationToken);
		var task2 = client.PutAsJsonAsync($"/api/articles/{article.Id}", editDto2, TestContext.Current.CancellationToken);

		await Task.WhenAll(task1, task2);

		var resp1 = await task1;
		var resp2 = await task2;

		// Debug: Log response details to understand which stage failed
		var stage1 = resp1.Headers.FirstOrDefault(h => h.Key == "X-Debug-Stage").Value.FirstOrDefault() ?? "unknown";
		var stage2 = resp2.Headers.FirstOrDefault(h => h.Key == "X-Debug-Stage").Value.FirstOrDefault() ?? "unknown";
		var errorCode1 = resp1.Headers.FirstOrDefault(h => h.Key == "X-Debug-ErrorCode").Value.FirstOrDefault() ?? "none";
		var errorCode2 = resp2.Headers.FirstOrDefault(h => h.Key == "X-Debug-ErrorCode").Value.FirstOrDefault() ?? "none";
		Console.WriteLine($"[TEST] Resp1: Status={resp1.StatusCode} Stage={stage1} ErrorCode={errorCode1}");
		Console.WriteLine($"[TEST] Resp2: Status={resp2.StatusCode} Stage={stage2} ErrorCode={errorCode2}");

		// Assert - at least one success, at least one 409 conflict
		(resp1.IsSuccessStatusCode || resp2.IsSuccessStatusCode).Should().BeTrue();
		(resp1.StatusCode == System.Net.HttpStatusCode.Conflict || resp2.StatusCode == System.Net.HttpStatusCode.Conflict).Should().BeTrue();

		// If conflict response contains ConcurrencyConflictResponseDto, validate structure
		if (resp1.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			var conflict = await resp1.Content.ReadFromJsonAsync<ConcurrencyConflictResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
			conflict.Should().NotBeNull();
			conflict.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
		}

		if (resp2.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			var conflict = await resp2.Content.ReadFromJsonAsync<ConcurrencyConflictResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
			conflict.Should().NotBeNull();
			conflict.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
		}

		// Verify the article exists in DB
		var saved = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		saved.Should().NotBeNull();
	}
}
