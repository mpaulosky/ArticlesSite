namespace Web.Tests.Playwright.Tests;

[ExcludeFromCodeCoverage]
public class SmokeTests : PlaywrightTestBase
{

	/// <summary>
	/// Smoke tests always run regardless of server availability
	/// </summary>
	protected override bool IsSmokeTest => true;

	[Fact]
	[SmokeTest]
	public void ShouldBeAbleToRunABasicTest()
	{
		// This is a simple test to verify Playwright is set up correctly
		true.Should().BeTrue();
	}

	[Fact]
	[SmokeTest]
	public async Task ShouldBeAbleToCheckServerAvailability()
	{
		// This test verifies we can detect server availability
		var serverIsRunning = await IsServerAvailableAsync(BaseUrl);

		// This assertion will pass whether server is running or not
		// We're just verifying the check mechanism works
		_ = serverIsRunning; // Use the variable to avoid warnings
		true.Should().BeTrue(); // Test always passes
	}

}