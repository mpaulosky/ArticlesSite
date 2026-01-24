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
public sealed class MongoDbFixture : IAsyncLifetime
{

	private readonly MongoDbContainer _mongoContainer;
	private IMongoClient? _mongoClient;
	private IMongoDatabase? _database;
	private IMongoDbContextFactory? _contextFactory;
	private const string DatabaseNameValue = "articledb";

	public MongoDbFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:7.0")  // Use MongoDB 7.0 for better compatibility with current driver
			.Build();
	}

	/// <summary>
	///   Gets the MongoDB connection string for the container
	/// </summary>
	public string ConnectionString => _mongoContainer.GetConnectionString();

	/// <summary>
	///   Gets the configured MongoDB database instance
	/// </summary>
	public IMongoDatabase Database => _database
		?? throw new InvalidOperationException("Database not initialized. Call InitializeAsync first.");

	/// <summary>
	///   Gets the database name used for testing
	/// </summary>
	public string DatabaseName => DatabaseNameValue;

	/// <summary>
	///   Gets the context factory for creating MongoDbContext instances
	/// </summary>
	public IMongoDbContextFactory ContextFactory => _contextFactory
		?? throw new InvalidOperationException("Context factory not initialized. Call InitializeAsync first.");

	/// <summary>
	///   Initializes the MongoDB container and database
	/// </summary>
	public async ValueTask InitializeAsync()
	{
		await _mongoContainer.StartAsync();

		// Re-set environment variables after container starts in case timing matters
		Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", ConnectionString, EnvironmentVariableTarget.Process);
		Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", DatabaseNameValue, EnvironmentVariableTarget.Process);

		_mongoClient = new MongoClient(ConnectionString);
		_database = _mongoClient.GetDatabase(DatabaseNameValue);

		// Create context factory
		_contextFactory = new TestMongoDbContextFactory(_mongoClient, DatabaseNameValue);

		// Create collections
		await _database.CreateCollectionAsync("Articles");
		await _database.CreateCollectionAsync("Categories");
		await _database.CreateCollectionAsync("Authors");

		// Create indexes for better query performance
		var articlesCollection = _database.GetCollection<Article>("Articles");
		await articlesCollection.Indexes.CreateOneAsync(
			new CreateIndexModel<Article>(
				Builders<Article>.IndexKeys.Ascending(a => a.Title)));

		var categoriesCollection = _database.GetCollection<Category>("Categories");
		await categoriesCollection.Indexes.CreateOneAsync(
			new CreateIndexModel<Category>(
				Builders<Category>.IndexKeys.Ascending(c => c.CategoryName)));
	}

	/// <summary>
	///   Cleans up the MongoDB container
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await _mongoContainer.DisposeAsync();
	}

	/// <summary>
	///   Clears all data from the test collections
	/// </summary>
	public async Task ClearCollectionsAsync()
	{
		// Delete all documents from each collection and wait for completion
		var articlesTask = Database.GetCollection<Article>("Articles").DeleteManyAsync(FilterDefinition<Article>.Empty);
		var categoriesTask = Database.GetCollection<Category>("Categories").DeleteManyAsync(FilterDefinition<Category>.Empty);

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
