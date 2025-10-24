// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DatabaseService.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  AppHost
// =======================================================

using Aspire.Hosting;
using Aspire.Hosting.MongoDB;

namespace AppHost;

/// <summary>
///   Extension methods for adding and configuring MongoDB resources with local container support.
/// </summary>
public static class DatabaseService
{
	/// <summary>
	///   Adds MongoDB services to the distributed application builder, including resource tagging, grouping, and improved
	///   seeding logic.
	/// </summary>
	/// <param name="builder">The distributed application builder.</param>
	/// <returns>The MongoDB database resource builder.</returns>
	public static IResourceBuilder<MongoDBDatabaseResource> AddMongoDbServices(
		this IDistributedApplicationBuilder builder)
	{
		// Configure MongoDB with persistent container for local development
		var database = builder.AddMongoDB(ArticleConnect)
			.WithLifetime(ContainerLifetime.Persistent)
			.AddDatabase(DatabaseName);

		return database;
	}
}