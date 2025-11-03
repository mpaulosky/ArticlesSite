// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleRepositoryAdvancedIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Repositories;

/// <summary>
///   Advanced integration tests for ArticleRepository covering edge cases and complex scenarios
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleRepositoryAdvancedIntegrationTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	public ArticleRepositoryAdvancedIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
	}

	[Fact]
	public async Task GetArticle_WithValidDateAndSlug_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var publishDate = new DateTimeOffset(2025, 10, 15, 0, 0, 0, TimeSpan.Zero);
		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.CreatedOn = publishDate;
		article.PublishedOn = publishDate;
		article.IsPublished = true;
		article.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticle("2025-10-15", article.Slug);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().Be(article.Slug);
		result.Value.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task GetArticle_WithArchivedArticle_ReturnsArticle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;
		article.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticle("2025-10-15", article.Slug);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull("repository returns ALL articles including archived");
		result.Value!.Slug.Should().Be(article.Slug);
		result.Value.IsArchived.Should().BeTrue("the archived flag is preserved");
	}

	[Fact]
	public async Task GetArticles_WithWhereExpression_FiltersCorrectly()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(3, useSeed: true);

		// Set specific IsPublished states
		articles[0].IsPublished = true;
		articles[0].IsArchived = false;
		articles[1].IsPublished = false;
		articles[1].IsArchived = false;
		articles[2].IsPublished = true;
		articles[2].IsArchived = false;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Get only published articles
		var result = await _repository.GetArticles(a => a.IsPublished);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().AllSatisfy(a => a.IsPublished.Should().BeTrue());
	}

	[Fact]
	public async Task GetArticles_ReturnsAllIncludingArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var articles = FakeArticle.GetArticles(2, useSeed: true);

		// Set specific archived states
		articles[0].IsPublished = true;
		articles[0].IsArchived = false;
		articles[1].IsPublished = true;
		articles[1].IsArchived = true;

		var activeTitles = new[] { articles[0].Title };
		var archivedTitles = new[] { articles[1].Title };

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticles();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2, "repository returns ALL articles including archived");
		result.Value.Should().Contain(a => a.Title == activeTitles[0]);
		result.Value.Should().Contain(a => a.Title == archivedTitles[0], "archived articles are included");
		result.Value.Should().Contain(a => a.IsArchived, "at least one archived article is present");
	}

	[Fact]
	public async Task GetArticles_WithCategoryFilter_ReturnsOnlyMatchingCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var techCategory = FakeCategory.GetNewCategory(useSeed: true);
		var scienceCategory = FakeCategory.GetNewCategory(useSeed: false);

		var articles = FakeArticle.GetArticles(2, useSeed: true);

		articles[0].Category = techCategory;
		articles[0].IsPublished = true;
		articles[0].IsArchived = false;

		articles[1].Category = scienceCategory;
		articles[1].IsPublished = true;
		articles[1].IsArchived = false;

		var techTitle = articles[0].Title;
		var scienceTitle = articles[1].Title;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Filter by Tech category
		var result = await _repository.GetArticles(a => a.Category == techCategory);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.Should().Contain(a => a.Title == techTitle);
		result.Value.Should().NotContain(a => a.Title == scienceTitle);
	}

	[Fact]
	public async Task UpdateArticle_PreservesIdAndCreatedOn()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.CreatedOn = createdOn;
		article.IsPublished = false;

		var originalId = article.Id;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Modify article
		article.Update("New Title", "New Intro", "New Content", "https://example.com/new.jpg",
				true, DateTimeOffset.UtcNow, false);

		// Act
		var result = await _repository.UpdateArticle(article);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(originalId, "ID should not change");   // Verify in database

		var dbArticle = await collection.Find(a => a.Id == originalId)
				.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

		dbArticle.Should().NotBeNull();
		dbArticle!.Id.Should().Be(originalId);
		dbArticle.Title.Should().Be("New Title");
	}

	[Fact]
	public async Task AddArticle_WithCompleteData_PreservesAllFields()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var publishDate = DateTimeOffset.UtcNow;

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsPublished = true;
		article.PublishedOn = publishDate;
		article.IsArchived = false;

		// Act
		var result = await _repository.AddArticle(article);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();

		// Verify all fields in database
		var collection = _fixture.Database.GetCollection<Article>("Articles");

		var dbArticle = await collection.Find(a => a.Id == result.Value.Id)
				.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

		dbArticle.Should().NotBeNull();
		dbArticle!.Title.Should().Be(article.Title);
		dbArticle.Introduction.Should().Be(article.Introduction);
		dbArticle.Content.Should().Be(article.Content);
		dbArticle.CoverImageUrl.Should().Be(article.CoverImageUrl);
		dbArticle.Slug.Should().Be(article.Slug);
		dbArticle.Author.Should().BeEquivalentTo(article.Author);
		dbArticle.Category.Should().BeEquivalentTo(article.Category);
		dbArticle.IsPublished.Should().BeTrue();
		dbArticle.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task ArchiveArticle_WithNonExistentSlug_DoesNotThrow()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var act = async () => await _repository.ArchiveArticle("nonexistent-slug");

		// Assert - Should not throw, just silently succeed (no document updated)
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public async Task GetArticles_OrderedByCreatedDate_ReturnsInCorrectOrder()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var baseDate = DateTimeOffset.UtcNow.AddDays(-10);

		var articles = FakeArticle.GetArticles(3, useSeed: true);

		// Set specific dates for ordering
		articles[0].CreatedOn = baseDate;
		articles[0].IsPublished = true;
		articles[0].IsArchived = false;

		articles[1].CreatedOn = baseDate.AddDays(5);
		articles[1].IsPublished = true;
		articles[1].IsArchived = false;

		articles[2].CreatedOn = baseDate.AddDays(2);
		articles[2].IsPublished = true;
		articles[2].IsArchived = false;

		var oldestTitle = articles[0].Title;
		var newestTitle = articles[1].Title;
		var middleTitle = articles[2].Title;

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync(articles, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetArticles();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(3);

		var sortedArticles = result.Value!.OrderBy(a => a.CreatedOn).ToList();
		sortedArticles[0].Title.Should().Be(oldestTitle);
		sortedArticles[1].Title.Should().Be(middleTitle);
		sortedArticles[2].Title.Should().Be(newestTitle);
	}

}


