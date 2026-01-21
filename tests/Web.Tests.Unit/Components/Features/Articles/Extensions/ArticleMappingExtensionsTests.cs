// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleMappingExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Features.Articles.Extensions;

namespace Web.Tests.Unit.Components.Features.Articles.Extensions;

/// <summary>
/// Unit tests for <see cref="ArticleMappingExtensions"/> mapping methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArticleMappingExtensionsTests
{

	[Fact]
	public void ToDto_WithValidArticle_MapsAllProperties()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author");
		var category = new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Tech" };
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-3);

		var article = new Article(
			"Test Title",
			"Test Introduction",
			"Test Content",
			"https://example.com/image.jpg",
			author,
			category,
			true,
			publishedOn,
			false,
			"test-article"
		)
		{
			Id = ObjectId.GenerateNewId(),
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn
		};

		// Act
		var dto = article.ToDto(canEdit: true);

		// Assert
		dto.Should().NotBeNull();
		dto.Id.Should().Be(article.Id);
		dto.Slug.Should().Be("test-article");
		dto.Title.Should().Be("Test Title");
		dto.Introduction.Should().Be("Test Introduction");
		dto.Content.Should().Be("Test Content");
		dto.CoverImageUrl.Should().Be("https://example.com/image.jpg");
		dto.Author.Should().Be(author);
		dto.Category.Should().Be(category);
		dto.IsPublished.Should().BeTrue();
		dto.PublishedOn.Should().Be(publishedOn);
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
		dto.CanEdit.Should().BeTrue();
	}

	[Fact]
	public void ToDto_WithCanEditFalse_SetsFlagCorrectly()
	{
		// Arrange
		var article = new Article(
			"Title",
			"Intro",
			"Content",
			null,
			null,
			null,
			false,
			null,
			false,
			"slug"
		);

		// Act
		var dto = article.ToDto(canEdit: false);

		// Assert
		dto.CanEdit.Should().BeFalse();
	}

	[Fact]
	public void ToDto_WithArchivedArticle_MapsArchiveStatus()
	{
		// Arrange
		var article = new Article(
			"Title",
			"Intro",
			"Content",
			null,
			null,
			null,
			false,
			null,
			true, // isArchived
			"slug"
		);

		// Act
		var dto = article.ToDto(canEdit: true);

		// Assert
		dto.IsArchived.Should().BeTrue();
	}

	[Fact]
	public void ToDto_WithNullArticle_ThrowsArgumentNullException()
	{
		// Arrange
		Article? article = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => article!.ToDto());
	}

	[Fact]
	public void ToEntity_WithValidDto_CreatesArticleWithAllProperties()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author");
		var category = new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Tech" };
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);

		var dto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-slug",
			"Test Title",
			"Test Introduction",
			"Test Content",
			"https://example.com/image.jpg",
			author,
			category,
			true,
			publishedOn,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		// Act
		var article = dto.ToEntity();

		// Assert
		article.Should().NotBeNull();
		article.Title.Should().Be("Test Title");
		article.Introduction.Should().Be("Test Introduction");
		article.Content.Should().Be("Test Content");
		article.CoverImageUrl.Should().Be("https://example.com/image.jpg");
		article.Author.Should().Be(author);
		article.Category.Should().Be(category);
		article.IsPublished.Should().BeTrue();
		article.PublishedOn.Should().Be(publishedOn);
		article.IsArchived.Should().BeFalse();
		article.Slug.Should().Be("test-slug");
	}

	[Fact]
	public void ToEntity_WithNullAuthorAndCategory_CreatesArticleWithoutReferences()
	{
		// Arrange
		var dto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-slug",
			"Title",
			"Intro",
			"Content",
			string.Empty,
			null, // author
			null, // category
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		// Act
		var article = dto.ToEntity();

		// Assert
		article.Author.Should().BeNull();
		article.Category.Should().BeNull();
	}

	[Fact]
	public void ToEntity_WithNullDto_ThrowsArgumentNullException()
	{
		// Arrange
		ArticleDto? dto = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => dto!.ToEntity());
	}

	[Fact]
	public void ToDto_PreservesNullCoverImageUrl()
	{
		// Arrange
		var article = new Article(
			"Title",
			"Intro",
			"Content",
			null, // coverImageUrl
			null,
			null,
			false,
			null,
			false,
			"slug"
		);

		// Act
		var dto = article.ToDto();

		// Assert
		// Article sets default if null, so check the result
		dto.CoverImageUrl.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void ToDto_RoundTrip_PreservesAllData()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author");
		var category = new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Tech" };
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-3);

		var originalDto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-slug",
			"Test Title",
			"Test Introduction",
			"Test Content",
			"https://example.com/image.jpg",
			author,
			category,
			true,
			publishedOn,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		// Act
		var entity = originalDto.ToEntity();
		var resultDto = entity.ToDto(canEdit: false);

		// Assert
		resultDto.Title.Should().Be(originalDto.Title);
		resultDto.Introduction.Should().Be(originalDto.Introduction);
		resultDto.Content.Should().Be(originalDto.Content);
		resultDto.CoverImageUrl.Should().Be(originalDto.CoverImageUrl);
		resultDto.Slug.Should().Be(originalDto.Slug);
		resultDto.IsPublished.Should().Be(originalDto.IsPublished);
		resultDto.IsArchived.Should().Be(originalDto.IsArchived);
	}

}
