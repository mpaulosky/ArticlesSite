// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

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

	private readonly CreateArticle.Handler _handler;

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

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
			a.Title == "Test Article" &&
			a.Introduction == "Test Intro" &&
			a.Content == "Test Content" &&
			a.Slug == "test_article"
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullRequest_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(null);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The request is null.");
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
			null,
			null,
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
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var articleDto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"https://example.com/image.jpg",
			null,
			null,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns<Task<Result<Article>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().StartWith("An error occurred while creating the article:");
		result.Error.Should().Contain("Connection failed");
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

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
			a.Slug == "test_title" &&
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

	[Fact]
	public async Task HandleAsync_WithNullAuthor_ShouldUseEmptyAuthorId()
	{
		// Arrange
		var articleDto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"",
			null, // Null author
			null,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
			a.Author == null
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullCategory_ShouldUseEmptyObjectId()
	{
		// Arrange
		var articleDto = new ArticleDto(
			ObjectId.GenerateNewId(),
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"",
			null,
			null, // Null category
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.AddArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddArticle(Arg.Is<Article>(a =>
			a.Category == null
		));
	}

}
