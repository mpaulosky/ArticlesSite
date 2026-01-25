// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     LoadingComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for LoadingComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
[Obsolete]
public class LoadingComponentTests : BunitContext
{

	[Fact]
	public void LoadingComponent_ShouldRender_WithLoadingText()
	{
		// Act
		var cut = Render<LoadingComponent>();

		// Assert
		cut.Find("h3").TextContent.Trim().Should().Be("Loading...");
	}

	[Fact]
	public void LoadingComponent_ShouldHave_SpinningIcon()
	{
		// Act
		var cut = Render<LoadingComponent>();

		// Assert
		var svg = cut.Find("svg");
		svg.ClassList.Should().Contain("animate-spin");
	}

	[Fact]
	public void LoadingComponent_ShouldHave_CorrectStyling()
	{
		// Act
		var cut = Render<LoadingComponent>();

		// Assert
		var container = cut.Find("div");
		container.ClassList.Should().Contain("dark:bg-gray-800");
		container.ClassList.Should().Contain("rounded-md");
	}

}
