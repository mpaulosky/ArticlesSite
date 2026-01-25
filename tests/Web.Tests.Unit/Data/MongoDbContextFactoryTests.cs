//=======================================================
//Copyright (c) 2026. All rights reserved.
//File Name :     MongoDbContextFactoryTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

namespace Web.Data;

[ExcludeFromCodeCoverage]
public class MongoDbContextFactoryTests
{
	[Fact]
	public void CreateDbContext_NoConfigOrEnv_UsesDefaults()
	{
		// Arrange
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			Directory.SetCurrentDirectory(tempDir);
			Environment.SetEnvironmentVariable("DefaultConnection", null);
			Environment.SetEnvironmentVariable("DatabaseName", null);

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert
			ctx.Should().NotBeNull();
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("ArticleSiteDb");

			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("localhost");
			server.Port.Should().Be(27017);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void CreateDbContext_UsesEnvironmentVariables_WhenSet()
	{
		// Arrange
		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			Environment.SetEnvironmentVariable("DefaultConnection", "mongodb://envhost:27018");
			Environment.SetEnvironmentVariable("DatabaseName", "EnvDb");

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("EnvDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("envhost");
			server.Port.Should().Be(27018);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
		}
	}

	[Fact]
	public void CreateDbContext_AppSettingsUsed_WhenEnvNotSet()
	{
		// Arrange
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			// Create an appsettings.json in a temp dir
			string json = "{\"DefaultConnection\": \"mongodb://apphost:27019\", \"DatabaseName\": \"AppDb\" }";
			File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), json);

			Directory.SetCurrentDirectory(tempDir);
			Environment.SetEnvironmentVariable("DefaultConnection", null);
			Environment.SetEnvironmentVariable("DatabaseName", null);

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert - appsettings should be used when env is not set
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("AppDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("apphost");
			server.Port.Should().Be(27019);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void CreateDbContext_EnvironmentOverridesAppSettings_WhenBothSet()
	{
		// Arrange
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			// Create an appsettings.json in a temp dir
			string json = "{\"DefaultConnection\": \"mongodb://apphost:27019\", \"DatabaseName\": \"AppDb\" }";
			File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), json);

			Directory.SetCurrentDirectory(tempDir);
			Environment.SetEnvironmentVariable("DefaultConnection", "mongodb://envhost:27018");
			Environment.SetEnvironmentVariable("DatabaseName", "EnvDb");

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert - environment variables override appsettings since AddEnvironmentVariables was added last
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("EnvDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("envhost");
			server.Port.Should().Be(27018);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void CreateDbContext_WithArgs_DelegatesToParameterless()
	{
		// Arrange
		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			Environment.SetEnvironmentVariable("DefaultConnection", "mongodb://envhost:27018");
			Environment.SetEnvironmentVariable("DatabaseName", "EnvDb");

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext(new string[] { "--noop" });

			// Assert
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("EnvDb");
		}
		finally
		{
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
		}
	}

	[Fact]
	public void CreateDbContext_UsesConnectionStringFromConnectionStringsSection()
	{
		// Arrange
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			string json = "{\"ConnectionStrings\": { \"DefaultConnection\": \"mongodb://connhost:27020\", \"DatabaseName\": \"ConnDb\" } }";
			File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), json);

			Directory.SetCurrentDirectory(tempDir);
			Environment.SetEnvironmentVariable("DefaultConnection", null);
			Environment.SetEnvironmentVariable("DatabaseName", null);

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert - should read from ConnectionStrings section via GetConnectionString
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("ConnDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("connhost");
			server.Port.Should().Be(27020);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void CreateDbContext_DevelopmentSettingsOverride_AppSettings()
	{
		// Arrange - appsettings.json and appsettings.Development.json both present; Development should override
		string originalDir = Directory.GetCurrentDirectory();
		string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDir);

		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			string baseJson = "{ \"DefaultConnection\": \"mongodb://apphost:27019\", \"DatabaseName\": \"AppDb\" }";
			string devJson = "{ \"DefaultConnection\": \"mongodb://devhost:27021\", \"DatabaseName\": \"DevDb\" }";
			File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), baseJson);
			File.WriteAllText(Path.Combine(tempDir, "appsettings.Development.json"), devJson);

			Directory.SetCurrentDirectory(tempDir);
			Environment.SetEnvironmentVariable("DefaultConnection", null);
			Environment.SetEnvironmentVariable("DatabaseName", null);

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert - development settings should override appsettings
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("DevDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("devhost");
			server.Port.Should().Be(27021);
		}
		finally
		{
			Directory.SetCurrentDirectory(originalDir);
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
			Directory.Delete(tempDir, true);
		}
	}

	[Fact]
	public void CreateDbContext_WhitespaceEnvConnectionString_IsIgnored_UsesDefaults()
	{
		// Arrange
		string? origConn = Environment.GetEnvironmentVariable("DefaultConnection");
		string? origDb = Environment.GetEnvironmentVariable("DatabaseName");

		try
		{
			Environment.SetEnvironmentVariable("DefaultConnection", "   ");
			Environment.SetEnvironmentVariable("DatabaseName", null);

			var factory = new MongoDbContextFactory();

			// Act
			IMongoDbContext ctx = factory.CreateDbContext();

			// Assert - whitespace env var should be treated as empty and defaults applied
			ctx.Database.DatabaseNamespace.DatabaseName.Should().Be("ArticleSiteDb");
			var server = ctx.Database.Client.Settings.Servers.First();
			server.Host.Should().Be("localhost");
			server.Port.Should().Be(27017);
		}
		finally
		{
			Environment.SetEnvironmentVariable("DefaultConnection", origConn);
			Environment.SetEnvironmentVariable("DatabaseName", origDb);
		}
	}
}
