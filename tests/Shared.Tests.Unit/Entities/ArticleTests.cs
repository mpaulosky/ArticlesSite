// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared.Tests.Unit
// =======================================================

using FluentAssertions;
using MongoDB.Bson;
using Shared.Entities;

namespace Shared.Tests.Unit.Entities;

/// <summary>
///   Unit tests for the <see cref="Article" /> entity.
/// </summary>
public class ArticleTests
{
	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		var article = new Article();

		// Assert
		article.Id.Should().NotBe(ObjectId.Empty);
		article.Title.Should().Be(string.Empty);
		article.Introduction.Should().Be(string.Empty);
		article.Content.Should().Be(string.Empty);
		article.CoverImageUrl.Should().Be(string.Empty);
		article.Slug.Should().Be(string.Empty);
		article.CreatedOn.Should().NotBeNull();
		article.Author.Should().BeNull();
		article.Category.Should().BeNull();
		article.IsPublished.Should().BeFalse();
		article.PublishedOn.Should().BeNull();
		article.IsArchived.Should().BeFalse();
		article.ModifiedOn.Should().BeNull();
	}

	[Fact]
	public void Constructor_WithRequiredParameters_ShouldSetPropertiesAndGenerateSlug()
	{
		// Arrange
		const string title = "Test Article";
		const string intro = "Test introduction";
		const string content = "Test content";
		const string coverImage = "https://example.com/image.jpg";
		var author = new AuthorInfo("auth0|123", "John Doe");
		var category = new Category { CategoryName = "Technology" };

		// Act
		var article = new Article(title, intro, content, coverImage, author, category);

		// Assert
		article.Title.Should().Be(title);
		article.Introduction.Should().Be(intro);
		article.Content.Should().Be(content);
		article.CoverImageUrl.Should().Be(coverImage);
		article.Slug.Should().Be("test_article");
		article.Author.Should().Be(author);
		article.Category.Should().Be(category);
		article.IsPublished.Should().BeFalse();
		article.PublishedOn.Should().BeNull();
		article.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Constructor_WithNullCoverImageUrl_ShouldUseDefaultImage()
	{
		// Arrange & Act
		var article = new Article("Title", "Intro", "Content", null, null, null);

		// Assert
		article.CoverImageUrl.Should().Be("https://example.com/image.jpg");
	}

	[Fact]
	public void Constructor_WithAllParameters_ShouldSetAllProperties()
	{
		// Arrange
		const string title = "Test Article";
		const string intro = "Test introduction";
		const string content = "Test content";
		const string coverImage = "https://example.com/cover.jpg";
		var author = new AuthorInfo("auth0|123", "John Doe");
		var category = new Category { CategoryName = "Technology" };
		var publishedOn = DateTimeOffset.UtcNow;

		// Act
		var article = new Article(title, intro, content, coverImage, author, category, true, publishedOn, true);

		// Assert
		article.Title.Should().Be(title);
		article.IsPublished.Should().BeTrue();
		article.PublishedOn.Should().Be(publishedOn);
		article.IsArchived.Should().BeTrue();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	public void Constructor_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
	{
		// Arrange & Act
		var act = () => new Article(invalidTitle!, "Intro", "Content", "cover.jpg", null, null);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("Title cannot be null or whitespace.*")
			.And.ParamName.Should().Be("title");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	public void Constructor_WithInvalidIntroduction_ShouldThrowArgumentException(string? invalidIntro)
	{
		// Arrange & Act
		var act = () => new Article("Title", invalidIntro!, "Content", "cover.jpg", null, null);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("Introduction cannot be null or whitespace.*")
			.And.ParamName.Should().Be("introduction");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	public void Constructor_WithInvalidContent_ShouldThrowArgumentException(string? invalidContent)
	{
		// Arrange & Act
		var act = () => new Article("Title", "Intro", invalidContent!, "cover.jpg", null, null);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("Content cannot be null or whitespace.*")
			.And.ParamName.Should().Be("content");
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		var empty = Article.Empty;

		// Assert
		empty.Id.Should().Be(ObjectId.Empty);
		empty.Title.Should().Be(string.Empty);
		empty.Introduction.Should().Be(string.Empty);
		empty.Content.Should().Be(string.Empty);
		empty.CoverImageUrl.Should().Be(string.Empty);
		empty.Slug.Should().Be(string.Empty);
		empty.Author.Should().BeNull();
		empty.Category.Should().BeNull();
		empty.IsPublished.Should().BeFalse();
		empty.PublishedOn.Should().BeNull();
		empty.IsArchived.Should().BeFalse();
	}

	[Fact]
	public void Update_WithValidData_ShouldUpdatePropertiesAndSetModifiedOn()
	{
		// Arrange
		var article = new Article("Old Title", "Old Intro", "Old Content", "old.jpg", null, null);
		var beforeModified = DateTimeOffset.UtcNow;

		// Act
		article.Update("New Title", "New Intro", "New Content", "new.jpg", true, DateTimeOffset.UtcNow, true);

		// Assert
		article.Title.Should().Be("New Title");
		article.Introduction.Should().Be("New Intro");
		article.Content.Should().Be("New Content");
		article.CoverImageUrl.Should().Be("new.jpg");
		article.Slug.Should().Be("new_title");
		article.IsPublished.Should().BeTrue();
		article.IsArchived.Should().BeTrue();
		article.ModifiedOn.Should().NotBeNull();
		article.ModifiedOn.Should().BeOnOrAfter(beforeModified);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("  ")]
	public void Update_WithInvalidTitle_ShouldThrowArgumentException(string? invalidTitle)
	{
		// Arrange
		var article = new Article("Title", "Intro", "Content", "cover.jpg", null, null);

		// Act
		var act = () => article.Update(invalidTitle!, "Intro", "Content", "cover.jpg", false, null, false);

		// Assert
		act.Should().Throw<ArgumentException>()
			.WithMessage("Title cannot be null or whitespace.*")
			.And.ParamName.Should().Be("title");
	}

	[Fact]
	public void Publish_ShouldSetPublishedProperties()
	{
		// Arrange
		var article = new Article("Title", "Intro", "Content", "cover.jpg", null, null);
		var publishedOn = DateTimeOffset.UtcNow;
		var beforeModified = DateTimeOffset.UtcNow;

		// Act
		article.Publish(publishedOn);

		// Assert
		article.IsPublished.Should().BeTrue();
		article.PublishedOn.Should().Be(publishedOn);
		article.ModifiedOn.Should().NotBeNull();
		article.ModifiedOn.Should().BeOnOrAfter(beforeModified);
	}

	[Fact]
	public void Unpublish_ShouldClearPublishedProperties()
	{
		// Arrange
		var article = new Article("Title", "Intro", "Content", "cover.jpg", null, null, true, DateTimeOffset.UtcNow, false);
		var beforeModified = DateTimeOffset.UtcNow;

		// Act
		article.Unpublish();

		// Assert
		article.IsPublished.Should().BeFalse();
		article.PublishedOn.Should().BeNull();
		article.ModifiedOn.Should().NotBeNull();
		article.ModifiedOn.Should().BeOnOrAfter(beforeModified);
	}

	[Theory]
	[InlineData("Test Article", "test_article")]
	[InlineData("Hello World!", "hello_world")]
	[InlineData("C# Programming", "c_programming")]
	[InlineData("Multiple   Spaces", "multiple___spaces")]
	[InlineData("Special@Characters#Here", "specialcharactershere")]
	[InlineData("123 Numbers 456", "123_numbers_456")]
	public void SlugGeneration_ShouldConvertTitleToValidSlug(string title, string expectedSlug)
	{
		// Arrange & Act
		var article = new Article(title, "Intro", "Content", "cover.jpg", null, null);

		// Assert
		article.Slug.Should().Be(expectedSlug);
	}

	[Fact]
	public void Properties_ShouldBeSettable()
	{
		// Arrange
		var article = new Article();
		var author = new AuthorInfo("auth0|123", "John Doe");
		var category = new Category { CategoryName = "Tech" };

		// Act
		article.Title = "New Title";
		article.Introduction = "New Intro";
		article.Content = "New Content";
		article.CoverImageUrl = "new.jpg";
		article.Author = author;
		article.Category = category;
		article.IsPublished = true;
		article.PublishedOn = DateTimeOffset.UtcNow;
		article.IsArchived = true;

		// Assert
		article.Title.Should().Be("New Title");
		article.Introduction.Should().Be("New Intro");
		article.Content.Should().Be("New Content");
		article.CoverImageUrl.Should().Be("new.jpg");
		article.Author.Should().Be(author);
		article.Category.Should().Be(category);
		article.IsPublished.Should().BeTrue();
		article.PublishedOn.Should().NotBeNull();
		article.IsArchived.Should().BeTrue();
	}
}
