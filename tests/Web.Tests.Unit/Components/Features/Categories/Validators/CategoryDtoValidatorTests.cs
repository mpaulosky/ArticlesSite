//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     CategoryDtoValidatorTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

namespace Web.Tests.Unit.Components.Features.Categories.Validators;

/// <summary>
///   Unit tests for the <see cref="CategoryDtoValidator" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryDtoValidatorTests
{

	private readonly CategoryDtoValidator _validator = new();

	[Fact]
	public void Validate_WithValidCategoryDto_ShouldPass()
	{
		// Arrange
		CategoryDto dto = new() { Id = ObjectId.GenerateNewId(), CategoryName = "Technology" };

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithEmptyId_ShouldPass()
	{
		// Arrange - The validator checks for NotNull, but ObjectId is a struct and cannot be null
		// ObjectId.Empty is a valid value, just not useful
		CategoryDto dto = new() { CategoryName = "Technology" };

		// Act
		ValidationResult? result = _validator.Validate(dto);

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
		CategoryDto dto = new() { Id = ObjectId.GenerateNewId(), CategoryName = invalidName! };

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage == "Category name is required");
	}

	[Fact]
	public void Validate_WithCategoryNameTooLong_ShouldFail()
	{
		// Arrange
		CategoryDto dto = new() { Id = ObjectId.GenerateNewId(), CategoryName = new string('A', 81) };

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
	}

	[Fact]
	public void Validate_WithCategoryNameAtMaxLength_ShouldPass()
	{
		// Arrange
		CategoryDto dto = new() { Id = ObjectId.GenerateNewId(), CategoryName = new string('A', 80) };

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

}
