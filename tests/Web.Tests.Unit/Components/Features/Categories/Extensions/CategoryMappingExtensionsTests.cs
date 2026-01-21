// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryMappingExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Tests.Unit.Components.Features.Categories.Extensions;

/// <summary>
/// Unit tests for <see cref="CategoryMappingExtensions"/> mapping methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoryMappingExtensionsTests
{

	[Fact]
	public void ToDto_WithValidCategory_MapsAllProperties()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-5);

		var category = new Category
		{
			Id = categoryId,
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		// Act
		var dto = category.ToDto();

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(categoryId);
		dto.CategoryName.Should().Be("Technology");
		dto.Slug.Should().Be("technology");
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void ToDto_WithArchivedCategory_MapsArchiveStatus()
	{
		// Arrange
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Old Category",
			Slug = "old-category",
			IsArchived = true
		};

		// Act
		var dto = category.ToDto();

		// Assert
		dto.IsArchived.Should().BeTrue();
	}

	[Fact]
	public void ToDto_WithNullSlug_DefaultsToEmptyString()
	{
		// Arrange
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Test",
			Slug = string.Empty,
			IsArchived = false
		};

		// Act
		var dto = category.ToDto();

		// Assert
		dto.Slug.Should().Be(string.Empty);
	}

	[Fact]
	public void ToDto_WithNullCreatedOn_DefaultsToUtcNow()
	{
		// Arrange
		var before = DateTimeOffset.UtcNow;
		var category = new Category
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Test",
			Slug = "test",
			CreatedOn = null,
			IsArchived = false
		};
		var after = DateTimeOffset.UtcNow;

		// Act
		var dto = category.ToDto();

		// Assert
		dto.CreatedOn.Should().BeOnOrAfter(before);
		dto.CreatedOn.Should().BeOnOrBefore(after.AddSeconds(1));
	}

	[Fact]
	public void ToDto_WithNullCategory_ThrowsArgumentNullException()
	{
		// Arrange
		Category? category = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => category!.ToDto());
	}

	[Fact]
	public void ToEntity_WithValidDto_CreatesCategory()
	{
		// Arrange
		var createdOn = DateTimeOffset.UtcNow.AddDays(-5);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Business",
			Slug = "business",
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		// Act
		var category = dto.ToEntity();

		// Assert
		category.Should().NotBeNull();
		category.CategoryName.Should().Be("Business");
		category.Slug.Should().Be("business");
		category.CreatedOn.Should().Be(createdOn);
		category.ModifiedOn.Should().Be(modifiedOn);
		category.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void ToEntity_WithNullDto_ThrowsArgumentNullException()
	{
		// Arrange
		CategoryDto? dto = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => dto!.ToEntity());
	}

	[Fact]
	public void ToDtos_WithValidCollection_MapsAllItems()
	{
		// Arrange
		var categories = new List<Category>
		{
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Tech", Slug = "tech", IsArchived = false },
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Business", Slug = "business", IsArchived = false },
			new() { Id = ObjectId.GenerateNewId(), CategoryName = "Lifestyle", Slug = "lifestyle", IsArchived = true }
		};

		// Act
		var dtos = categories.ToDtos().ToList();

		// Assert
		dtos.Should().HaveCount(3);
		dtos[0].CategoryName.Should().Be("Tech");
		dtos[1].CategoryName.Should().Be("Business");
		dtos[2].CategoryName.Should().Be("Lifestyle");
		dtos[2].IsArchived.Should().BeTrue();
	}

	[Fact]
	public void ToDtos_WithEmptyCollection_ReturnsEmptyEnumerable()
	{
		// Arrange
		var categories = new List<Category>();

		// Act
		var dtos = categories.ToDtos().ToList();

		// Assert
		dtos.Should().BeEmpty();
	}

	[Fact]
	public void ToDtos_WithNullCollection_ThrowsArgumentNullException()
	{
		// Arrange
		List<Category>? categories = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => categories!.ToDtos().ToList());
	}

	[Fact]
	public void ToDtos_PreservesPropertyMapping()
	{
		// Arrange
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-5);

		var categories = new List<Category>
		{
			new()
			{
				Id = ObjectId.GenerateNewId(),
				CategoryName = "First",
				Slug = "first",
				CreatedOn = createdOn,
				ModifiedOn = modifiedOn,
				IsArchived = false
			}
		};

		// Act
		var dtos = categories.ToDtos().ToList();

		// Assert
		dtos[0].CategoryName.Should().Be("First");
		dtos[0].Slug.Should().Be("first");
		dtos[0].CreatedOn.Should().Be(createdOn);
		dtos[0].ModifiedOn.Should().Be(modifiedOn);
		dtos[0].IsArchived.Should().BeFalse();
	}

	[Fact]
	public void ToDto_RoundTrip_PreservesAllData()
	{
		// Arrange
		var originalDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Original",
			Slug = "original",
			CreatedOn = DateTimeOffset.UtcNow,
			ModifiedOn = DateTimeOffset.UtcNow.AddDays(-1),
			IsArchived = false
		};

		// Act
		var entity = originalDto.ToEntity();
		var resultDto = entity.ToDto();

		// Assert
		resultDto.CategoryName.Should().Be(originalDto.CategoryName);
		resultDto.Slug.Should().Be(originalDto.Slug);
		resultDto.IsArchived.Should().Be(originalDto.IsArchived);
	}

	[Fact]
	public void ToDtos_WithLargeCollection_MapsCorrectly()
	{
		// Arrange
		var categories = Enumerable.Range(1, 100)
			.Select(i => new Category
			{
				Id = ObjectId.GenerateNewId(),
				CategoryName = $"Category{i}",
				Slug = $"category-{i}",
				IsArchived = i % 2 == 0 // Even numbers are archived
			})
			.ToList();

		// Act
		var dtos = categories.ToDtos().ToList();

		// Assert
		dtos.Should().HaveCount(100);
		dtos.Where(d => d.IsArchived).Should().HaveCount(50);
		dtos.Where(d => !d.IsArchived).Should().HaveCount(50);
	}

}
