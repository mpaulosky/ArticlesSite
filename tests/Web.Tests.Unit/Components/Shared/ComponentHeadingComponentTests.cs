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
	public void RendersWithDefaultValues()
	{
		var cut = Render<ComponentHeadingComponent>();

		cut.Find("h1").TextContent.Should().Be("My Blog");
	}

	[Fact]
	public void RendersDefaultHeaderTextWhenLevelAndTextProvided()
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "3")
			.Add(p => p.HeaderText, "My Component"));

		cut.Find("h3").TextContent.Should().Be("My Component");
	}

	[Theory]
	[InlineData("1", "h1", "text-3xl")]
	[InlineData("2", "h2", "text-2xl")]
	[InlineData("3", "h3", "text-1xl")]
	[InlineData("4", "h4", "text-lg")]
	[InlineData("5", "h5", "text-md")]
	public void RendersCorrectHeaderLevelWithCorrectTextSize(string level, string expectedTag, string expectedClass)
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, level)
			.Add(p => p.HeaderText, "Test Heading"));

		var element = cut.Find(expectedTag);
		element.TextContent.Should().Be("Test Heading");
		element.ClassList.Should().Contain(expectedClass);
	}

	[Theory]
	[InlineData("1")]
	[InlineData("2")]
	[InlineData("3")]
	[InlineData("4")]
	[InlineData("5")]
	public void AllHeaderLevelsHaveCommonStyling(string level)
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, level)
			.Add(p => p.HeaderText, "Test"));

		var element = cut.FindAll("h1, h2, h3, h4, h5").First();
		element.ClassList.Should().Contain("font-semibold");
		element.ClassList.Should().Contain("tracking-tight");
		element.ClassList.Should().Contain("py-4");
	}

	[Fact]
	public void RendersHeaderElementWithinHeaderTag()
	{
		var cut = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "2")
			.Add(p => p.HeaderText, "Section Title"));

		var headerElement = cut.Find("header");
		var h2Element = headerElement.QuerySelector("h2");

		h2Element.Should().NotBeNull();
		h2Element!.TextContent.Should().Be("Section Title");
	}

	[Fact]
	public void HeaderElementHasCorrectMarginAndPadding()
	{
		var cut = Render<ComponentHeadingComponent>();

		var headerElement = cut.Find("header");
		headerElement.ClassList.Should().Contain("mb-6");
		headerElement.ClassList.Should().Contain("py-4");
	}

	[Fact]
	public void RendersDifferentHeaderWhenLevelChanges()
	{
		var cutLevel1 = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "1")
			.Add(p => p.HeaderText, "Heading 1"));

		cutLevel1.Find("h1").TextContent.Should().Be("Heading 1");

		var cutLevel2 = Render<ComponentHeadingComponent>(parameters => parameters
			.Add(p => p.Level, "2")
			.Add(p => p.HeaderText, "Heading 2"));

		cutLevel2.Find("h2").TextContent.Should().Be("Heading 2");
		cutLevel2.FindAll("h1").Should().BeEmpty();
	}

}
