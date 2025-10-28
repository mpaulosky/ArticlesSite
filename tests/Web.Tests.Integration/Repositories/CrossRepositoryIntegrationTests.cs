// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CrossRepositoryIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Shared.Validators;

namespace Web.Tests.Integration.Repositories;

/// <summary>
///   Integration tests that verify interactions between Article and Category repositories
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class CrossRepositoryIntegrationTests
{

	private readonly MongoDbFixture _fixture;
	private readonly IArticleRepository _articleRepository;
	private readonly ICategoryRepository _categoryRepository;

	public CrossRepositoryIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_articleRepository = new ArticleRepository(_fixture.ContextFactory);
		_categoryRepository = new CategoryRepository(_fixture.ContextFactory);
	}

	[Fact]
	public async Task CreateCategoryAndArticle_BothPersistCorrectly()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		// Act - Create category first
		var categoryResult = await _categoryRepository.AddCategory(category);

		// Create an article with that category
		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Category = categoryResult.Value;

		var articleResult = await _articleRepository.AddArticle(article);

		// Assert
		categoryResult.Success.Should().BeTrue();
		categoryResult.Value.Should().NotBeNull();
		articleResult.Success.Should().BeTrue();
		articleResult.Value.Should().NotBeNull();

		// Verify category exists
		var fetchedCategory = await _categoryRepository.GetCategoryByIdAsync(categoryResult.Value!.Id);
		fetchedCategory.Success.Should().BeTrue();
		fetchedCategory.Value.Should().NotBeNull();
		fetchedCategory.Value!.CategoryName.Should().Be(category.CategoryName);

		// Verify the article exists and has the correct category
		var fetchedArticle = await _articleRepository.GetArticleByIdAsync(articleResult.Value!.Id);
		fetchedArticle.Success.Should().BeTrue();
		fetchedArticle.Value.Should().NotBeNull();
		fetchedArticle.Value!.Category.Should().NotBeNull();
		fetchedArticle.Value.Category.Id.Should().Be(categoryResult.Value.Id);
		fetchedArticle.Value.Title.Should().Be(article.Title);
	}

	[Fact]
	public async Task ArchiveCategory_ArticlesStillAccessible()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var categoryResult = await _categoryRepository.AddCategory(category);
		categoryResult.Success.Should().BeTrue();
		categoryResult.Value.Should().NotBeNull();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Category = categoryResult.Value;

		var articleResult = await _articleRepository.AddArticle(article);
		articleResult.Success.Should().BeTrue();
		articleResult.Value.Should().NotBeNull();

		// Act - Archive the category
		await _categoryRepository.ArchiveCategory(category.Slug);

		// Assert - Category is archived
		var fetchedCategory = await _categoryRepository.GetCategory(category.Slug);
		fetchedCategory.Value.Should().BeNull("archived categories should not be returned");

		// But the article is still accessible by ID
		var fetchedArticle = await _articleRepository.GetArticleByIdAsync(articleResult.Value!.Id);
		fetchedArticle.Success.Should().BeTrue();
		fetchedArticle.Value.Should().NotBeNull();
		fetchedArticle.Value!.Title.Should().Be(article.Title);
	}

	[Fact]
	public async Task MultipleArticles_SameCategory_AllLinkedCorrectly()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var categoryResult = await _categoryRepository.AddCategory(category);
		categoryResult.Success.Should().BeTrue();
		categoryResult.Value.Should().NotBeNull();

		var articles = FakeArticle.GetArticles(3, useSeed: true);

		// Link all articles to the same category
		foreach (var article in articles)
		{
			article.Category = categoryResult.Value;
		}

		// Act - Add all articles
		var tasks = articles.Select(a => _articleRepository.AddArticle(a)).ToArray();
		var results = await Task.WhenAll(tasks);

		// Assert - All articles created successfully
		results.Should().HaveCount(3);
		results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

		// Get all articles and verify they're linked to the same category
		var allArticles = await _articleRepository.GetArticles(a => a.Category!.Id == categoryResult.Value.Id);
		allArticles.Success.Should().BeTrue();
		allArticles.Value.Should().NotBeNull().And.HaveCount(3);
		allArticles.Value.Should().AllSatisfy(a => a.Category!.Id.Should().Be(categoryResult.Value.Id));
	}

	[Fact]
	public async Task UpdateCategoryDetails_ArticlesCategoryIdUnchanged()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var categoryResult = await _categoryRepository.AddCategory(category);
		categoryResult.Success.Should().BeTrue();
		categoryResult.Value.Should().NotBeNull();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Category = categoryResult.Value;

		await _articleRepository.AddArticle(article);

		// Act - Update category details
		category.Update("Updated Name", "updated-slug", false);
		await _categoryRepository.UpdateCategory(category);
		await _articleRepository.UpdateArticle(article);
		// Assert - Article still points to the same category object
		var fetchedArticle = await _articleRepository.GetArticles(a => a.Category!.Id == categoryResult.Value.Id);
		fetchedArticle.Success.Should().BeTrue();
		fetchedArticle.Value.Should().NotBeNull().And.HaveCount(1);
		fetchedArticle.Value!.First().Category!.Id.Should().Be(categoryResult.Value.Id);
	}

	[Fact]
	public async Task ComplexScenario_MultipleCategoriesAndArticles()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Create multiple categories
		var categories = FakeCategory.GetCategories(3, useSeed: true);

		var categoryTasks = categories.Select(c => _categoryRepository.AddCategory(c)).ToArray();
		var categoryResults = await Task.WhenAll(categoryTasks);

		// Create articles for each category
		var articles = new List<Article>();
		foreach (var categoryResult in categoryResults)
		{
			categoryResult.Success.Should().BeTrue();
			categoryResult.Value.Should().NotBeNull();

			var article = FakeArticle.GetNewArticle(useSeed: true);
			article.Category = categoryResult.Value;
			articles.Add(article);
		}

		// Act
		var articleTasks = articles.Select(a => _articleRepository.AddArticle(a)).ToArray();
		var articleResults = await Task.WhenAll(articleTasks);

		// Assert
		// All categories created
		var allCategories = await _categoryRepository.GetCategories();
		allCategories.Value.Should().HaveCount(3);

		// All articles created
		var allArticles = await _articleRepository.GetArticles();
		allArticles.Value.Should().HaveCount(3);

		// Each article linked to the correct category
		foreach (var categoryResult in categoryResults)
		{
			// Already verified categoryResult.Value in the previous loop
			var categoryArticles = await _articleRepository.GetArticles(a => a.Category!.Id == categoryResult.Value!.Id);
			categoryArticles.Value.Should().HaveCount(1);
		}
	}

	[Fact]
	public async Task CreateArticle_WithoutCategory_ShouldSucceedButFailValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.Category = null;

		// Act - Repository will insert the article (MongoDB doesn't enforce validation)
		var result = await _articleRepository.AddArticle(article);

		// Assert - Repository succeeds (validation is enforced at application layer, not repository layer)
		result.Success.Should().BeTrue();

		// However, the ArticleValidator would reject this article
		var validator = new ArticleValidator();
		var validationResult = await validator.ValidateAsync(article, TestContext.Current.CancellationToken);
		validationResult.IsValid.Should().BeFalse();
		validationResult.Errors.Should().Contain(e => e.ErrorMessage == "Category is required");
	}

}