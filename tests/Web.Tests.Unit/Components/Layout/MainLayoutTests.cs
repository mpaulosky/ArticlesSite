// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MainLayoutTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Layout;

/// <summary>
/// Unit tests for MainLayout component
/// </summary>
[ExcludeFromCodeCoverage]
public class MainLayoutTests
{
	[Fact]
	public void GetErrorCode_WithNullException_ShouldReturn500()
	{
		// Arrange
		Exception? exception = null;

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	[Fact]
	public void GetErrorCode_WithUnauthorizedAccessException_ShouldReturn401()
	{
		// Arrange
		var exception = new UnauthorizedAccessException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(401);
	}

	[Fact]
	public void GetErrorCode_WithArgumentException_ShouldReturn400()
	{
		// Arrange
		var exception = new ArgumentException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(400);
	}

	[Fact]
	public void GetErrorCode_WithKeyNotFoundException_ShouldReturn404()
	{
		// Arrange
		var exception = new KeyNotFoundException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(404);
	}

	[Fact]
	public void GetErrorCode_WithInvalidOperationException_ShouldReturn500()
	{
		// Arrange
		var exception = new InvalidOperationException();

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	[Fact]
	public void GetErrorCode_WithGenericException_ShouldReturn500()
	{
		// Arrange
		var exception = new Exception("Generic error");

		// Act
		var result = InvokeGetErrorCode(exception);

		// Assert
		result.Should().Be(500);
	}

	// Helper method to test the private static GetErrorCode method via reflection
	private static int InvokeGetErrorCode(Exception? exception)
	{
		var mainLayoutType = typeof(Web.Components.Layout.MainLayout);
		var method = mainLayoutType.GetMethod("GetErrorCode",
			BindingFlags.NonPublic | BindingFlags.Static);

		method.Should().NotBeNull("GetErrorCode method should exist");

		var result = method!.Invoke(null, new object?[] { exception });
		return (int)result!;
	}
}
