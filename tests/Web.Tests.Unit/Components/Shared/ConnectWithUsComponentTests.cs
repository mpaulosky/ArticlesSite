// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ConnectWithUsComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

/// <summary>
/// Unit tests for ConnectWithUsComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class ConnectWithUsComponentTests : BunitContext
{
	[Fact]
	public void RendersHeaderText()
	{
		var cut = Render<ConnectWithUsComponent>();
		cut.Markup.Contains("Connect With Us");
	}

	[Fact]
	public void RendersAllSocialLinks()
	{
		var cut = Render<ConnectWithUsComponent>();
		Assert.Contains("https://www.threads/", cut.Markup);
		Assert.Contains("https://www.instagram.com/", cut.Markup);
		Assert.Contains("https://www.youtube.com/", cut.Markup);
	}

	[Fact]
	public void RendersAllSocialIcons()
	{
		var cut = Render<ConnectWithUsComponent>();
		Assert.Contains("ri-threads-line", cut.Markup);
		Assert.Contains("ri-instagram-line", cut.Markup);
		Assert.Contains("ri-youtube-line", cut.Markup);
	}
}
