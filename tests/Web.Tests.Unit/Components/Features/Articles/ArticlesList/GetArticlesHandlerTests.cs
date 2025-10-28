// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticlesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;

using Web.Components.Features.Articles.ArticlesList;

namespace Web.Tests.Unit.Components.Features.Articles.ArticlesList;

/// <summary>
///   Unit tests for GetArticles.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArticlesHandlerTests
{

	private readonly IArticleRepository _mockRepository;

	private readonly ILogger<GetArticles.Handler> _mockLogger;

	private readonly GetArticles.Handler _handler;

	public GetArticlesHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		_mockLogger = Substitute.For<ILogger<GetArticles.Handler>>();
		_handler = new GetArticles.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithArticlesAvailable_ShouldReturnSuccess()
	{
		// Arrange
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
			new("Test 1", "Intro 1", "Content 1", string.Empty, author, category) { IsPublished = true },
			new("Test 2", "Intro 2", "Content 2", string.Empty, author, category) { IsPublished = false }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Ok((IEnumerable<Article>)articles)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(2);

		var firstDto = result.Value!.First();
		firstDto.Title.Should().Be("Test 1");
		firstDto.Author.Should().Be(author);
		firstDto.Category.Should().Be(category);
		firstDto.IsPublished.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithEmptyList_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Ok((IEnumerable<Article>)new List<Article>())));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("No articles found.");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Fail("Database error")));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetArticles().Returns<Task<Result<IEnumerable<Article>?>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Connection failed");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllArticleProperties()
	{
		// Arrange
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var article = new Article("Test Title", "Test Intro", "Test Content", "https://example.com/image.jpg", author, category)
		{
			IsPublished = true,
			PublishedOn = publishedOn,
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Ok((IEnumerable<Article>)new List<Article> { article })));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		var dto = result.Value!.First();

		dto.Slug.Should().Be("test_title");
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

	[Fact]
	public async Task HandleAsync_WithMultipleArticles_ShouldReturnAllInCorrectOrder()
	{
		// Arrange
		var articles = new List<Article>
		{
			new("First", "Intro", "Content", string.Empty, null, null),
			new("Second", "Intro", "Content", string.Empty, null, null),
			new("Third", "Intro", "Content", string.Empty, null, null)
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Ok((IEnumerable<Article>)articles)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(3);

		var dtos = result.Value!.ToList();
		dtos[0].Title.Should().Be("First");
		dtos[1].Title.Should().Be("Second");
		dtos[2].Title.Should().Be("Third");
	}

}
