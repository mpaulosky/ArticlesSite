//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     ArticleDtoTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Models;

/// <summary>
///   Unit tests for the <see cref="ArticleDto" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArticleDtoTests
{

	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		ArticleDto dto = new();

		// Assert
		dto.Id.Should().Be(ObjectId.Empty);
		dto.Slug.Should().Be(string.Empty);
		dto.Title.Should().Be(string.Empty);
		dto.Introduction.Should().Be(string.Empty);
		dto.Content.Should().Be(string.Empty);
		dto.CoverImageUrl.Should().Be(string.Empty);
		dto.Author.Should().BeNull();
		dto.Category.Should().BeNull();
		dto.IsPublished.Should().BeFalse();
		dto.PublishedOn.Should().BeNull();
		dto.CreatedOn.Should().BeNull();
		dto.ModifiedOn.Should().BeNull();
		dto.IsArchived.Should().BeFalse();
		dto.CanEdit.Should().BeFalse();
	}

	[Fact]
	public void Constructor_WithParameters_ShouldSetAllProperties()
	{
		// Arrange
		ObjectId id = ObjectId.GenerateNewId();
		const string slug = "test_article";
		const string title = "Test Article";
		const string intro = "Test introduction";
		const string content = "Test content";
		const string coverImageUrl = "https://example.com/cover.jpg";
		AuthorInfo author = new("auth0|123", "John Doe");
		Category category = new() { CategoryName = "Technology" };
		const bool isPublished = true;
		DateTimeOffset publishedOn = DateTimeOffset.UtcNow;
		DateTimeOffset createdOn = DateTimeOffset.UtcNow.AddDays(-1);
		DateTimeOffset modifiedOn = DateTimeOffset.UtcNow;
		const bool isArchived = false;
		const bool canEdit = true;

		// Act
		ArticleDto dto = new(id, slug, title, intro, content, coverImageUrl, author, category,
				isPublished, publishedOn, createdOn, modifiedOn, isArchived, canEdit);

		// Assert
		dto.Id.Should().Be(id);
		dto.Slug.Should().Be(slug);
		dto.Title.Should().Be(title);
		dto.Introduction.Should().Be(intro);
		dto.Content.Should().Be(content);
		dto.CoverImageUrl.Should().Be(coverImageUrl);
		dto.Author.Should().Be(author);
		dto.Category.Should().Be(category);
		dto.IsPublished.Should().Be(isPublished);
		dto.PublishedOn.Should().Be(publishedOn);
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().Be(isArchived);
		dto.CanEdit.Should().Be(canEdit);
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		ArticleDto empty = ArticleDto.Empty;

		// Assert
		empty.Id.Should().Be(ObjectId.Empty);
		empty.Slug.Should().Be(string.Empty);
		empty.Title.Should().Be(string.Empty);
		empty.Introduction.Should().Be(string.Empty);
		empty.Content.Should().Be(string.Empty);
		empty.CoverImageUrl.Should().Be(string.Empty);
		empty.Author.Should().BeNull();
		empty.Category.Should().BeNull();
		empty.IsPublished.Should().BeFalse();
		empty.PublishedOn.Should().BeNull();
		empty.CreatedOn.Should().BeNull();
		empty.ModifiedOn.Should().BeNull();
		empty.IsArchived.Should().BeFalse();
		empty.CanEdit.Should().BeFalse();
	}

	[Fact]
	public void UrlSlug_ShouldGetAndSetSlug()
	{
		// Arrange
		ArticleDto dto = new();
		const string slug = "test_url_slug";

		// Act
		dto.UrlSlug = slug;

		// Assert
		dto.UrlSlug.Should().Be(slug);
		dto.Slug.Should().Be(slug);
	}

	[Fact]
	public void UrlSlug_Get_ShouldReturnSlugValue()
	{
		// Arrange
		ArticleDto dto = new() { Slug = "test_slug" };

		// Act
		string urlSlug = dto.UrlSlug;

		// Assert
		urlSlug.Should().Be("test_slug");
	}

	[Fact]
	public void Properties_ShouldBeSettable()
	{
		// Arrange
		ArticleDto dto = new();
		AuthorInfo author = new("auth0|123", "John Doe");
		Category category = new() { CategoryName = "Tech" };

		// Act
		dto.Slug = "new_slug";
		dto.Title = "New Title";
		dto.Introduction = "New Intro";
		dto.Content = "New Content";
		dto.CoverImageUrl = "new.jpg";
		dto.Author = author;
		dto.Category = category;
		dto.IsPublished = true;
		dto.PublishedOn = DateTimeOffset.UtcNow;
		dto.CreatedOn = DateTimeOffset.UtcNow;
		dto.ModifiedOn = DateTimeOffset.UtcNow;
		dto.IsArchived = true;
		dto.CanEdit = true;

		// Assert
		dto.Slug.Should().Be("new_slug");
		dto.Title.Should().Be("New Title");
		dto.Introduction.Should().Be("New Intro");
		dto.Content.Should().Be("New Content");
		dto.CoverImageUrl.Should().Be("new.jpg");
		dto.Author.Should().Be(author);
		dto.Category.Should().Be(category);
		dto.IsPublished.Should().BeTrue();
		dto.PublishedOn.Should().NotBeNull();
		dto.CreatedOn.Should().NotBeNull();
		dto.ModifiedOn.Should().NotBeNull();
		dto.IsArchived.Should().BeTrue();
		dto.CanEdit.Should().BeTrue();
	}

}