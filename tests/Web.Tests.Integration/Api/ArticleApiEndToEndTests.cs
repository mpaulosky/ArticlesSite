using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

namespace Web.Tests.Integration.Api;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleApiEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly MongoDbFixture _fixture;
	private readonly WebApplicationFactory<Program> _factory;

	public ArticleApiEndToEndTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
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
	public async Task EndToEnd_ConcurrentUpdate_OneSucceeds_OtherGets409()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Ensure the article is actually in the database
		var verify = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		verify.Should().NotBeNull("Article should be inserted into test database");

		var client = _factory.CreateClient();

		var dto1 = new ArticleDto(article.Id, article.Slug, "E2E Title 1", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);
		var dto2 = new ArticleDto(article.Id, article.Slug, "E2E Title 2", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);

		// Act
		var t1 = client.PutAsJsonAsync($"/api/articles/{article.Id}", dto1, TestContext.Current.CancellationToken);
		var t2 = client.PutAsJsonAsync($"/api/articles/{article.Id}", dto2, TestContext.Current.CancellationToken);
		await Task.WhenAll(t1, t2);

		var r1 = await t1;
		var r2 = await t2;

		// Assert at least one success and at least one 409
		(r1.IsSuccessStatusCode || r2.IsSuccessStatusCode).Should().BeTrue();
		(r1.StatusCode == System.Net.HttpStatusCode.Conflict || r2.StatusCode == System.Net.HttpStatusCode.Conflict).Should().BeTrue();

		// If we got Conflict, ensure the response contains a structured payload
		if (r1.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			var conflict = await r1.Content.ReadFromJsonAsync<ConcurrencyConflictResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
			conflict.Should().NotBeNull();
			conflict.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
		}

		if (r2.StatusCode == System.Net.HttpStatusCode.Conflict)
		{
			var conflict = await r2.Content.ReadFromJsonAsync<ConcurrencyConflictResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
			conflict.Should().NotBeNull();
			conflict.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
		}
	}
}
