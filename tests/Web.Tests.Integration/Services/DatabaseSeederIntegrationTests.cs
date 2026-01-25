using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using MongoDB.Driver;

using Web.Components.Features.Articles.Models;
using Web.Services;
using Web.Tests.Integration.Infrastructure;

using Xunit;

namespace Web.Tests.Integration.Services;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class DatabaseSeederIntegrationTests
{
	private readonly MongoDbFixture _fixture;

	public DatabaseSeederIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public async Task SeedAsync_InsertsCategoriesAndArticle_WhenEmpty()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var seeder = new DatabaseSeeder(_fixture.Database, NullLogger<DatabaseSeeder>.Instance);

		// Act
		await seeder.SeedAsync();

		// Assert
		var categoriesCount = await _fixture.Database.GetCollection<Category>("Categories").CountDocumentsAsync(FilterDefinition<Category>.Empty, cancellationToken: TestContext.Current.CancellationToken);
		var articlesCount = await _fixture.Database.GetCollection<Article>("Articles").CountDocumentsAsync(FilterDefinition<Article>.Empty, cancellationToken: TestContext.Current.CancellationToken);

		categoriesCount.Should().BeGreaterThanOrEqualTo(7L); // embedded JSON contains at least 7 categories
		articlesCount.Should().Be(1L);

		var article = await _fixture.Database.GetCollection<Article>("Articles").Find(FilterDefinition<Article>.Empty).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		article.Should().NotBeNull();
		article!.Title.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task SeedAsync_IsIdempotent_WhenCalledMultipleTimes()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var seeder = new DatabaseSeeder(_fixture.Database, NullLogger<DatabaseSeeder>.Instance);

		// Act
		await seeder.SeedAsync();
		var categoriesCountAfterFirst = await _fixture.Database.GetCollection<Category>("Categories").CountDocumentsAsync(FilterDefinition<Category>.Empty, cancellationToken: TestContext.Current.CancellationToken);
		var articlesCountAfterFirst = await _fixture.Database.GetCollection<Article>("Articles").CountDocumentsAsync(FilterDefinition<Article>.Empty, cancellationToken: TestContext.Current.CancellationToken);

		await seeder.SeedAsync();
		var categoriesCountAfterSecond = await _fixture.Database.GetCollection<Category>("Categories").CountDocumentsAsync(FilterDefinition<Category>.Empty, cancellationToken: TestContext.Current.CancellationToken);
		var articlesCountAfterSecond = await _fixture.Database.GetCollection<Article>("Articles").CountDocumentsAsync(FilterDefinition<Article>.Empty, cancellationToken: TestContext.Current.CancellationToken);

		// Assert
		categoriesCountAfterSecond.Should().Be(categoriesCountAfterFirst);
		articlesCountAfterSecond.Should().Be(articlesCountAfterFirst);
	}
}
