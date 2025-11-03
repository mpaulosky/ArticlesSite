// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Categories;

/// <summary>
///   Integration tests for GetCategory handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class GetCategoryHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly ICategoryRepository _repository;

	private readonly Components.Features.Categories.CategoryDetails.GetCategory.IGetCategoryHandler _handler;

	private readonly ILogger<Components.Features.Categories.CategoryDetails.GetCategory.Handler> _logger;

	public GetCategoryHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
		_logger = Substitute.For<ILogger<Components.Features.Categories.CategoryDetails.GetCategory.Handler>>();
		_handler = new Components.Features.Categories.CategoryDetails.GetCategory.Handler(_repository, _logger);
	}

	[Fact]
	public async Task HandleAsync_WithValidId_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(category.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(category.Id);
		result.Value.CategoryName.Should().Be(category.CategoryName);
		result.Value.Slug.Should().Be(category.Slug);
	}

	[Fact]
	public async Task HandleAsync_WithValidSlug_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.Slug = "test-category-slug";

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync("test-category-slug");

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CategoryName.Should().Be(category.CategoryName);
		result.Value.Slug.Should().Be("test-category-slug");
	}

	[Fact]
	public async Task HandleByIdAsync_WithValidObjectId_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleByIdAsync(category.Id);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(category.Id);
		result.Value.CategoryName.Should().Be(category.CategoryName);
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
	public async Task HandleAsync_WithInvalidSlug_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync("non-existent-slug");

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Contain("not found");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyIdentifier_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(string.Empty);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category identifier cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WithNullIdentifier_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		// Act
		var result = await _handler.HandleAsync(null!);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category identifier cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WithArchivedCategory_ReturnsCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(category.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_ReturnsCorrectTimestamps()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.CreatedOn = DateTimeOffset.UtcNow.AddDays(-5);
		category.ModifiedOn = DateTimeOffset.UtcNow.AddDays(-1);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act
		var result = await _handler.HandleAsync(category.Id.ToString());

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CreatedOn.Should().BeCloseTo(category.CreatedOn!.Value, TimeSpan.FromSeconds(1));
		result.Value.ModifiedOn.Should().BeCloseTo(category.ModifiedOn!.Value, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task HandleByIdAsync_WithInvalidObjectId_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var nonExistentId = ObjectId.GenerateNewId();

		// Act
		var result = await _handler.HandleByIdAsync(nonExistentId);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Contain("not found");
	}

}



