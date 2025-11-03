// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticleHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
///   Integration tests for GetArticle handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class GetArticleHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler _handler;

	private readonly ILogger<Components.Features.Articles.ArticleDetails.GetArticle.Handler> _logger;

	public GetArticleHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		_logger = Substitute.For<ILogger<Components.Features.Articles.ArticleDetails.GetArticle.Handler>>();
		_handler = new Components.Features.Articles.ArticleDetails.GetArticle.Handler(_repository, _logger);
	}

	[Fact]
	public async Task HandleAsync_WithValidId_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(article.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(article.Id);
		result.Value.Title.Should().Be(article.Title);
		result.Value.Content.Should().Be(article.Content);
	}

	[Fact]
	public async Task HandleAsync_WithInvalidId_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var nonExistentId = ObjectId.GenerateNewId().ToString();

		// Act
		var result = await _handler.HandleAsync(nonExistentId);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Contain("not found");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyId_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(string.Empty);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Article ID cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WithInvalidObjectIdFormat_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync("invalid-id-format");

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Invalid article ID format");
	}

	[Fact]
	public async Task HandleAsync_WithArchivedArticle_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(article.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeTrue();
		result.Value.CanEdit.Should().BeFalse(); // Archived articles cannot be edited
	}

	[Fact]
	public async Task HandleAsync_WithUnpublishedArticle_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = false;
		article.PublishedOn = null;

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(article.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsPublished.Should().BeFalse();
		result.Value.PublishedOn.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WithNullId_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(null!);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Article ID cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_VerifiesSlugIsReturned()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(article.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().NotBeNullOrWhiteSpace();
		result.Value.Slug.Should().Be(article.Slug);
	}

}



