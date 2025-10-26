// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AppHost.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  AppHost
// =======================================================

#region

using AppHost;

using Projects;

#endregion

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<MongoDBDatabaseResource> database = builder.AddMongoDbServices();

IResourceBuilder<RedisResource> cache = builder.AddRedisServices();

// Add a composite command that coordinates multiple operations
builder.AddProject<Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(database).WaitFor(database)
		.WithReference(cache).WaitFor(cache);

builder.Build().Run();