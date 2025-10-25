// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ResultTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using Shared.Abstractions;

namespace Shared.Tests.Unit.Abstractions;

/// <summary>
///   Unit tests for the <see cref="Result" /> class.
/// </summary>
public class ResultTests
{
	[Fact]
	public void Ok_ShouldCreateSuccessResult()
	{
		// Arrange & Act
		var result = Result.Ok();

		// Assert
		result.Success.Should().BeTrue();
		result.Failure.Should().BeFalse();
		result.Error.Should().BeNull();
	}

	[Fact]
	public void Fail_ShouldCreateFailureResult_WithErrorMessage()
	{
		// Arrange
		const string errorMessage = "Test error message";

		// Act
		var result = Result.Fail(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
	}

	[Fact]
	public void Failure_ShouldReturnOppositeOfSuccess()
	{
		// Arrange & Act
		var successResult = Result.Ok();
		var failureResult = Result.Fail("Error");

		// Assert
		successResult.Failure.Should().BeFalse();
		failureResult.Failure.Should().BeTrue();
	}
}

/// <summary>
///   Unit tests for the <see cref="Result{T}" /> class.
/// </summary>
public class ResultOfTTests
{
	[Fact]
	public void Ok_ShouldCreateSuccessResult_WithValue()
	{
		// Arrange
		const int expectedValue = 42;

		// Act
		var result = Result.Ok(expectedValue);

		// Assert
		result.Success.Should().BeTrue();
		result.Failure.Should().BeFalse();
		result.Error.Should().BeNull();
		result.Value.Should().Be(expectedValue);
	}

	[Fact]
	public void Fail_ShouldCreateFailureResult_WithErrorMessage()
	{
		// Arrange
		const string errorMessage = "Test error message";

		// Act
		var result = Result.Fail<int>(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
		result.Value.Should().Be(default);
	}

	[Fact]
	public void FromValue_ShouldCreateSuccessResult_WhenValueIsNotNull()
	{
		// Arrange
		const string value = "test value";

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
	}

	[Fact]
	public void FromValue_ShouldCreateFailureResult_WhenValueIsNull()
	{
		// Arrange
		string? value = null;

		// Act
		var result = Result.FromValue(value);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Provided value is null.");
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitConversion_ToValue_ShouldReturnValue()
	{
		// Arrange
		const int expectedValue = 42;
		var result = Result.Ok(expectedValue);

		// Act
		int? actualValue = result;

		// Assert
		actualValue.Should().Be(expectedValue);
	}

	[Fact]
	public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
	{
		// Arrange
		const int value = 42;

		// Act
		Result<int> result = value;

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be(value);
	}
}
