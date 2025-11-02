// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using NSubstitute;

using Shared.Abstractions;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;

using Web.Components.Features.Articles.ArticleCreate;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleCreate;

/// <summary>
///   Unit tests for CreateArticle.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateArticleHandlerTests
{

	private readonly IArticleRepository _mockRepository;

	private readonly ILogger<CreateArticle.Handler> _mockLogger;

	private readonly CreateArticle.ICreateArticleHandler _handler;

	public CreateArticleHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		_mockLogger = Substitute.For<ILogger<CreateArticle.Handler>>();
		_handler = new CreateArticle.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
	{
		// Arrange
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var articleDto = new ArticleDto(
				ObjectId.GenerateNewId(),
				"test-article",
				"Test Article",
				"Test Intro",
				"Test Content",
				"https://example.com/image.jpg",
				author,
				category,
				false,
				null,
				DateTimeOffset.UtcNow,
				null,
				false,
				false
		);

		var createdArticle = new Article(
				articleDto.Title,
				articleDto.Introduction,
				articleDto.Content,
				articleDto.CoverImageUrl,
				articleDto.Author,
				articleDto.Category,
				articleDto.IsPublished,
				articleDto.PublishedOn,
				articleDto.IsArchived,
				articleDto.Slug
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result.Ok(createdArticle)));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Title.Should().Be("Test Article");
		result.Value.Introduction.Should().Be("Test Intro");
		result.Value.Content.Should().Be("Test Content");

		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
				a.Title == "Test Article" &&
				a.Introduction == "Test Intro" &&
				a.Content == "Test Content"
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullRequest_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(null);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Article data cannot be null");
		result.Value.Should().BeNull();
		await _mockRepository.DidNotReceive().AddArticle(Arg.Any<Article>());
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryFails_ShouldReturnFailure()
	{
		// Arrange
		var articleDto = new ArticleDto(
				ObjectId.GenerateNewId(),
				"test-article",
				"Test Article",
				"Test Intro",
				"Test Content",
				"https://example.com/image.jpg",
				new AuthorInfo("user1", "Test Author"), // Always provide valid Author
				new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Tech" }, // Always provide valid Category
				false,
				null,
				DateTimeOffset.UtcNow,
				null,
				false,
				false
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Fail("Database error")));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
		result.Value.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllDtoProperties()
	{
		// Arrange
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);

		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { Id = ObjectId.GenerateNewId(), CategoryName = "Tech" };

		var articleDto = new ArticleDto(
				ObjectId.GenerateNewId(),
				"test-slug",
				"Test Title",
				"Test Intro",
				"Test Content",
				"https://example.com/image.jpg",
				author,
				category,
				true,
				publishedOn,
				createdOn,
				null,
				false,
				false
		);

		var createdArticle = new Article(
				articleDto.Title,
				articleDto.Introduction,
				articleDto.Content,
				articleDto.CoverImageUrl,
				articleDto.Author,
				articleDto.Category,
				articleDto.IsPublished,
				articleDto.PublishedOn,
				articleDto.IsArchived,
				articleDto.Slug
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result.Ok(createdArticle)));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Title.Should().Be("Test Title");
		result.Value.Introduction.Should().Be("Test Intro");
		result.Value.Content.Should().Be("Test Content");
		result.Value.CoverImageUrl.Should().Be("https://example.com/image.jpg");
		result.Value.Author.Should().Be(author);
		result.Value.Category.Should().Be(category);
		result.Value.IsPublished.Should().BeTrue();
		result.Value.PublishedOn.Should().Be(publishedOn);
		result.Value.IsArchived.Should().BeFalse();

		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
				a.Title == "Test Title" &&
				a.Introduction == "Test Intro" &&
				a.Content == "Test Content" &&
				a.CoverImageUrl == "https://example.com/image.jpg" &&
				a.Author == author &&
				a.Category == category &&
				a.IsPublished == true &&
				a.PublishedOn == publishedOn &&
				a.IsArchived == false
		));
	}

}