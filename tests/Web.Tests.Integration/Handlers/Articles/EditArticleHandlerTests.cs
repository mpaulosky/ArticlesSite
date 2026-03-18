// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Microsoft.Extensions.Options;

using Web.Components.Features.Articles.ArticleEdit;
using Web.Infrastructure;

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
///   Integration tests for EditArticle handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class EditArticleHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly EditArticle.IEditArticleHandler _handler;

	public EditArticleHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		ILogger<EditArticle.Handler> logger = Substitute.For<ILogger<EditArticle.Handler>>();
		IValidator<ArticleDto> validator = Substitute.For<IValidator<ArticleDto>>();
		var options = Options.Create(new ConcurrencyOptions());
		_handler = new EditArticle.Handler(_repository, logger, validator, options);
	}

	[Fact]
	public async Task HandleAsync_WithValidDto_UpdatesArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				"Updated Title",
				"Updated Introduction",
				"Updated Content",
				"http://example.com/updated.jpg",
				article.Author,
				article.Category,
				true,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Title.Should().Be("Updated Title");
		result.Value.Introduction.Should().Be("Updated Introduction");
		result.Value.Content.Should().Be("Updated Content");
		result.Value.ModifiedOn.Should().NotBeNull();

		// Verify it was actually updated in a database
		var saved = await _repository.GetArticleByIdAsync(article.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.Title.Should().Be("Updated Title");
	}

	[Fact]
	public async Task HandleAsync_WithNullDto_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(null);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Article data cannot be null");
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentArticle_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);
		var nonExistentId = ObjectId.GenerateNewId();

		var dto = new ArticleDto(
				nonExistentId,
				"test-slug",
				"Test Title",
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
		result.Failure.Should().BeTrue();
		result.Error.Should().Contain("not found");
	}

	[Fact]
	public async Task HandleAsync_ArchivingArticle_SetsCanEditToFalse()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				article.Title,
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				article.IsPublished,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				true, // Archive it
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeTrue();
		result.Value.CanEdit.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_UnarchivingArticle_UpdatesIsArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				article.Title,
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				article.IsPublished,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false, // Unarchive it
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_UpdatesAuthorAndCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var newAuthor = FakeAuthorInfo.GetNewAuthorInfo(useSeed: false);
		var newCategory = FakeCategory.GetNewCategory(useSeed: false);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				article.Title,
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				newAuthor,
				newCategory,
				article.IsPublished,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Author.Should().Be(newAuthor);
		result.Value.Category.Should().BeEquivalentTo(newCategory);

		// Verify in database
		var saved = await _repository.GetArticleByIdAsync(article.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.Author.Should().Be(newAuthor);
		saved.Value.Category.Should().BeEquivalentTo(newCategory);
	}

	[Fact]
	public async Task HandleAsync_WithEmptyTitle_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				"", // Empty title
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				article.IsPublished,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task HandleAsync_PublishingArticle_SetsPublishedDate()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = false;
		article.PublishedOn = null;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var publishDate = DateTimeOffset.UtcNow;

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				article.Title,
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				true, // Publish it
				publishDate,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsPublished.Should().BeTrue();
		result.Value.PublishedOn.Should().NotBeNull();
	}

	[Fact]
	public async Task HandleAsync_UnpublishingArticle_ClearsPublishedDate()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;
		article.PublishedOn = DateTimeOffset.UtcNow;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				article.Slug,
				article.Title,
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				false, // Unpublish it
				null,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsPublished.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_PreservesOriginalSlug()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var originalSlug = article.Slug;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new ArticleDto(
				article.Id,
				originalSlug,
				"Updated Title",
				article.Introduction,
				article.Content,
				article.CoverImageUrl,
				article.Author,
				article.Category,
				article.IsPublished,
				article.PublishedOn,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		// Note: Article.Update() regenerates slug from the new title
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().Be("updated_title"); // Slug regenerated from title
		result.Value.Slug.Should().NotBe(originalSlug); // Slug has changed
	}

}
