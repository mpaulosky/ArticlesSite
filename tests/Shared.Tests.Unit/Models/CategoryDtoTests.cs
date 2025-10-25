// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryDtoTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

namespace Shared.Tests.Unit.Models;

/// <summary>
///   Unit tests for the <see cref="CategoryDto" /> class.
/// </summary>
public class CategoryDtoTests
{
	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		var dto = new CategoryDto();

		// Assert
		dto.Id.Should().Be(ObjectId.Empty);
		dto.CategoryName.Should().Be(string.Empty);
		dto.CreatedOn.Should().NotBe(default);
		dto.ModifiedOn.Should().BeNull();
		dto.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		var empty = CategoryDto.Empty;

		// Assert
		empty.Id.Should().Be(ObjectId.Empty);
		empty.CategoryName.Should().Be(string.Empty);
		empty.CreatedOn.Should().NotBe(default);
		empty.ModifiedOn.Should().BeNull();
		empty.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Properties_ShouldBeSettable()
	{
		// Arrange
		var dto = new CategoryDto();
		var id = ObjectId.GenerateNewId();
		var createdOn = DateTimeOffset.UtcNow;
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(1);

		// Act
		dto.Id = id;
		dto.CategoryName = "Technology";
		dto.CreatedOn = createdOn;
		dto.ModifiedOn = modifiedOn;
		dto.IsArchived = true;

		// Assert
		dto.Id.Should().Be(id);
		dto.CategoryName.Should().Be("Technology");
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeTrue();
	}

	[Fact]
	public void CategoryDto_ShouldSupportObjectInitializer()
	{
		// Arrange & Act
		var dto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Programming",
			CreatedOn = DateTimeOffset.UtcNow,
			ModifiedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Assert
		dto.CategoryName.Should().Be("Programming");
		dto.IsArchived.Should().BeFalse();
	}
}
