// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleDtoValidatorTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using FluentAssertions;
using MongoDB.Bson;
using Shared.Entities;
using Shared.Models;
using Shared.Validators;

namespace Shared.Tests.Unit.Validators;

/// <summary>
///   Unit tests for the <see cref="ArticleDtoValidator" /> class.
/// </summary>
public class ArticleDtoValidatorTests
{
	private readonly ArticleDtoValidator _validator = new();

	[Fact]
	public void Validate_WithValidArticleDto_ShouldPass()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Slug = "test_article",
			Title = "Test Article",
			Introduction = "Test introduction",
			Content = "Test content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Technology" },
			IsPublished = true,
			PublishedOn = DateTimeOffset.UtcNow
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithNullId_ShouldPass()
	{
		// Arrange - The validator checks for NotNull, but ObjectId.Empty is not null
		// ObjectId is a struct and cannot be null
		var dto = new ArticleDto
		{
			Id = ObjectId.Empty
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert - Should fail on other required fields but not on Id
		result.IsValid.Should().BeFalse();
		result.Errors.Should().NotContain(e => e.PropertyName == "Id");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidTitle_ShouldFail(string? invalidTitle)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = invalidTitle!,
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title is required");
	}

	[Fact]
	public void Validate_WithTitleTooLong_ShouldFail()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = new string('A', 101),
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Title");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidIntroduction_ShouldFail(string? invalidIntro)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = invalidIntro!,
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Introduction" && e.ErrorMessage == "Introduction is required");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidContent_ShouldFail(string? invalidContent)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = invalidContent!,
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Content is required");
	}

	[Fact]
	public void Validate_WithContentTooLong_ShouldFail()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = new string('A', 4001),
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Content cannot exceed 4000 characters");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidCoverImageUrl_ShouldFail(string? invalidCoverImageUrl)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = invalidCoverImageUrl!,
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "CoverImageUrl" && e.ErrorMessage == "Cover image is required");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidSlug_ShouldFail(string? invalidSlug)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = invalidSlug!,
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage == "URL slug is required");
	}

	[Theory]
	[InlineData("Invalid Slug")]
	[InlineData("invalid-slug")]
	[InlineData("INVALID_SLUG")]
	public void Validate_WithInvalidSlugFormat_ShouldFail(string invalidSlug)
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = invalidSlug,
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Slug" && e.ErrorMessage == "URL slug can only contain lowercase letters, numbers, and underscores");
	}

	[Fact]
	public void Validate_WithNullAuthor_ShouldFail()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = null,
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Author" && e.ErrorMessage == "Author is required");
	}

	[Fact]
	public void Validate_WithNullCategory_ShouldFail()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = null
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Category" && e.ErrorMessage == "Category is required");
	}

	[Fact]
	public void Validate_WithPublishedButNoPublishedOn_ShouldFail()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = true,
			PublishedOn = null
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "PublishedOn" && e.ErrorMessage == "PublishedOn is required when IsPublished is true");
	}

	[Fact]
	public void Validate_WithNotPublishedAndNoPublishedOn_ShouldPass()
	{
		// Arrange
		var dto = new ArticleDto
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = false,
			PublishedOn = null
		};

		// Act
		var result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}
}
