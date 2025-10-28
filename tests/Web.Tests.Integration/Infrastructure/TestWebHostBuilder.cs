using Microsoft.AspNetCore.Hosting;

namespace Web.Tests.Integration.Infrastructure
{
	/// <summary>
	///   Test web application factory for integration testing
	/// </summary>
	public class TestWebHostBuilder : WebApplicationFactory<IAppMarker>
	{

		private Action<IServiceCollection>? _configureServices;

		public static TestWebHostBuilder Create() => new();

		public TestWebHostBuilder WithServices(Action<IServiceCollection> configure)
		{
			_configureServices += configure;

			return this;
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			if (_configureServices != null)
			{
				builder.ConfigureServices(_configureServices);
			}
		}

		public TestWebHostBuilder Build() => this;

	}
}