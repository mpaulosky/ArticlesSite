// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AppHost.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  AppHost
// =======================================================

using AppHost;



IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);


IResourceBuilder<MongoDBDatabaseResource> database = builder.AddMongoDbServices();
// Explicitly use ServiceDefaults.RedisServices to avoid ambiguity
IResourceBuilder<RedisResource> cache = AppHost.RedisServices.AddRedisServices(builder);

// Add a composite command that coordinates multiple operations
var website = builder.AddProject("Web", "Website")
		.WithHttpHealthCheck("/health")
		.WithReference(database).WaitFor(database)
		.WithReference(cache).WaitFor(cache);

builder.Build().Run();