using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisServices();

var database = builder.AddMongoDbServices();

//var webapi = builder.AddProject<Projects.Api>("api")
//	.WithExternalHttpEndpoints()
//	.WithHttpHealthCheck("/health")
//	.WithReference(database)
//	.WaitFor(database);

// Add a composite command that coordinates multiple operations
builder.AddProject<Projects.Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(cache).WaitFor(cache)
		.WithReference(database).WaitFor(database);
		//.WithReference(webapi).WaitFor(webapi);

builder.Build().Run();