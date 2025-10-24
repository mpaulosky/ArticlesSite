// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryDtoValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using FluentAssertions;
using MongoDB.Bson;
using Shared.Models;
using Shared.Validators;

namespace Shared.Tests.Unit.Validators;

/// <summary>
///   Unit tests for the <see cref="CategoryDtoValidator" /> class.
/// </summary>
public class CategoryDtoValidatorTests
{
	private readonly CategoryDtoValidator _validator = new();

	[Fact]
	public void Validate_WithValidCategoryDto_ShouldPass()
	{
		// Arrange
		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Technology"
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithEmptyId_ShouldPass()
	{
		// Arrange - The validator checks for NotNull, but ObjectId is a struct and cannot be null
		// ObjectId.Empty is a valid value, just not useful
		var dto = CategoryDto.Empty;
		dto.CategoryName = "Technology";

		// Act
		var result = _validator.Validate(dto);

		// Assert - Should pass as ObjectId.Empty is not null
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidCategoryName_ShouldFail(string? invalidName)
	{
		// Arrange
		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = invalidName!
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage == "Name is required");
	}

	[Fact]
	public void Validate_WithCategoryNameTooLong_ShouldFail()
	{
		// Arrange
		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = new string('A', 81)
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
	}

	[Fact]
	public void Validate_WithCategoryNameAtMaxLength_ShouldPass()
	{
		// Arrange
		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = new string('A', 80)
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}
}
