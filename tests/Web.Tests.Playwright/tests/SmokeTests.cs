using FluentAssertions;
using Web.Tests.Playwright.PageObjects;

namespace Web.Tests.Playwright.Tests;

public class SmokeTests : PlaywrightTestBase
{
    [Fact]
    public void ShouldBeAbleToRunABasicTest()
    {
        // This is a simple test to verify Playwright is set up correctly
        true.Should().BeTrue();
    }
}
