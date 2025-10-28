// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleRepositoryIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using System.ComponentModel.DataAnnotations;

namespace Web.Tests.Integration.Repositories;

/// <summary>
///   Integration tests for ArticleRepository with real MongoDB container
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleRepositoryIntegrationTests
{

	private readonly MongoDbFixture _fixture;
	private readonly IArticleRepository _repository;

	public ArticleRepositoryIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
	}

	[Fact]
	public async Task GetArticles_WithNoArticles_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _repository.GetArticles();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.BeEmpty();
	}

	[Fact]
	public async Task GetArticles_WithMultipleArticles_ReturnsAllArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(2, useSeed: true);

		// Ensure articles are not archived
		foreach (var article in articles)
		{
			article.IsPublished = true;
		}

		var articleTitles = articles.Select(a => a.Title).ToList();

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticles();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().Contain(a => a.Title == articleTitles[0]);
		result.Value.Should().Contain(a => a.Title == articleTitles[1]);
	}

	[Fact]
	public async Task GetArticleByIdAsync_WithValidId_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticleByIdAsync(article.Id);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(article.Id);
		result.Value.Title.Should().Be(article.Title);
		result.Value.Content.Should().Be(article.Content);
	}

	[Fact]
	public async Task GetArticleByIdAsync_WithInvalidId_ReturnsNull()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.GetArticleByIdAsync(nonExistentId);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().BeNull();
	}

	[Fact]
	public async Task AddArticle_WithValidArticle_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = false;

		// Act
		var result = await _repository.AddArticle(article);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().NotBe(ObjectId.Empty);
		result.Value.Title.Should().Be(article.Title);

		// Verify in database
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		var dbArticle = await collection.Find(a => a.Id == result.Value.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		dbArticle.Should().NotBeNull();
		dbArticle!.Title.Should().Be(article.Title);
	}

	[Fact]
	public async Task UpdateArticle_WithExistingArticle_UpdatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Modify article
		article.Update("Updated Title", "Updated Intro", "Updated Content", "https://example.com/updated.jpg", true, DateTimeOffset.UtcNow, false);

		// Act
		var result = await _repository.UpdateArticle(article);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Title.Should().Be("Updated Title");
		result.Value.Content.Should().Be("Updated Content");
		result.Value.IsPublished.Should().BeTrue();

		// Verify in database
		var dbArticle = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		dbArticle.Should().NotBeNull();
		dbArticle!.Title.Should().Be("Updated Title");
	}

	[Fact]
	public async Task ArchiveArticle_WithExistingArticle_ArchivesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;
		article.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		await _repository.ArchiveArticle(article.Slug);

		// Assert - Verify in database
		var dbArticle = await collection.Find(a => a.Slug == article.Slug).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		dbArticle.Should().NotBeNull();
		dbArticle!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task ConcurrentOperations_HandleMultipleThreadsSafely()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act - Create multiple articles concurrently
		var articles = FakeArticle.GetArticles(10, useSeed: true);

		foreach (var article in articles)
		{
			article.IsPublished = true;
		}

		var tasks = articles.Select(article => _repository.AddArticle(article)).ToArray();

		var results = await Task.WhenAll(tasks);

		// Assert
		results.Should().HaveCount(10);
		results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

		var allArticles = await _repository.GetArticles();
		allArticles.Value.Should().HaveCount(10);
	}

	[Fact]
	public async Task AddArticle_WithMissingRequiredFields_ShouldFailValidation()
	{
		await _fixture.ClearCollectionsAsync();

		var invalidArticle = new Article
		{
			Id = ObjectId.Empty,
			Title = "",
			Introduction = "",
			Content = "",
			CoverImageUrl = "",
			Author = null,
			Category = null
		};

		var context = new ValidationContext(invalidArticle);
		var results = new List<ValidationResult>();
		var isValid = Validator.TryValidateObject(invalidArticle, context, results, true);

		isValid.Should().BeFalse();
		// Id auto-generates, so DataAnnotations may not report it as missing
		results.Should().Contain(r => r.ErrorMessage == "Title is required");
		results.Should().Contain(r => r.ErrorMessage == "Introduction is required");
		results.Should().Contain(r => r.ErrorMessage == "Content is required");
		results.Should().Contain(r => r.ErrorMessage == "Cover image is required");
		results.Should().Contain(r => r.ErrorMessage == "Author is required");
		results.Should().Contain(r => r.ErrorMessage == "Category is required");
	}

}

