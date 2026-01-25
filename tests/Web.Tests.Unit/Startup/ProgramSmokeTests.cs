using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Web.Startup;

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
					["MongoDb:ConnectionString"] = "mongodb://localhost:27017"
				};
				config.AddInMemoryCollection(dict);
			});
		}
	}

	[Fact]
	public void App_Should_Start_Without_Errors()
	{
		string? orig = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
		try
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", "mongodb://localhost:27017");
			using var factory = new TestFactory();
			var client = factory.CreateClient();
			Assert.NotNull(client);
		}
		finally
		{
			Environment.SetEnvironmentVariable("MONGODB_CONNECTION_STRING", orig);
		}
	}
}
