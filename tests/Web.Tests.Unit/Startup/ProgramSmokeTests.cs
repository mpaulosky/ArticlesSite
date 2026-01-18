using Microsoft.AspNetCore.Mvc.Testing;

namespace Web.Tests.Unit.Startup;

public class ProgramSmokeTests
{
	[Fact]
	public void App_Should_Start_Without_Errors()
	{
		using var factory = new WebApplicationFactory<Program>();
		var client = factory.CreateClient();
		Assert.NotNull(client);
	}
}
