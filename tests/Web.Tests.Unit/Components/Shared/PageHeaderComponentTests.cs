// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     PageHeaderComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

/// <summary>
/// Unit tests for PageHeaderComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class PageHeaderComponentTests : BunitContext
{
	[Fact]
	public void RendersDefaultHeaderTextAndLevel()
	{
		var cut = Render<PageHeaderComponent>();
		cut.Find("h1").TextContent.Should().Be("My Blog");
	}

	[Theory]
	[InlineData("1", "h1", "My Blog")]
	[InlineData("2", "h2", "My Blog")]
	[InlineData("3", "h3", "My Blog")]
	public void RendersCorrectHeaderLevel(string level, string expectedTag, string expectedText)
	{
		var cut = Render<PageHeaderComponent>(parameters => parameters
				.Add(p => p.Level, level)
				.Add(p => p.HeaderText, expectedText));
		cut.Find(expectedTag).TextContent.Should().Be(expectedText);
	}

	[Fact]
	public void RendersCustomHeaderText()
	{
		var cut = Render<PageHeaderComponent>(parameters => parameters
				.Add(p => p.HeaderText, "Welcome to Articles!"));
		cut.Find("h1").TextContent.Should().Be("Welcome to Articles!");
	}
}
