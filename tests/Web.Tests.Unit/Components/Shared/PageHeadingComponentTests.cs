// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     PageHeadingComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

/// <summary>
/// Unit tests for PageHeadingComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class PageHeadingComponentTests : BunitContext
{
	[Fact]
	public void RendersDefaultHeaderTextAndLevel()
	{
		var cut = Render<PageHeadingComponent>();
		cut.Find("h1").TextContent.Should().Be("My Blog");
		cut.Find("h1").ClassList.Should().Contain("text-gray-50");
	}

	[Theory]
	[InlineData("1", "h1", "text-3xl")]
	[InlineData("2", "h2", "text-2xl")]
	[InlineData("3", "h3", "text-1xl")]
	public void RendersCorrectHeaderLevelAndClass(string level, string expectedTag, string expectedClass)
	{
		var cut = Render<PageHeadingComponent>(parameters => parameters
				.Add(p => p.Level, level)
				.Add(p => p.HeaderText, "Test Heading"));
		cut.Find(expectedTag).TextContent.Should().Be("Test Heading");
		cut.Find(expectedTag).ClassList.Should().Contain(expectedClass);
	}

	[Fact]
	public void RendersCustomTextColorClass()
	{
		var cut = Render<PageHeadingComponent>(parameters => parameters
				.Add(p => p.TextColorClass, "text-blue-500"));
		cut.Find("h1").ClassList.Should().Contain("text-blue-500");
	}
}
