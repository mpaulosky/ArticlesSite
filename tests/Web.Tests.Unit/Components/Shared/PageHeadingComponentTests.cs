// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     PageHeadingComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

[ExcludeFromCodeCoverage]
public class PageHeadingComponentTests : BunitContext
{
	[Fact]
	public void RendersDefaultHeaderTextAndLevel()
	{
		var cut = Render<PageHeadingComponent>();
		cut.Find("h1").TextContent.Should().Be("My Blog");
	}

	[Theory]
	[InlineData("1", "h1", "My Blog")]
	[InlineData("2", "h2", "My Blog")]
	[InlineData("3", "h3", "My Blog")]
	public void RendersCorrectHeaderLevel(string level, string expectedTag, string expectedText)
	{
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.Level, level)
			.Add(p => p.HeaderText, expectedText));
		cut.Find(expectedTag).TextContent.Should().Be(expectedText);
	}

	[Theory]
	[InlineData("1", "h1", "text-3xl text-red-500")]
	[InlineData("2", "h2", "text-2xl text-blue-500")]
	[InlineData("3", "h3", "text-1xl text-green-500")]
	public void RendersCorrectHeaderLevelAndClass(string level, string expectedTag, string expectedClass)
	{
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.Level, level)
			.Add(p => p.HeaderText, "Test Heading")
			.Add(p => p.TextColorClass, expectedClass));

		var element = cut.Find(expectedTag);
		element.TextContent.Should().Be("Test Heading");
		element.ClassList.Should().Contain(expectedClass.Split(' ')[0]);
	}

	[Fact]
	public void DoesNotRenderHeader_ForUnknownLevel()
	{
		var cut = Render<PageHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "99")
			.Add(p => p.HeaderText, "Should Not Render"));
		cut.Markup.Should().NotContain("Should Not Render");
	}
}
