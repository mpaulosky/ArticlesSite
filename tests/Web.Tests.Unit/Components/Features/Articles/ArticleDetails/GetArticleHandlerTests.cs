// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using NSubstitute;

using Shared.Entities;
using Shared.Interfaces;

using Web.Components.Features.Articles.ArticleDetails;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleDetails;

/// <summary>
///   Unit tests for GetArticle.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArticleHandlerTests
{

	private readonly IArticleRepository _mockRepository;

	private readonly ILogger<GetArticle.Handler> _mockLogger;

	private readonly GetArticle.Handler _handler;

	public GetArticleHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		_mockLogger = Substitute.For<ILogger<GetArticle.Handler>>();
		_handler = new GetArticle.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidId_ShouldReturnSuccess()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var article = new Article("Test Article", "Test Intro", "Test Content", null, author, category, true, null, false)
		{
			Id = objectId
		};

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Ok(article)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(objectId);
		result.Value.Title.Should().Be("Test Article");
		result.Value.Author.Should().Be(author);
		result.Value.Category.Should().Be(category);
	}

	[Fact]
	public async Task HandleAsync_WithNullId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(null!);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(string.Empty);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithWhitespaceId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync("   ");

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithInvalidObjectId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync("invalid-id");

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WhenArticleNotFound_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Fail("Article not found")));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Article not found");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var failResult = Result<Article?>.Fail("Database error");
		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(failResult));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		_mockRepository.GetArticleByIdAsync(objectId).Returns<Task<Result<Article?>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Connection failed");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllArticleProperties()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var article = new Article("Test Title", "Test Intro", "Test Content", null, author, category, true, publishedOn, false)
		{
			Id = objectId,
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Ok(article)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		var dto = result.Value!;

		dto.Id.Should().Be(objectId);
		dto.Title.Should().Be("Test Title");
		dto.Introduction.Should().Be("Test Intro");
		dto.Content.Should().Be("Test Content");
		dto.CoverImageUrl.Should().Be("https://example.com/image.jpg");
		dto.Author.Should().Be(author);
		dto.Category.Should().Be(category);
		dto.IsPublished.Should().BeTrue();
		dto.PublishedOn.Should().Be(publishedOn);
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
		dto.CanEdit.Should().BeFalse(); // Default value in DTO constructor
	}

}