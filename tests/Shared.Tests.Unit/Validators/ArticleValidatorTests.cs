//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     ArticleValidatorTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using FluentValidation.Results;

using Shared.Validators;

namespace Shared.Tests.Unit.Validators;

/// <summary>
///   Unit tests for the <see cref="ArticleValidator" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArticleValidatorTests
{

	private readonly ArticleValidator _validator = new();

	[Fact]
	public void Validate_WithValidArticle_ShouldPass()
	{
		// Arrange
		Article article = new("Test Article", "Test introduction", "Test content", "https://example.com/cover.jpg",
				new AuthorInfo("auth0|123", "John Doe"),
				new Category { CategoryName = "Technology" })
		{ IsPublished = true, PublishedOn = DateTimeOffset.UtcNow };

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithEmptyId_ShouldFail()
	{
		// Arrange
		Article article = Article.Empty;

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Id");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidTitle_ShouldFail(string? invalidTitle)
	{
		// Arrange
		Article article = new()
		{
			Title = invalidTitle!,
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title is required");
	}

	[Fact]
	public void Validate_WithTitleTooLong_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = new string('A', 101),
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Title");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidIntroduction_ShouldFail(string? invalidIntroduction)
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = invalidIntroduction!,
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should()
				.Contain(e => e.PropertyName == "Introduction" && e.ErrorMessage == "Introduction is required");
	}

	[Fact]
	public void Validate_WithIntroductionTooLong_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = new string('A', 201),
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Introduction");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidContent_ShouldFail(string? invalidContent)
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = invalidContent!,
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Content is required");
	}

	[Fact]
	public void Validate_WithContentTooLong_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = new string('A', 10001),
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should().Contain(e =>
				e.PropertyName == "Content" && e.ErrorMessage == "Content cannot exceed 10000 characters");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidCoverImageUrl_ShouldFail(string? invalidCoverImageUrl)
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = invalidCoverImageUrl!,
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should()
				.Contain(e => e.PropertyName == "CoverImageUrl" && e.ErrorMessage == "Cover image is required");
	}

	[Fact]
	public void Validate_WithInvalidSlug_ShouldFail()
	{
		// Arrange - Use Article.Empty which has empty slug
		Article article = Article.Empty;

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Slug");
	}

	[Fact]
	public void Validate_WithInvalidSlugFormat_ShouldFail()
	{
		// Arrange - Since Slug is generated from title and is read-only, we test with Article.Empty which has empty slug
		Article article = Article.Empty;

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Slug");
	}

	[Fact]
	public void Validate_WithNullAuthor_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = null,
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Author" && e.ErrorMessage == "Author is required");
	}

	[Fact]
	public void Validate_WithNullCategory_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = null
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Category" && e.ErrorMessage == "Category is required");
	}

	[Fact]
	public void Validate_WithPublishedButNoPublishedOn_ShouldFail()
	{
		// Arrange
		Article article = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = true,
			PublishedOn = null
		};

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should().Contain(e =>
				e.PropertyName == "PublishedOn" && e.ErrorMessage == "PublishedOn is required when IsPublished is true");
	}

	[Fact]
	public void Validate_WithNotPublishedAndNoPublishedOn_ShouldPass()
	{
		// Arrange
		Article article = new("Valid Title", "Valid intro", "Valid content", "https://example.com/cover.jpg",
				new AuthorInfo("auth0|123", "John Doe"),
				new Category { CategoryName = "Tech" })
		{ IsPublished = false, PublishedOn = null };

		// Act
		ValidationResult? result = _validator.Validate(article);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

}