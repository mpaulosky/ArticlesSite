using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddMongoDbServices();

var cache = builder.AddRedisServices();

// Add a composite command that coordinates multiple operations
builder.AddProject<Projects.Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(database).WaitFor(database)
		.WithReference(cache).WaitFor(cache);

builder.Build().Run();