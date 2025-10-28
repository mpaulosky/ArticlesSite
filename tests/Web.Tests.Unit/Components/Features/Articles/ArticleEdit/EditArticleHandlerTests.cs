// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticleHandlerTests.cs
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

using Web.Components.Features.Articles.ArticleEdit;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleEdit;

/// <summary>
///   Unit tests for EditArticle.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class EditArticleHandlerTests
{

	private readonly IArticleRepository _mockRepository;

	private readonly ILogger<EditArticle.Handler> _mockLogger;

	private readonly EditArticle.Handler _handler;

	public EditArticleHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		_mockLogger = Substitute.For<ILogger<EditArticle.Handler>>();
		_handler = new EditArticle.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var existingArticle = new Article("Old Title", "Intro", "Content", string.Empty, author, category, false, null, false)
		{
			Id = objectId
		};

		var articleDto = new ArticleDto(
			objectId,
			"updated-article",
			"Updated Article",
			"Updated Intro",
			"Updated Content",
			"https://example.com/updated.jpg",
			author,
			category,
			true,
			DateTimeOffset.UtcNow,
			DateTimeOffset.UtcNow.AddDays(-10),
			null,
			false,
			false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Ok(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).GetArticleByIdAsync(objectId);
		await _mockRepository.Received(1).UpdateArticle(Arg.Is<Article>(a =>
			a.Title == "Updated Article" &&
			a.Introduction == "Updated Intro" &&
			a.Slug == "updated_article"
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
		await _mockRepository.DidNotReceive().GetArticleByIdAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task HandleAsync_WhenArticleNotFound_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var articleDto = new ArticleDto(
			objectId,
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"",
			null,
			null,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Fail("Article not found")));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Article not found");
		await _mockRepository.DidNotReceive().UpdateArticle(Arg.Any<Article>());
	}

	[Fact]
	public async Task HandleAsync_WhenUpdateFails_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var existingArticle = new Article { Id = objectId, Title = "Old Title" };
		var articleDto = new ArticleDto(
			objectId,
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"",
			null,
			null,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Ok(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Fail("Update failed")));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Update failed");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var articleDto = new ArticleDto(
			objectId,
			"test-article",
			"Test Article",
			"Test Intro",
			"Test Content",
			"",
			null,
			null,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns<Task<Result<Article?>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Connection failed");
	}

	[Fact]
	public async Task HandleAsync_ShouldUpdateAllProperties()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var author = new AuthorInfo("user1", "Test Author");
		var categoryId = ObjectId.GenerateNewId();
		var category = new Category { Id = categoryId, CategoryName = "Tech" };

		var existingArticle = new Article { Id = objectId, Title = "Old Title" };

		var articleDto = new ArticleDto(
			objectId,
			"updated-slug",
			"Updated Title",
			"Updated Intro",
			"Updated Content",
			"https://example.com/updated.jpg",
			author,
			category,
			true,
			publishedOn,
			DateTimeOffset.UtcNow.AddDays(-10),
			null,
			false,
			false
		);

		_mockRepository.GetArticleByIdAsync(objectId).Returns(Task.FromResult(Result<Article?>.Ok(existingArticle)));
		_mockRepository.UpdateArticle(Arg.Any<Article>()).Returns(Task.FromResult(Result<Article>.Ok(new Article())));

		// Act
		var result = await _handler.HandleAsync(articleDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).UpdateArticle(Arg.Is<Article>(a =>
			a.Title == "Updated Title" &&
			a.Introduction == "Updated Intro" &&
			a.Content == "Updated Content" &&
			a.Slug == "updated_title" &&
			a.CoverImageUrl == "https://example.com/updated.jpg" &&
			a.Author == author &&
			a.Category == category &&
			a.IsPublished == true &&
			a.PublishedOn == publishedOn &&
			a.IsArchived == false &&
			a.ModifiedOn != null
		));
	}

}
