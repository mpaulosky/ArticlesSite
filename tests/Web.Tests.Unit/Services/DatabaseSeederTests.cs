using MongoDB.Driver;

namespace Web.Services;

[ExcludeFromCodeCoverage]
public class DatabaseSeederTests
{
	[Fact]
	public async Task SeedAsync_DoesNotSeed_WhenCollectionsAlreadyContainData()
	{
		// Arrange
		var database = Substitute.For<IMongoDatabase>();
		var categoriesCollection = Substitute.For<IMongoCollection<Category>>();
		var articlesCollection = Substitute.For<IMongoCollection<Article>>();

		// Return counts > 0 for both collections
		categoriesCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Category>>(), Arg.Any<CountOptions>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(1L));
		articlesCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Article>>(), Arg.Any<CountOptions>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(1L));

		database.GetCollection<Category>("Categories", Arg.Any<MongoCollectionSettings>()).Returns(categoriesCollection);
		database.GetCollection<Article>("Articles", Arg.Any<MongoCollectionSettings>()).Returns(articlesCollection);

		var logger = Substitute.For<ILogger<DatabaseSeeder>>();

		var seeder = new DatabaseSeeder(database, logger);

		// Act
		await seeder.SeedAsync();

		// Assert
		// Insert methods should not be called
		await categoriesCollection.DidNotReceive().InsertManyAsync(Arg.Any<IEnumerable<Category>>(), Arg.Any<InsertManyOptions>(), Arg.Any<CancellationToken>());
		await articlesCollection.DidNotReceive().InsertOneAsync(Arg.Any<Article>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>());

		// Logger should be informed that seeding was skipped
		logger.Received().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
		logger.Received().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());

		// Verify the actual log messages were produced
		logger.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Log" && ((c.GetArguments().Length > 2 && c.GetArguments()[2]?.ToString()?.Contains("Categories already seeded.") == true))).Should().BeTrue();
		logger.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Log" && ((c.GetArguments().Length > 2 && c.GetArguments()[2]?.ToString()?.Contains("Articles already seeded.") == true))).Should().BeTrue();
	}

	[Fact]
	public async Task SeedAsync_InsertsCategoriesAndArticle_WhenCollectionsEmpty()
	{
		// Arrange
		var database = Substitute.For<IMongoDatabase>();
		var categoriesCollection = Substitute.For<IMongoCollection<Category>>();
		var articlesCollection = Substitute.For<IMongoCollection<Article>>();

		// Counts are zero to trigger seeding
		categoriesCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Category>>(), Arg.Any<CountOptions>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(0L));
		articlesCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Article>>(), Arg.Any<CountOptions>(), Arg.Any<CancellationToken>())
				.Returns(Task.FromResult(0L));

		database.GetCollection<Category>("Categories", Arg.Any<MongoCollectionSettings>()).Returns(categoriesCollection);
		database.GetCollection<Article>("Articles", Arg.Any<MongoCollectionSettings>()).Returns(articlesCollection);

		var logger = Substitute.For<ILogger<DatabaseSeeder>>();

		var seeder = new DatabaseSeeder(database, logger);

		// Capture what is inserted
		List<Category>? insertedCategories = null;
		categoriesCollection.InsertManyAsync(Arg.Do<IEnumerable<Category>>(x => insertedCategories = x.ToList()), Arg.Any<InsertManyOptions>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

		Article? insertedArticle = null;
		articlesCollection.InsertOneAsync(Arg.Do<Article>(a => insertedArticle = a), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

		// Act
		await seeder.SeedAsync();

		// Assert
		insertedCategories.Should().NotBeNull();
		insertedCategories!.Count.Should().BeGreaterThan(0); // there are several categories in the embedded json

		insertedArticle.Should().NotBeNull();

		// Verify logger reported seeding via recorded calls
		logger.Received().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());
		logger.Received().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Any<object>(), null, Arg.Any<Func<object, Exception?, string>>());

		logger.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Log" && ((c.GetArguments().Length > 2 && c.GetArguments()[2]?.ToString()?.Contains("Seeded") == true && c.GetArguments()[2]?.ToString()?.Contains("categories") == true))).Should().BeTrue();
		logger.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Log" && ((c.GetArguments().Length > 2 && c.GetArguments()[2]?.ToString()?.Contains("Seeded one article.") == true))).Should().BeTrue();
	}
}
