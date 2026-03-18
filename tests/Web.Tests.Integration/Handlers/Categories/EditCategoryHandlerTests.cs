// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Categories;

/// <summary>
///   Integration tests for EditCategory handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class EditCategoryHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly ICategoryRepository _repository;

	private readonly EditCategory.IEditCategoryHandler _handler;

	public EditCategoryHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
		ILogger<EditCategory.Handler> logger = Substitute.For<ILogger<EditCategory.Handler>>();
		CategoryDtoValidator categoryDtoValidator = Substitute.For<CategoryDtoValidator>();
		_handler = new EditCategory.Handler(_repository, logger, categoryDtoValidator);
	}

	[Fact]
	public async Task HandleAsync_WithValidDto_UpdatesCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Updated Category Name",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			ModifiedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CategoryName.Should().Be("Updated Category Name");
		result.Value.ModifiedOn.Should().NotBeNull();

		// Verify it was actually updated in a database
		var saved = await _repository.GetCategoryByIdAsync(category.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.CategoryName.Should().Be("Updated Category Name");
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
		result.Error.Should().Be("Category data cannot be null");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyCategoryName_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category name is required");
	}

	[Fact]
	public async Task HandleAsync_WithNonExistentCategory_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var nonExistentId = ObjectId.GenerateNewId();

		var dto = new CategoryDto
		{
			Id = nonExistentId,
			CategoryName = "Test Category",
			Slug = "test-slug",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Contain("not found");
	}

	[Fact]
	public async Task HandleAsync_RegeneratesSlugFromNewName()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.Slug = "old-slug";
		category.CategoryName = "Old Name";

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "New Category Name",
			Slug = "old-slug", // Old slug should be regenerated
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().NotBe("old-slug");
		result.Value.Slug.Should().Be("new_category_name");
	}

	[Fact]
	public async Task HandleAsync_ArchivingCategory_UpdatesIsArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.IsArchived = false;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = category.CategoryName,
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			ModifiedOn = DateTimeOffset.UtcNow,
			IsArchived = true
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeTrue();

		// Verify in database
		var saved = await _repository.GetCategoryByIdAsync(category.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_UnarchivingCategory_UpdatesIsArchived()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.IsArchived = true;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = category.CategoryName,
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_SetsModifiedOnTimestamp()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var originalModifiedOn = category.ModifiedOn;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Updated Name",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.ModifiedOn.Should().NotBeNull();

		if (originalModifiedOn.HasValue)
		{
			result.Value.ModifiedOn.Should().BeAfter(originalModifiedOn.Value);
		}
	}

	[Fact]
	public async Task HandleAsync_PreservesCreatedOn()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var originalCreatedOn = DateTimeOffset.UtcNow.AddDays(-10);
		category.CreatedOn = originalCreatedOn;

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Updated Name",
			Slug = category.Slug,
			CreatedOn = originalCreatedOn,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CreatedOn.Should().BeCloseTo(originalCreatedOn, TimeSpan.FromSeconds(1));
	}

	[Fact]
	public async Task HandleAsync_WithWhitespaceCategoryName_ReturnsFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "   ",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category name is required");
	}

	[Fact]
	public async Task HandleAsync_WithSpecialCharactersInName_GeneratesValidSlug()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var updatedDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Technology & Science!",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(updatedDto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().NotBeNullOrWhiteSpace();
		result.Value.Slug.Should().MatchRegex("^[a-z0-9_]+$");
	}

}
