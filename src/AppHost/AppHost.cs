using AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var redisCache = builder.AddRedisServices();

var mongoDb = builder.AddMongoDbServices();

var webProject = builder.AddProject<Projects.Web>(Website)
	.WithExternalHttpEndpoints()
	.WithHttpHealthCheck("/health")
	.WithReference(redisCache).WaitFor(redisCache)
	.WithReference(mongoDb).WaitFor(mongoDb);
	
builder.Build().Run();
