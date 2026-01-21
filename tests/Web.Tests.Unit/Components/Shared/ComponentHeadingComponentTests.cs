// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ComponentHeadingComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Shared;

namespace Web.Tests.Unit.Components.Shared;

/// <summary>
/// Unit tests for ComponentHeadingComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class ComponentHeadingComponentTests : BunitContext
{
	[Fact]
	public void RendersDefaultHeaderTextAndLevel()
	{
		var cut = Render<ComponentHeadingComponent>();
		cut.Find("h3").TextContent.Should().Be("My Component");
		cut.Find("h3").ClassList.Should().Contain("text-gray-50");
	}

	[Theory]
	[InlineData("1", "h1", "text-2xl")]
	[InlineData("2", "h2", "text-xl")]
	[InlineData("3", "h3", "text-lg")]
	[InlineData("4", "h4", "text-md")]
	[InlineData("5", "h5", "text-sm")]
	public void RendersCorrectHeaderLevelAndClass(string level, string expectedTag, string expectedClass)
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
				.Add(p => p.Level, level)
				.Add(p => p.HeaderText, "Test Heading"));
		cut.Find(expectedTag).TextContent.Should().Be("Test Heading");
		cut.Find(expectedTag).ClassList.Should().Contain(expectedClass);
	}

	[Fact]
	public void RendersCustomTextColorClass()
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
				.Add(p => p.TextColorClass, "text-blue-500"));
		cut.Find("h3").ClassList.Should().Contain("text-blue-500");
	}
}
