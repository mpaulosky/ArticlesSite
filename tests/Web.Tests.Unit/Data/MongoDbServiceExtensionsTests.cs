//=======================================================
//Copyright (c) 2026. All rights reserved.
//File Name :     MongoDbServiceExtensionsTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

using Microsoft.AspNetCore.Builder;

using MongoDB.Driver;

namespace Web.Data;

[ExcludeFromCodeCoverage]
public class MongoDbServiceExtensionsTests
{

	[Fact]
	public void AddMongoDb_RegistersServices_WhenConfigProvided()
	{
		// Arrange
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);
		string json = "{ \"MongoDb\": { \"ConnectionString\": \"mongodb://host:27021\", \"Database\": \"TestDb\" } }";
		File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), json);

		var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = tempDir });

		// Act
		builder.AddMongoDb();

		using var sp = builder.Services.BuildServiceProvider();

		// Assert client
		var client = sp.GetRequiredService<IMongoClient>();
		var server = client.Settings.Servers.First();
		server.Host.Should().Be("host");
		server.Port.Should().Be(27021);

		using var scope = sp.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
		db.DatabaseNamespace.DatabaseName.Should().Be("TestDb");

		var ctx = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
		ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("TestDb");

		var factory = scope.ServiceProvider.GetRequiredService<IMongoDbContextFactory>();
		factory.CreateDbContext().Database.DatabaseNamespace.DatabaseName.Should().Be("TestDb");

		Directory.Delete(tempDir, true);
	}

	[Fact]
	public void AddMongoDb_UsesEnvironmentVariables_WhenProvided()
	{
		// Arrange
		string? origConn = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
		string? origDb = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

		try
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://envhost:27022");
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", "EnvDb");

			var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = Path.GetTempPath() });
			// Make test deterministic even if other configuration providers exist
			builder.Configuration["MongoDb:ConnectionString"] = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
			builder.Configuration["MongoDb:Database"] = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

			builder.AddMongoDb();

			using var sp = builder.Services.BuildServiceProvider();
			using var scope = sp.CreateScope();

			var client = sp.GetRequiredService<IMongoClient>();
			var server = client.Settings.Servers.First();

			var db = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
			db.DatabaseNamespace.DatabaseName.Should().Be("EnvDb");
		}
		finally
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", origConn);
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", origDb);
		}
	}

	[Fact]
	public void AddMongoDb_RuntimeDatabaseName_ReadsEnvironmentOnResolve()
	{
		// Arrange - set connection string via config and rely on runtime env var for DB name
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);
		string json = "{ \"MongoDb\": { \"ConnectionString\": \"mongodb://host:27023\" } }";
		File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), json);

		string? origDb = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");

		try
		{
			// Ensure any pre-existing env var is cleared so the first resolution uses default
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", null);

			var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ContentRootPath = tempDir });
			builder.AddMongoDb();

			using var sp = builder.Services.BuildServiceProvider();
			using var scope1 = sp.CreateScope();
			// No env var -> default articlesdb
			scope1.ServiceProvider.GetRequiredService<IMongoDatabase>().DatabaseNamespace.DatabaseName.Should().Be("articlesdb");

			// Now set env var and resolve again in a new scope
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", "RuntimeDb");
			using var scope2 = sp.CreateScope();
			scope2.ServiceProvider.GetRequiredService<IMongoDatabase>().DatabaseNamespace.DatabaseName.Should().Be("RuntimeDb");
			scope2.ServiceProvider.GetRequiredService<IMongoDbContext>().Database.DatabaseNamespace.DatabaseName.Should().Be("RuntimeDb");
			scope2.ServiceProvider.GetRequiredService<IMongoDbContextFactory>().CreateDbContext().Database.DatabaseNamespace.DatabaseName.Should().Be("RuntimeDb");
		}
		finally
		{
			Environment.SetEnvironmentVariable("MONGODB_DATABASE_NAME", origDb);
			Directory.Delete(tempDir, true);
		}
	}
}
