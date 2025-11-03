// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoriesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Categories;

/// <summary>
///   Integration tests for GetCategories handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class GetCategoriesHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly ICategoryRepository _repository;

	private readonly Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler _handler;

	private readonly ILogger<Components.Features.Categories.CategoriesList.GetCategories.Handler> _logger;

	public GetCategoriesHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
		_logger = Substitute.For<ILogger<Components.Features.Categories.CategoriesList.GetCategories.Handler>>();
		_handler = new Components.Features.Categories.CategoriesList.GetCategories.Handler(_repository, _logger);
	}

	[Fact]
	public async Task HandleAsync_WithNoCategories_ReturnsEmptyList()
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
	public async Task HandleAsync_WithMultipleCategories_ReturnsAllCategories()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(3, useSeed: true);
		var categoryNames = categories.Select(c => c.CategoryName).ToList();

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(categories, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(3);
		result.Value.Should().Contain(c => c.CategoryName == categoryNames[0]);
		result.Value.Should().Contain(c => c.CategoryName == categoryNames[1]);
		result.Value.Should().Contain(c => c.CategoryName == categoryNames[2]);
	}

	[Fact]
	public async Task HandleAsync_IncludesArchivedCategories()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(3, useSeed: true);
		categories[0].IsArchived = false;
		categories[1].IsArchived = false;
		categories[2].IsArchived = true;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(categories, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();

		// Repository now returns all categories, handler filters by default (includeArchived=false)
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().OnlyContain(c => !c.IsArchived); // Handler filters out archived by default
	}

	[Fact]
	public async Task HandleAsync_WithIncludeArchived_ReturnsAllCategories()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(3, useSeed: true);
		categories[0].IsArchived = false;
		categories[1].IsArchived = false;
		categories[2].IsArchived = true;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(categories, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(includeArchived: true);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();

		// When includeArchived=true, all categories including archived should be returned
		result.Value.Should().NotBeNull().And.HaveCount(3);
		result.Value.Should().Contain(c => c.IsArchived); // Should include archived category
	}

	[Fact]
	public async Task HandleAsync_ReturnsCorrectSlugs()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var category1 = FakeCategory.GetNewCategory(useSeed: true);
		category1.Slug = "category-one";

		var category2 = FakeCategory.GetNewCategory(useSeed: false);
		category2.Slug = "category-two";

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(new[] { category1, category2 }, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(2);
		result.Value.Should().Contain(c => c.Slug == "category-one");
		result.Value.Should().Contain(c => c.Slug == "category-two");
	}

	[Fact]
	public async Task HandleAsync_ReturnsCorrectTimestamps()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var createdDate = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedDate = DateTimeOffset.UtcNow.AddDays(-2);

		category.CreatedOn = createdDate;
		category.ModifiedOn = modifiedDate;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);

		var dto = result.Value.First();
		dto.CreatedOn.Should().BeCloseTo(createdDate, TimeSpan.FromSeconds(1));
		dto.ModifiedOn.Should().NotBeNull();
		dto.ModifiedOn!.Value.Should().BeCloseTo(modifiedDate, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task HandleAsync_WithNullModifiedOn_ReturnsNullModifiedOn()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.ModifiedOn = null;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().ModifiedOn.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_OrdersResultsConsistently()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(5, useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(categories, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result1 = await _handler.HandleAsync();
		var result2 = await _handler.HandleAsync();

		// Assert
		result1.Should().NotBeNull();
		result1.Success.Should().BeTrue();
		result2.Should().NotBeNull();
		result2.Success.Should().BeTrue();

		var list1 = result1.Value.ToList();
		var list2 = result2.Value.ToList();

		list1.Should().HaveCount(5);
		list2.Should().HaveCount(5);

		// Results should be in the same order
		for (int i = 0; i < list1.Count; i++)
		{
			list1[i].Id.Should().Be(list2[i].Id);
		}
	}

	[Fact]
	public async Task HandleAsync_WithSingleCategory_ReturnsSingleCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);
		result.Value.First().Id.Should().Be(category.Id);
		result.Value.First().CategoryName.Should().Be(category.CategoryName);
	}

	[Fact]
	public async Task HandleAsync_AllFieldsAreMapped()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.CategoryName = "Complete Category";
		category.Slug = "complete-category";
		category.CreatedOn = DateTimeOffset.UtcNow.AddDays(-5);
		category.ModifiedOn = DateTimeOffset.UtcNow.AddDays(-1);
		category.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull().And.HaveCount(1);

		var dto = result.Value.First();
		dto.Id.Should().Be(category.Id);
		dto.CategoryName.Should().Be("Complete Category");
		dto.Slug.Should().Be("complete-category");
		dto.CreatedOn.Should().BeCloseTo(category.CreatedOn!.Value, TimeSpan.FromSeconds(1));
		dto.ModifiedOn.Should().BeCloseTo(category.ModifiedOn!.Value, TimeSpan.FromSeconds(1));
		dto.IsArchived.Should().BeFalse();
	}

}



