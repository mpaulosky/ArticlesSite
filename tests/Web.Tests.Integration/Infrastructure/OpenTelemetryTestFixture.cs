using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

namespace Web.Tests.Integration.Infrastructure
{
	// Simple OpenTelemetry tracer for integration tests
	public static class OpenTelemetryTestFixture
	{

		public static void AddTestOpenTelemetry(IServiceCollection services)
		{
			services.AddOpenTelemetry().WithTracing(builder =>
			{
				builder.AddSource("TestSource");
				builder.SetSampler(new AlwaysOnSampler());
			});
		}

	}
}
