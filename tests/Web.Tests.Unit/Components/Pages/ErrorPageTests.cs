// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ErrorPageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using System.Diagnostics;

using Web.Components.Pages;

namespace Web.Tests.Unit.Components.Pages;

/// <summary>
/// Unit tests for the Error page component.
/// Tests cover rendering, RequestId display, and Activity scenarios.
/// </summary>
[ExcludeFromCodeCoverage]
public class ErrorPageTests : BunitContext
{
	public ErrorPageTests()
	{
		// Ensure clean state for each test
		Activity.Current = null;
	}

	[Fact]
	public void ErrorPage_Renders_ErrorHeadingsAndContent()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		cut.Markup.Should().Contain("Error.");
		cut.Markup.Should().Contain("An error occurred while processing your request.");
		cut.Markup.Should().Contain("Development Mode");
		cut.Markup.Should().Contain("The Development environment shouldn't be enabled for deployed applications.");
	}

	[Fact]
	public void ErrorPage_Renders_WithoutErrors()
	{
		// Act
		var cut = Render<Error>();

		// Assert - Verify component renders successfully
		cut.Should().NotBeNull();
		cut.Markup.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ErrorPage_HasCorrectCssClasses()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		var errorHeadings = cut.FindAll("h1.text-danger, h2.text-danger");
		errorHeadings.Should().HaveCountGreaterThanOrEqualTo(2, "there should be error headings with danger styling");
	}

	[Fact]
	public void ErrorPage_DoesNotShowRequestId_WhenNoActivityOrHttpContext()
	{
		// Arrange
		Activity.Current = null;

		// Act
		var cut = Render<Error>();

		// Assert
		cut.Markup.Should().NotContain("Request ID:");
		cut.Markup.Should().NotContain("<strong>Request ID:</strong>");
	}

	[Fact]
	public void ErrorPage_ShowsRequestId_WhenActivityCurrentExists()
	{
		// Arrange
		var activity = new Activity("TestOperation");
		activity.Start();

		try
		{
			// Act
			var cut = Render<Error>();

			// Assert
			cut.Markup.Should().Contain("Request ID:");
			cut.Markup.Should().Contain("<strong>Request ID:</strong>");
			if (activity.Id is not null)
			{
				cut.Markup.Should().Contain(activity.Id);
			}
		}
		finally
		{
			activity.Stop();
			Activity.Current = null;
		}
	}

	[Fact]
	public void ErrorPage_RendersRequestIdInCodeElement_WhenActivityExists()
	{
		// Arrange
		var activity = new Activity("TestOperation");
		activity.Start();

		try
		{
			// Act
			var cut = Render<Error>();

			// Assert - RequestId should be in a <code> element
			var codeElements = cut.FindAll("code");
			codeElements.Should().NotBeEmpty();
			if (activity.Id is not null)
			{
				codeElements.Should().Contain(element => element.TextContent.Contains(activity.Id));
			}
		}
		finally
		{
			activity.Stop();
			Activity.Current = null;
		}
	}

	[Fact]
	public void ErrorPage_ContainsDevelopmentEnvironmentGuidance()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		cut.Markup.Should().Contain("ASPNETCORE_ENVIRONMENT");
		cut.Markup.Should().Contain("Development");
		cut.Markup.Should().Contain("sensitive information");
	}

	[Fact]
	public void ErrorPage_HasCorrectStructure()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		cut.FindAll("h1").Should().ContainSingle();
		cut.FindAll("h2").Should().ContainSingle();
		cut.FindAll("h3").Should().ContainSingle();
		cut.FindAll("p").Count.Should().BeGreaterThanOrEqualTo(2, "should have multiple paragraphs of content");
	}

	[Fact]
	public void ErrorPage_RequestIdSection_IsConditional()
	{
		// Arrange - Test with no RequestId
		Activity.Current = null;
		var cut1 = Render<Error>();

		// Assert - No RequestId section
		cut1.FindAll("strong").Should().NotContain(e => e.TextContent == "Request ID:");

		// Arrange - Test with RequestId
		var activity = new Activity("TestOperation");
		activity.Start();

		try
		{
			var cut2 = Render<Error>();

			// Assert - RequestId section present
			cut2.FindAll("strong").Should().Contain(e => e.TextContent == "Request ID:");
		}
		finally
		{
			activity.Stop();
			Activity.Current = null;
		}
	}

	[Fact]
	public void ErrorPage_ShowsSecurityWarning_AboutDevelopmentEnvironment()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		cut.Markup.Should().Contain("shouldn't be enabled for deployed applications");
		cut.Markup.Should().Contain("displaying sensitive information");
	}

	[Fact]
	public void ErrorPage_ProvidesInstructions_ForEnablingDevelopmentMode()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		cut.Markup.Should().Contain("setting the");
		cut.Markup.Should().Contain("environment variable");
		cut.Markup.Should().Contain("restarting the app");
	}

	[Fact]
	public void ErrorPage_HasDangerStyling_ForBothErrorMessages()
	{
		// Act
		var cut = Render<Error>();

		// Assert
		var h1 = cut.Find("h1");
		h1.ClassList.Should().Contain("text-danger");
		h1.TextContent.Should().Contain("Error.");

		var h2 = cut.Find("h2");
		h2.ClassList.Should().Contain("text-danger");
		h2.TextContent.Should().Contain("An error occurred");
	}

	[Fact]
	public void ErrorPage_ComponentLifecycle_InitializesRequestId()
	{
		// Arrange
		var activity = new Activity("LifecycleTest");
		activity.Start();

		try
		{
			// Act - Render triggers OnInitialized which sets RequestId
			var cut = Render<Error>();

			// Assert - Verify RequestId was set during initialization
			if (activity.Id is not null)
			{
				cut.Markup.Should().Contain(activity.Id, "RequestId should be set from Activity.Current.Id during OnInitialized");
			}
		}
		finally
		{
			activity.Stop();
			Activity.Current = null;
		}
	}

	[Fact]
	public void ErrorPage_ShowRequestId_Property_ReturnsFalse_WhenRequestIdIsNull()
	{
		// Arrange
		Activity.Current = null;

		// Act
		var cut = Render<Error>();

		// Assert - ShowRequestId should evaluate to false, hiding the RequestId section
		var requestIdSection = cut.FindAll("p").Where(p => p.TextContent.Contains("Request ID:"));
		requestIdSection.Should().BeEmpty("ShowRequestId should be false when RequestId is null or empty");
	}

	[Fact]
	public void ErrorPage_ShowRequestId_Property_ReturnsTrue_WhenRequestIdHasValue()
	{
		// Arrange
		var activity = new Activity("RequestIdTest");
		activity.Start();

		try
		{
			// Act
			var cut = Render<Error>();

			// Assert - ShowRequestId should evaluate to true, showing the RequestId section
			cut.Markup.Should().Contain("Request ID:", "ShowRequestId should be true when RequestId has a value");
		}
		finally
		{
			activity.Stop();
			Activity.Current = null;
		}
	}
}

