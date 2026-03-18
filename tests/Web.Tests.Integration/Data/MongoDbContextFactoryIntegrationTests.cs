// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MongoDbContextFactoryIntegrationTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Data;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class MongoDbContextFactoryIntegrationTests
{
	private readonly MongoDbFixture _fixture;

	public MongoDbContextFactoryIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
	}

	[Fact]
	public void CreateDbContext_UsesContainerConnectionString()
	{
		// Arrange - the fixture sets env vars after starting the container
		// Arrange - set env vars expected by MongoDbContextFactory
		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");
		try
		{
			Environment.SetEnvironmentVariable("DefaultConnection", _fixture.ConnectionString);
			Environment.SetEnvironmentVariable("DatabaseName", MongoDbFixture.DatabaseName);

			// Act
			var factory = new MongoDbContextFactory();
			var ctx = factory.CreateDbContext();

			// Assert
			ctx.Should().NotBeNull();
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be(MongoDbFixture.DatabaseName);
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Should().NotBeNull();
			server.Host.Should().NotBeNullOrWhiteSpace();
		}
		finally
		{
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
		}
	}

	[Fact]
	public void CreateDbContext_RespectsRuntimeDatabaseName_WhenEnvVarChanged()
	{
		// Arrange
		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			Environment.SetEnvironmentVariable("DefaultConnection", _fixture.ConnectionString);
			Environment.SetEnvironmentVariable("DatabaseName", MongoDbFixture.DatabaseName);

			// Now change the runtime DB name
			Environment.SetEnvironmentVariable("DatabaseName", "runtime_db_test");

			var factory = new MongoDbContextFactory();
			var ctx = factory.CreateDbContext();

			// Assert
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("runtime_db_test");
		}
		finally
		{
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
		}
	}

	[Fact]
	public void CreateDbContext_WithDevAppSettings_PrefersDevelopment()
	{
		// Arrange - create appsettings files in temp dir
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		try
		{
			string baseJson = "{ \"DefaultConnection\": \"mongodb://apphost:27019\", \"DatabaseName\": \"AppDb\" }";
			string devJson = "{ \"DefaultConnection\": \"mongodb://devhost:27021\", \"DatabaseName\": \"DevDb\" }";
			File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), baseJson);
			File.WriteAllText(Path.Combine(tempDir, "appsettings.Development.json"), devJson);

			Directory.SetCurrentDirectory(tempDir);

			var factory = new MongoDbContextFactory();
			var ctx = factory.CreateDbContext();

			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("DevDb");
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Directory.Delete(tempDir, true);
		}
	}
}
