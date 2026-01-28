// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbFixture.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Infrastructure;

/// <summary>
///   Provides a MongoDB container for integration tests using Testcontainers
/// </summary>
[ExcludeFromCodeCoverage]
public class MongoDbFixture : IAsyncLifetime
{

	private MongoDbContainer? _mongoDbContainer;
	private IMongoDbContextFactory? _contextFactory;

	/// <summary>
	/// Gets the MongoDB connection string from the running container.
	/// </summary>
	public string ConnectionString =>
			_mongoDbContainer?.GetConnectionString()
			?? throw new InvalidOperationException("MongoDB container not initialized");

	/// <summary>
	/// Gets the MongoDB client connected to the test container.
	/// </summary>
	private static IMongoClient MongoClient { get; set; } = null!;

	/// <summary>
	/// Gets the test database name.
	/// </summary>
	public static string DatabaseName => ArticleDatabase;

	/// <summary>
	/// Gets the MongoDB database for testing.
	/// </summary>
	public IMongoDatabase Database =>
		MongoClient?.GetDatabase(DatabaseName)
		?? throw new InvalidOperationException("MongoDB client not initialized");

	/// <summary>
	///   Gets the context factory for creating MongoDbContext instances
	/// </summary>
	public IMongoDbContextFactory ContextFactory =>
			_contextFactory
			?? throw new InvalidOperationException("Context factory not initialized. Call InitializeAsync first.");

	/// <summary>
	/// Initializes the MongoDB container before tests run.
	/// </summary>
	public async ValueTask InitializeAsync()
	{

		_mongoDbContainer = new MongoDbBuilder()
				.WithImage("mongo:8.0")
				.WithCleanUp(true)
				.WithPortBinding(0, 27017) // Use random available port mapped to container's 27017
				.Build();

		await _mongoDbContainer.StartAsync();

		MongoClient = new MongoClient(ConnectionString);

		try
		{
			// Verify connection
			await MongoClient.GetDatabase(DatabaseName).RunCommandAsync((Command<BsonDocument>)"{ping:1}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"MongoDB connection verification failed: {ex.Message}");

			return;
		}

		// Re-set environment variables after the container starts in case timing matters
		Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", ConnectionString,
				EnvironmentVariableTarget.Process);

		Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", DatabaseName, EnvironmentVariableTarget.Process);

		// Create a context factory
		_contextFactory = new TestMongoDbContextFactory(MongoClient, DatabaseName);

		// Create collections
		await Database.CreateCollectionAsync("Articles");
		await Database.CreateCollectionAsync("Categories");

		// Create indexes for better query performance
		var articlesCollection = Database.GetCollection<Article>("Articles");

		await articlesCollection.Indexes.CreateOneAsync(
				new CreateIndexModel<Article>(
						Builders<Article>.IndexKeys.Ascending(a => a.Title)));

		var categoriesCollection = Database.GetCollection<Category>("Categories");

		await categoriesCollection.Indexes.CreateOneAsync(
				new CreateIndexModel<Category>(
						Builders<Category>.IndexKeys.Ascending(c => c.CategoryName)));

	}

	/// <summary>
	/// Disposes the MongoDB container after tests complete.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		if (_mongoDbContainer is not null)
		{
			await _mongoDbContainer.DisposeAsync();
		}
	}

	/// <summary>
	/// Creates a new MongoDbContext for testing.
	/// </summary>
	public IMongoDbContext CreateDbContext()
	{
		return new MongoDbContext(MongoClient, DatabaseName);
	}

	/// <summary>
	///   Clears all data from the test collections
	/// </summary>
	public async Task ClearCollectionsAsync()
	{
		// Delete all documents from each collection and wait for completion
		var articlesTask = Database.GetCollection<Article>("Articles").DeleteManyAsync(FilterDefinition<Article>.Empty);

		var categoriesTask =
				Database.GetCollection<Category>("Categories").DeleteManyAsync(FilterDefinition<Category>.Empty);

		// Wait for all deletions to complete
		await Task.WhenAll(articlesTask, categoriesTask);

		// Add a small delay to ensure MongoDB has processed the deletions
		await Task.Delay(50);
	}

	/// <summary>
	///   Test-specific MongoDB context factory
	/// </summary>
	private sealed class TestMongoDbContextFactory : IMongoDbContextFactory
	{

		private readonly IMongoClient _client;

		private readonly string _databaseName;

		public TestMongoDbContextFactory(IMongoClient client, string databaseName)
		{
			_client = client;
			_databaseName = databaseName;
		}

		public IMongoDbContext CreateDbContext()
		{
			return new MongoDbContext(_client, _databaseName);
		}

	}
}
