//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     CategoryTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Entities;

/// <summary>
///   Unit tests for the <see cref="Category" /> entity.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryTests
{

	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		Category category = new();

		// Assert
		category.Id.Should().NotBe(ObjectId.Empty);
		category.CategoryName.Should().Be(string.Empty);
		category.Slug.Should().Be(string.Empty);
		category.CreatedOn.Should().BeNull();
		category.ModifiedOn.Should().BeNull();
		category.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		Category empty = Category.Empty;

		// Assert
		empty.Id.Should().Be(ObjectId.Empty);
		empty.CategoryName.Should().Be(string.Empty);
		empty.Slug.Should().Be(string.Empty);
		empty.CreatedOn.Should().BeNull();
		empty.ModifiedOn.Should().BeNull();
		empty.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Update_WithValidData_ShouldUpdatePropertiesAndSetModifiedOn()
	{
		// Arrange
		Category category = new() { CategoryName = "Old Name", Slug = "old_slug", IsArchived = false };
		DateTimeOffset beforeModified = DateTimeOffset.UtcNow;

		// Act
		category.Update("New Name", "new_slug", true);

		// Assert
		category.CategoryName.Should().Be("New Name");
		category.Slug.Should().Be("new_slug");
		category.IsArchived.Should().BeTrue();
		category.ModifiedOn.Should().NotBeNull();
		category.ModifiedOn.Should().BeOnOrAfter(beforeModified);
	}

	[Fact]
	public void Update_WithNullCategoryName_ShouldThrowArgumentException()
	{
		// Arrange
		Category category = new();

		// Act
		Action act = () => category.Update(null!, "slug", false);

		// Assert
		act.Should().Throw<ArgumentException>()
				.WithMessage("Category name cannot be null or whitespace.*")
				.And.ParamName.Should().Be("categoryName");
	}

	[Fact]
	public void Update_WithEmptyCategoryName_ShouldThrowArgumentException()
	{
		// Arrange
		Category category = new();

		// Act
		Action act = () => category.Update("  ", "slug", false);

		// Assert
		act.Should().Throw<ArgumentException>()
				.WithMessage("Category name cannot be null or whitespace.*")
				.And.ParamName.Should().Be("categoryName");
	}

	[Fact]
	public void Update_WithNullSlug_ShouldThrowArgumentException()
	{
		// Arrange
		Category category = new();

		// Act
		Action act = () => category.Update("Category Name", null!, false);

		// Assert
		act.Should().Throw<ArgumentException>()
				.WithMessage("Slug cannot be null or whitespace.*")
				.And.ParamName.Should().Be("slug");
	}

	[Fact]
	public void Update_WithEmptySlug_ShouldThrowArgumentException()
	{
		// Arrange
		Category category = new();

		// Act
		Action act = () => category.Update("Category Name", "  ", false);

		// Assert
		act.Should().Throw<ArgumentException>()
				.WithMessage("Slug cannot be null or whitespace.*")
				.And.ParamName.Should().Be("slug");
	}

	[Fact]
	public void Properties_ShouldBeSettable()
	{
		// Arrange
		Category category = new();
		DateTimeOffset createdOn = DateTimeOffset.UtcNow;
		DateTimeOffset modifiedOn = DateTimeOffset.UtcNow.AddDays(1);

		// Act
		category.CategoryName = "Test Category";
		category.Slug = "test_category";
		category.CreatedOn = createdOn;
		category.ModifiedOn = modifiedOn;
		category.IsArchived = true;

		// Assert
		category.CategoryName.Should().Be("Test Category");
		category.Slug.Should().Be("test_category");
		category.CreatedOn.Should().Be(createdOn);
		category.ModifiedOn.Should().Be(modifiedOn);
		category.IsArchived.Should().BeTrue();
	}

}
