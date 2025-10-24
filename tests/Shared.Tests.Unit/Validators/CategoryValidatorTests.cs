// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using FluentAssertions;
using MongoDB.Bson;
using Shared.Entities;
using Shared.Validators;

namespace Shared.Tests.Unit.Validators;

/// <summary>
///   Unit tests for the <see cref="CategoryValidator" /> class.
/// </summary>
public class CategoryValidatorTests
{
	private readonly CategoryValidator _validator = new();

	[Fact]
	public void Validate_WithValidCategory_ShouldPass()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow.AddSeconds(-1) // Set it slightly in the past to avoid timing issues
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithEmptyId_ShouldFail()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow
		};
		// Manually set Id to Empty since the constructor generates a new one
		var emptyCat = Category.Empty;
		emptyCat.CategoryName = "Technology";
		emptyCat.CreatedOn = DateTimeOffset.UtcNow;

		// Act
		var result = _validator.Validate(emptyCat);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidCategoryName_ShouldFail(string? invalidName)
	{
		// Arrange
		var category = new Category
		{
			CategoryName = invalidName!,
			CreatedOn = DateTimeOffset.UtcNow
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage == "Name is required");
	}

	[Fact]
	public void Validate_WithCategoryNameTooLong_ShouldFail()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = new string('A', 81),
			CreatedOn = DateTimeOffset.UtcNow
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CategoryName" && e.ErrorMessage == "Name cannot exceed 80 characters");
	}

	[Fact]
	public void Validate_WithNullCreatedOn_ShouldFail()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = null
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CreatedOn" && e.ErrorMessage == "CreatedOn is required");
	}

	[Fact]
	public void Validate_WithFutureCreatedOn_ShouldFail()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(1)
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CreatedOn" && e.ErrorMessage == "CreatedOn cannot be in the future");
	}

	[Fact]
	public void Validate_WithCurrentCreatedOn_ShouldPass()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow.AddSeconds(-1) // Set it slightly in the past to avoid timing issues
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithPastCreatedOn_ShouldPass()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-1)
		};

		// Act
		var result = _validator.Validate(category);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}
}
