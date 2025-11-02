// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryRepositoryIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Repositories;

/// <summary>
///   Integration tests for CategoryRepository with real MongoDB container
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class CategoryRepositoryIntegrationTests
{

	private readonly MongoDbFixture _fixture;

	private readonly ICategoryRepository _repository;

	public CategoryRepositoryIntegrationTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
	}

	[Fact]
	public async Task GetCategories_WithNoCategories_ReturnsEmptyList()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _repository.GetCategories();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.BeEmpty();
	}

	[Fact]
	public async Task GetCategories_WithMultipleCategories_ReturnsAllNonArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(2, useSeed: true);

		// Generate archived category without seed to avoid CategoryName collision
		var archivedCategory = FakeCategory.GetNewCategory(useSeed: false);

		// Force a unique name to avoid collision
		archivedCategory = new Category
		{
				CategoryName = "ARCHIVED_" + Guid.NewGuid().ToString(),
				Slug = archivedCategory.Slug,
				IsArchived = false,
				CreatedOn = DateTimeOffset.UtcNow
		};

		archivedCategory.Update(archivedCategory.CategoryName, archivedCategory.Slug, isArchived: true);

		var allCategories = categories.Concat(new[] { archivedCategory }).ToArray();
		var categoryNames = categories.Select(c => c.CategoryName).ToList();

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(allCategories, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetCategories();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);

		// Verify the two non-archived categories are present
		result.Value.Should().Contain(c => categoryNames.Contains(c.CategoryName));

		// Verify archived category is NOT present
		result.Value.Should().NotContain(c => c.CategoryName == archivedCategory.CategoryName);
	}

	[Fact]
	public async Task GetCategoryByIdAsync_WithValidId_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetCategoryByIdAsync(category.Id);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(category.Id);
		result.Value.CategoryName.Should().Be(category.CategoryName);
	}

	[Fact]
	public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _repository.GetCategoryByIdAsync(nonExistentId);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Value.Should().BeNull();
		result.Error.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task GetCategory_WithValidSlug_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetCategory(category.Slug);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().NotBeNull();
		result.Value.CategoryName.Should().Be(category.CategoryName);
		result.Value.Slug.Should().Be(category.Slug);
	}

	[Fact]
	public async Task GetCategory_WithInvalidSlug_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _repository.GetCategory("nonexistent-slug");

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Value.Should().BeNull();
		result.Error.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task GetCategory_WithArchivedCategory_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.Update(category.CategoryName, category.Slug, isArchived: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _repository.GetCategory(category.Slug);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Value.Should().BeNull("archived categories should not be returned");
		result.Error.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public async Task AddCategory_WithValidCategory_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		// Act
		var result = await _repository.AddCategory(category);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().NotBe(ObjectId.Empty);
		result.Value.CategoryName.Should().Be(category.CategoryName);

		// Verify in database
		var collection = _fixture.Database.GetCollection<Category>("Categories");

		var dbCategory = await collection.Find(c => c.Id == result.Value.Id)
				.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

		dbCategory.Should().NotBeNull();
		dbCategory.Should().NotBeNull();
		dbCategory.CategoryName.Should().Be(category.CategoryName);
	}

	[Fact]
	public async Task UpdateCategory_WithExistingCategory_UpdatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Modify category
		category.Update("Updated Name", "updated-slug", false);

		// Act
		var result = await _repository.UpdateCategory(category);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.CategoryName.Should().Be("Updated Name");
		result.Value.Slug.Should().Be("updated-slug");    // Verify in database

		var dbCategory = await collection.Find(c => c.Id == category.Id)
				.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

		dbCategory.Should().NotBeNull();
		dbCategory.Should().NotBeNull();
		dbCategory.CategoryName.Should().Be("Updated Name");
	}

	[Fact]
	public async Task ArchiveCategory_WithExistingCategory_ArchivesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		await _repository.ArchiveCategory(category.Slug);

		// Assert - Verify in database
		var dbCategory = await collection.Find(c => c.Slug == category.Slug)
				.FirstOrDefaultAsync(TestContext.Current.CancellationToken);

		dbCategory.Should().NotBeNull();
		dbCategory.Should().NotBeNull();
		dbCategory.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task GetCategories_WithWhereExpression_ReturnsFilteredCategories()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var allCategories = FakeCategory.GetCategories(3, useSeed: true);

		// Update the first two categories to have names that start with a common prefix
		allCategories[0].Update("Tech Alpha", allCategories[0].Slug, false);
		allCategories[1].Update("Tech Beta", allCategories[1].Slug, false);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(allCategories, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Filter categories that start with "Tech"
		var result = await _repository.GetCategories(c => c.CategoryName.StartsWith("Tech"));

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().AllSatisfy(c => c.CategoryName.Should().StartWith("Tech"));
	}

	[Fact]
	public async Task ConcurrentOperations_HandleMultipleThreadsSafely()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act - Create multiple categories concurrently
		var categories = FakeCategory.GetCategories(10, useSeed: true);
		var tasks = categories.Select(category => _repository.AddCategory(category)).ToArray();

		var results = await Task.WhenAll(tasks);

		// Assert
		results.Should().HaveCount(10);
		results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

		var allCategories = await _repository.GetCategories();
		allCategories.Value.Should().HaveCount(10);
	}

	[Fact]
	public async Task UpdateCategory_WithNonExistentCategory_DoesNotThrow()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		// Act
		var result = await _repository.UpdateCategory(category);

		// Assert - Should succeed even though no document was updated
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
	}

}


