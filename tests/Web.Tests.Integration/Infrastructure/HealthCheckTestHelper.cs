using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Tests.Integration.Infrastructure
{
	// Helper for running health checks in integration tests
	public static class HealthCheckTestHelper
	{

		public static async Task<HealthReport> RunHealthChecksAsync(IServiceProvider provider)
		{
			var healthCheckService = provider.GetRequiredService<HealthCheckService>();

			return await healthCheckService.CheckHealthAsync();
		}

	}
}
