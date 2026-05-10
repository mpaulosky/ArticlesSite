// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ProgramSmokeTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =============================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Web.Startup;

[ExcludeFromCodeCoverage]
public class ProgramSmokeTests
{
	private sealed class TestFactory : WebApplicationFactory<Program>
	{
		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.ConfigureAppConfiguration((context, config) =>
			{
				var dict = new Dictionary<string, string?>
				{
					["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
					["Auth0:Domain"] = "test.auth0.com",
					["Auth0:ClientId"] = "test-client-id",
					["Auth0:ClientSecret"] = "test-client-secret"
				};
				config.AddInMemoryCollection(dict);
			});
		}
	}

	[Fact]
	public void App_Should_Start_Without_Errors()
	{
		string? origMongoDb = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
		string? origAuth0Domain = Environment.GetEnvironmentVariable("Auth0__Domain");
		string? origAuth0ClientId = Environment.GetEnvironmentVariable("Auth0__ClientId");
		string? origAuth0ClientSecret = Environment.GetEnvironmentVariable("Auth0__ClientSecret");
		try
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://localhost:27017");
			Environment.SetEnvironmentVariable("Auth0__Domain", "test.auth0.com");
			Environment.SetEnvironmentVariable("Auth0__ClientId", "test-client-id");
			Environment.SetEnvironmentVariable("Auth0__ClientSecret", "test-client-secret");
			using var factory = new TestFactory();
			var client = factory.CreateClient();
			Assert.NotNull(client);
		}
		finally
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", origMongoDb);
			Environment.SetEnvironmentVariable("Auth0__Domain", origAuth0Domain);
			Environment.SetEnvironmentVariable("Auth0__ClientId", origAuth0ClientId);
			Environment.SetEnvironmentVariable("Auth0__ClientSecret", origAuth0ClientSecret);
		}
	}
}
