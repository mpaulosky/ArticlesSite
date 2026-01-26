//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     ResultTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using System.Reflection;

using Shared.Abstractions;

namespace Shared.Tests.Unit.Abstractions;

/// <summary>
///   Unit tests for the <see cref="Result" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResultTests
{

	[Fact]
	public void Ok_ShouldCreateSuccessResult()
	{
		// Arrange & Act
		Result result = Result.Ok();

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
		Result result = Result.Fail(errorMessage);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be(errorMessage);
	}

	[Fact]
	public void Failure_ShouldReturnOppositeOfSuccess()
	{
		// Arrange & Act
		Result successResult = Result.Ok();
		Result failureResult = Result.Fail("Error");

		// Assert
		successResult.Failure.Should().BeFalse();
		failureResult.Failure.Should().BeTrue();
	}

	[Fact]
	public void Fail_WithErrorCode_ShouldSetErrorCode()
	{
		// Arrange
		const string errorMessage = "Test error message";

		// Act
		Result result = Result.Fail(errorMessage, ResultErrorCode.Conflict);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Conflict);
		result.Error.Should().Be(errorMessage);
	}

	[Fact]
	public void Fail_WithErrorCodeAndDetails_ShouldSetDetailsAndErrorCode()
	{
		// Arrange
		const string errorMessage = "Test error message";
		var details = new { ServerVersion = "1.2.3" };

		// Act
		Result result = Result.Fail(errorMessage, ResultErrorCode.Concurrency, details);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Concurrency);
		result.Details.Should().BeSameAs(details);
		result.Error.Should().Be(errorMessage);
	}

}

/// <summary>
///   Unit tests for the <see cref="Result{T}" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ResultOfTTests
{

	[Fact]
	public void Ok_ShouldCreateSuccessResult_WithValue()
	{
		// Arrange
		const int expectedValue = 42;

		// Act
		Result<int> result = Result.Ok(expectedValue);

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
		Result<int> result = Result.Fail<int>(errorMessage);

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
		Result<string> result = Result.FromValue(value);

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
		Result<string> result = Result.FromValue(value);

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
		Result<int> result = Result.Ok(expectedValue);

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

	[Fact]
	public void ImplicitConversion_ToValue_WithNullResult_ShouldReturnDefault()
	{
		Result<int>? result = null;

		// Act
		int? value = result;

		// Assert - for value types the conversion returns the default value (0)
		value.Should().Be(0);
	}


	[Fact]
	public void Fail_WithErrorCode_ShouldSetErrorCode_OnGeneric()
	{
		// Arrange
		const string errorMessage = "Test error message";

		// Act
		Result<string> result = Result.Fail<string>(errorMessage, ResultErrorCode.NotFound);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
		result.Error.Should().Be(errorMessage);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void Fail_WithErrorCodeAndDetails_ShouldSetDetailsAndErrorCode_OnGeneric()
	{
		// Arrange
		const string errorMessage = "Test error message";
		var details = new { Attempt = 5 };

		// Act
		Result<string> result = Result.Fail<string>(errorMessage, ResultErrorCode.Conflict, details);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Conflict);
		result.Details.Should().BeSameAs(details);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitConversion_ToValue_ForReferenceTypeFailure_ShouldReturnNull()
	{
		// Arrange
		Result<string> result = Result.Fail<string>("err");

		// Act
		string? value = result;

		// Assert
		value.Should().BeNull();
	}

	[Fact]
	public void GenericStaticFail_OnGenericType_ShouldSetErrorCode()
	{
		// Arrange
		const string errorMessage = "Test error message";

		// Act
		Result<string> result = Result<string>.Fail(errorMessage, ResultErrorCode.NotFound);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.NotFound);
		result.Error.Should().Be(errorMessage);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void GenericStaticFail_OnGenericType_WithDetails_ShouldSetDetails()
	{
		// Arrange
		const string errorMessage = "Test error message";
		var details = new { Attempt = 7 };

		// Act
		Result<string> result = Result<string>.Fail(errorMessage, ResultErrorCode.Conflict, details);

		// Assert
		result.Success.Should().BeFalse();
		result.ErrorCode.Should().Be(ResultErrorCode.Conflict);
		result.Details.Should().BeSameAs(details);
		result.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitConversion_FromNullValue_ShouldCreateSuccessWithNullValue()
	{
		// Arrange
		string? value = null;

		// Act
		Result<string> result = value;

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().BeNull();
	}

}
