namespace Web.Components.Layout;

[ExcludeFromCodeCoverage]
public class FooterComponentTests : BunitContext
{
	[Fact]
	public void RendersCompanyYearAndDotNetVersion()
	{
		// Act
		var cut = Render<FooterComponent>();

		// Assert - copyright and company
		var markup = cut.Markup;
		markup.Should().Contain(DateTime.Now.Year.ToString());
		markup.Should().Contain("MPaulosky Co.");

		// Assert .NET major version is rendered
		markup.Should().Contain($".NET {Environment.Version.Major}");
	}

	[Fact]
	public void SocialLinks_HaveExpectedAttributes()
	{
		// Act
		var cut = Render<FooterComponent>();

		// GitHub
		var github = cut.Find("a[aria-label='GitHub']");
		github.GetAttribute("href").Should().Be("https://github.com");
		github.GetAttribute("target").Should().Be("_blank");
		github.GetAttribute("rel").Should().Contain("noopener");

		// Twitter
		var twitter = cut.Find("a[aria-label='Twitter']");
		twitter.GetAttribute("href").Should().Be("https://twitter.com");
		twitter.GetAttribute("target").Should().Be("_blank");
		twitter.GetAttribute("rel").Should().Contain("noopener");

		// LinkedIn
		var linkedin = cut.Find("a[aria-label='LinkedIn']");
		linkedin.GetAttribute("href").Should().Be("https://linkedin.com");
		linkedin.GetAttribute("target").Should().Be("_blank");
		linkedin.GetAttribute("rel").Should().Contain("noopener");

		// Email
		var email = cut.Find("a[aria-label='Email']");
		email.GetAttribute("href").Should().StartWith("mailto:");
		email.GetAttribute("target").Should().BeNull();
	}

	[Fact]
	public void Footer_NavigationLinksPresent()
	{
		// Act
		var cut = Render<FooterComponent>();

		cut.Find("a[href='#features']").TextContent.Should().Contain("Features");
		cut.Find("a[href='#how-it-works']").TextContent.Should().Contain("How It Works");
		cut.Find("a[href='/changelog']").TextContent.Should().Contain("Changelog");

		cut.Find("a[href='/privacy']").TextContent.Should().Contain("Privacy Policy");
		cut.Find("a[href='/terms']").TextContent.Should().Contain("Terms of Service");
		cut.Find("a[href='/cookies']").TextContent.Should().Contain("Cookie Policy");
	}
}
