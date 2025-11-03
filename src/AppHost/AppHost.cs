using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Web>("web");

var redisCache = builder.AddRedisServices();

var mongoDb = builder.AddMongoDbServices();

builder.AddProject<Projects.Web>(Website)
		.WithExternalHttpEndpoints()
		.WithHttpHealthCheck("/health")
		.WithReference(redisCache).WaitFor(redisCache)
		.WithReference(mongoDb).WaitFor(mongoDb);

builder.Build().Run();