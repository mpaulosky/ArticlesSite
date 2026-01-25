// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     PageHeadingComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for PageHeadingComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class PageHeadingComponentTests : BunitContext
{
	[Fact]
	public void RendersDefaultHeaderTextAndLevel()
	{
		var cut = Render<ComponentHeadingComponent>();
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

	[Theory]
	[InlineData("1", "h1", "text-3xl")]
	[InlineData("2", "h2", "text-2xl")]
	[InlineData("3", "h3", "text-1xl")]
	public void RendersCorrectHeaderLevelAndClass(string level, string expectedTag, string expectedClass)
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
				.Add(p => p.Level, level)
				.Add(p => p.HeaderText, "Test Heading"));

		var element = cut.Find(expectedTag);
		element.TextContent.Should().Be("Test Heading");
		element.ClassList.Should().Contain(expectedClass);
	}

}
