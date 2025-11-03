// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticlesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using System.Security.Claims;

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
///   Integration tests for GetArticles handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class GetArticlesHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly Components.Features.Articles.ArticlesList.GetArticles.IGetArticlesHandler _handler;

	private readonly ILogger<Components.Features.Articles.ArticlesList.GetArticles.Handler> _logger;

	public GetArticlesHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		_logger = Substitute.For<ILogger<Components.Features.Articles.ArticlesList.GetArticles.Handler>>();
		_handler = new Components.Features.Articles.ArticlesList.GetArticles.Handler(_repository, _logger);
	}

	[Fact]
	public async Task HandleAsync_WithNoArticles_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.BeEmpty();
	}

	[Fact]
	public async Task HandleAsync_WithMultipleArticles_ReturnsAllNonArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(3, useSeed: true);
		articles[0].IsArchived = false;
		articles[0].IsPublished = true;
		articles[1].IsArchived = false;
		articles[1].IsPublished = true;
		articles[2].IsArchived = true; // This one should be filtered out

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().OnlyContain(a => !a.IsArchived);
	}

	[Fact]
	public async Task HandleAsync_WithIncludeArchived_ReturnsAllArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(3, useSeed: true);
		articles[0].IsArchived = false;
		articles[1].IsArchived = false;
		articles[2].IsArchived = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(includeArchived: true);

		// Assert
		// Repository now returns all articles, handler respects includeArchived parameter
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(3);
	}

	[Fact]
	public async Task HandleAsync_WithFilterByUser_ReturnsOnlyUserArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var author1 = new AuthorInfo("user123", "Test Author 1");
		var author2 = new AuthorInfo("user456", "Test Author 2");

		var articles = FakeArticle.GetArticles(3, useSeed: true);
		articles[0].Author = author1;
		articles[0].IsArchived = false;
		articles[1].Author = author1;
		articles[1].IsArchived = false;
		articles[2].Author = author2;
		articles[2].IsPublished = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");

		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "user123") }));

		// Act
		var result = await _handler.HandleAsync(user: user, filterByUser: true);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().OnlyContain(a => a.Author != null && a.Author.UserId == "user123");
	}

	[Fact]
	public async Task HandleAsync_AdminUser_CanEditAllArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(2, useSeed: true);
		articles[0].IsArchived = false;
		articles[1].IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		var adminUser = new ClaimsPrincipal(new ClaimsIdentity(
				new[] { new Claim(ClaimTypes.NameIdentifier, "admin123"), new Claim(ClaimTypes.Role, "Admin") }, "TestAuth"));

		// Act
		var result = await _handler.HandleAsync(user: adminUser);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().OnlyContain(a => a.CanEdit);
	}

	[Fact]
	public async Task HandleAsync_NonAdminUser_CannotEditOthersArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var author = new AuthorInfo("other-user", "Other User");

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Author = author;
		article.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "current-user") },
				"TestAuth"));

		// Act
		var result = await _handler.HandleAsync(user: user);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().CanEdit.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_WithNoUser_DefaultsCanEditToTrue()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(user: null);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().CanEdit.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_PreservesExistingSlug()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = false;
		var expectedSlug = article.Slug;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().Slug.Should().Be(expectedSlug);
	}

	[Fact]
	public async Task HandleAsync_ArchivedArticles_CannotBeEdited()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var adminUser = new ClaimsPrincipal(new ClaimsIdentity(
				new[] { new Claim(ClaimTypes.NameIdentifier, "admin123"), new Claim(ClaimTypes.Role, "Admin") }, "TestAuth"));

		// Act
		var result = await _handler.HandleAsync(user: adminUser, includeArchived: true);

		// Assert
		// Repository now returns all articles including archived
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().IsArchived.Should().BeTrue();
		result.Value.First().CanEdit.Should().BeFalse(); // Archived articles cannot be edited
	}

}



