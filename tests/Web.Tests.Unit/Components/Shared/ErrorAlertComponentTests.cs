// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ErrorAlertComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for ErrorAlertComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
[Obsolete]
public class ErrorAlertComponentTests : BunitContext
{

	[Fact]
	public void ErrorAlertComponent_ShouldRender_WithDefaultValues()
	{
		// Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		cut.Find("h3").TextContent.Should().Be("Error");
		cut.Find("div.text-red-700").TextContent.Trim().Should().Be("An error occurred.");
	}

	[Fact]
	public void ErrorAlertComponent_ShouldRender_WithCustomTitleAndMessage()
	{
		// Arrange
		var title = "Custom Error";
		var message = "Something went wrong!";

		// Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Title, title)
			.Add(p => p.Message, message));

		// Assert
		cut.Find("h3").TextContent.Should().Be(title);
		cut.Find("div.text-red-700").TextContent.Trim().Should().Be(message);
	}

	[Fact]
	public void ErrorAlertComponent_ShouldRender_WithChildContent()
	{
		// Arrange
		var title = "Validation Error";
		var childContentText = "Custom child content here";

		// Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Title, title)
			.AddChildContent(childContentText));

		// Assert
		cut.Find("h3").TextContent.Should().Be(title);
		cut.Find("div.text-red-700").TextContent.Trim().Should().Be(childContentText);
	}

	[Fact]
	public void ErrorAlertComponent_ShouldHave_CorrectStyling()
	{
		// Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		var container = cut.Find("div.bg-red-50");
		container.ClassList.Should().Contain("border-red-200");
		container.ClassList.Should().Contain("rounded-md");
	}

	[Fact]
	public void ErrorAlertComponent_ShouldHave_ErrorIcon()
	{
		// Act
		var cut = Render<ErrorAlertComponent>();

		// Assert
		var svg = cut.Find("svg");
		svg.ClassList.Should().Contain("text-red-500");
	}

	[Fact]
	public void ErrorAlertComponent_ChildContent_ShouldOverride_MessageParameter()
	{
		// Arrange
		var message = "This message should not appear";
		var childContent = "This should appear instead";

		// Act
		var cut = Render<ErrorAlertComponent>(parameters => parameters
			.Add(p => p.Message, message)
			.AddChildContent(childContent));

		// Assert
		cut.Find("div.text-red-700").TextContent.Trim().Should().Be(childContent);
		cut.Markup.Should().NotContain(message);
	}

}
