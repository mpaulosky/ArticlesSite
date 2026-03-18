//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     ArticleDtoValidatorTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

namespace Web.Components.Features.Articles.Validators;

/// <summary>
///   Unit tests for the <see cref="ArticleDtoValidator" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArticleDtoValidatorTests
{

	private readonly ArticleDtoValidator _validator = new();

	[Fact]
	public void Validate_WithValidArticleDto_ShouldPass()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Slug = "test_article",
			Title = "Test Article",
			Introduction = "Test introduction",
			Content = "Test content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Technology" },
			IsPublished = true,
			PublishedOn = DateTimeOffset.UtcNow
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void Validate_WithNullId_ShouldPass()
	{
		// Arrange - The validator checks for NotNull, but ObjectId.Empty is not null
		// ObjectId is a struct and cannot be null
		ArticleDto dto = new() { Id = ObjectId.Empty };

		// Act
		ValidationResult? result = _validator.Validate(dto);

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
		ArticleDto dto = new()
		{
			Title = invalidTitle!,
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage == "Title is required");
	}

	[Fact]
	public void Validate_WithTitleTooLong_ShouldFail()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = new string('A', 101),
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

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
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = invalidIntro!,
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should()
				.Contain(e => e.PropertyName == "Introduction" && e.ErrorMessage == "Introduction is required");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidContent_ShouldFail(string? invalidContent)
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = invalidContent!,
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Content" && e.ErrorMessage == "Content is required");
	}

	[Fact]
	public void Validate_WithContentTooLong_ShouldFail()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = new string('A', 10001),
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

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
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = invalidCoverImageUrl!,
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should()
				.Contain(e => e.PropertyName == "CoverImageUrl" && e.ErrorMessage == "Cover image is required");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void Validate_WithInvalidSlug_ShouldFail(string? invalidSlug)
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = invalidSlug!,
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

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
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = invalidSlug,
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" }
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should().Contain(e =>
				e.PropertyName == "Slug" &&
				e.ErrorMessage == "URL slug can only contain lowercase letters, numbers, and underscores");
	}

	[Fact]
	public void Validate_WithNullAuthor_ShouldFail()
	{
		// Arrange
		ArticleDto dto = new()
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
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Author" && e.ErrorMessage == "Author is required");
	}

	[Fact]
	public void Validate_WithNullCategory_ShouldFail()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = null
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == "Category" && e.ErrorMessage == "Category is required");
	}

	[Fact]
	public void Validate_WithPublishedButNoPublishedOn_ShouldFail()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = true,
			PublishedOn = null
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeFalse();

		result.Errors.Should().Contain(e =>
				e.PropertyName == "PublishedOn" && e.ErrorMessage == "PublishedOn is required when IsPublished is true");
	}

	[Fact]
	public void Validate_WithNotPublishedAndNoPublishedOn_ShouldPass()
	{
		// Arrange
		ArticleDto dto = new()
		{
			Title = "Valid Title",
			Introduction = "Valid intro",
			Content = "Valid content",
			CoverImageUrl = "https://example.com/cover.jpg",
			Slug = "valid_slug",
			Author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("auth0|123", "John Doe"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = false,
			PublishedOn = null
		};

		// Act
		ValidationResult? result = _validator.Validate(dto);

		// Assert
		result.IsValid.Should().BeTrue();
		result.Errors.Should().BeEmpty();
	}

}
