// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
///   Integration tests for CreateArticle handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class CreateArticleHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly CreateArticle.ICreateArticleHandler _handler;

	public CreateArticleHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		ILogger<CreateArticle.Handler> logger = Substitute.For<ILogger<CreateArticle.Handler>>();
		IValidator<ArticleDto>? validator = Substitute.For<IValidator<ArticleDto>?>();
		_handler = new CreateArticle.Handler(_repository, logger, validator);
	}

	[Fact]
	public async Task HandleAsync_WithValidDto_CreatesArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
				ObjectId.Empty,
				"test-article",
				"Test Article",
				"Test Introduction",
				"Test Content",
				"http://example.com/image.jpg",
				author,
				category,
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Title.Should().Be("Test Article");
		result.Value.Introduction.Should().Be("Test Introduction");
		result.Value.Content.Should().Be("Test Content");
		result.Value.Id.Should().NotBe(ObjectId.Empty);

		// Verify it was actually saved to a database
		var saved = await _repository.GetArticleByIdAsync(result.Value.Id);
		saved.Success.Should().BeTrue();
		saved.Value.Should().NotBeNull();
	}

	[Fact]
	public async Task HandleAsync_WithNullDto_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(null!);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Article data cannot be null");
	}

	[Fact]
	public async Task HandleAsync_WithValidDto_SetsCorrectTimestamps()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);
		var now = DateTimeOffset.UtcNow;

		var dto = new ArticleDto(
				ObjectId.Empty,
				"test-article",
				"Test Article",
				"Test Introduction",
				"Test Content",
				"http://example.com/image.jpg",
				author,
				category,
				true,
				now,
				now,
				null,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CreatedOn.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
		result.Value.ModifiedOn.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WithMultipleArticles_CreatesAllSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto1 = new ArticleDto(
				ObjectId.Empty,
				"article-one",
				"Article One",
				"Introduction One",
				"Content One",
				"http://example.com/image1.jpg",
				author,
				category,
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				false,
				true
		);

		var dto2 = new ArticleDto(
				ObjectId.Empty,
				"article-two",
				"Article Two",
				"Introduction Two",
				"Content Two",
				"http://example.com/image2.jpg",
				author,
				category,
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				null,
				false,
				true
		);

		// Act
		var result1 = await _handler.HandleAsync(dto1);
		var result2 = await _handler.HandleAsync(dto2);

		// Assert
		result1.Success.Should().BeTrue();
		result2.Success.Should().BeTrue();
		result1.Value!.Id.Should().NotBe(result2.Value!.Id);

		// Verify both exist in a database
		var allArticles = await _repository.GetArticles();
		allArticles.Success.Should().BeTrue();
		allArticles.Value.Should().HaveCount(2);
	}

	[Fact]
	public async Task HandleAsync_WithUnpublishedArticle_CreatesCorrectly()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
				ObjectId.Empty,
				"unpublished-article",
				"Unpublished Article",
				"Test Introduction",
				"Test Content",
				"http://example.com/image.jpg",
				author,
				category,
				false, // Not published
				null,  // No published date
				DateTimeOffset.UtcNow,
				null,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsPublished.Should().BeFalse();
		result.Value.PublishedOn.Should().BeNull();
	}

}
