// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Categories;

/// <summary>
///   Integration tests for CreateCategory handler
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerTests
{

	private readonly MongoDbFixture _fixture;

	private readonly CreateCategory.ICreateCategoryHandler _handler;

	private readonly CategoryRepository _repository;

	public CreateCategoryHandlerTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
		ILogger<CreateCategory.Handler> logger = Substitute.For<ILogger<CreateCategory.Handler>>();
		CategoryDtoValidator categoryDtoValidator = Substitute.For<CategoryDtoValidator>();

		_handler = new CreateCategory.Handler(_repository, logger, categoryDtoValidator);
	}

	[Fact]
	public async Task HandleAsync_WithValidDto_CreatesCategory()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto = new CategoryDto { CategoryName = "Test Category", IsArchived = false };

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CategoryName.Should().Be("Test Category");
		result.Value.Slug.Should().Be("test_category");
		result.Value.IsArchived.Should().BeFalse();
		result.Value.Id.Should().NotBe(ObjectId.Empty);

		// Verify it was actually saved to a database
		var saved = await _repository.GetCategoryByIdAsync(result.Value.Id);
		saved.Success.Should().BeTrue();
		saved.Value.Should().NotBeNull();
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
	public async Task HandleAsync_WithEmptyCategoryName_ReturnsValidationFailure()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto = new CategoryDto { CategoryName = "", IsArchived = false };

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task HandleAsync_GeneratesSlugFromCategoryName()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto = new CategoryDto { CategoryName = "Technology & Programming", IsArchived = false };

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Slug.Should().NotBeNullOrWhiteSpace();
		result.Value.Slug.Should().MatchRegex("^[a-z0-9_]+$"); // Only lowercase, numbers, and underscores
	}

	[Fact]
	public async Task HandleAsync_SetsCorrectTimestamps()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();
		var now = DateTimeOffset.UtcNow;

		var dto = new CategoryDto { CategoryName = "Test Category", IsArchived = false };

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CreatedOn.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
		result.Value.ModifiedOn.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WithArchivedCategory_CreatesCorrectly()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto = new CategoryDto { CategoryName = "Archived Category", IsArchived = true };

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.IsArchived.Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithMultipleCategories_CreatesAllSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto1 = new CategoryDto { CategoryName = "Category One", IsArchived = false };

		var dto2 = new CategoryDto { CategoryName = "Category Two", IsArchived = false };

		// Act
		var result1 = await _handler.HandleAsync(dto1);
		var result2 = await _handler.HandleAsync(dto2);

		// Assert
		result1.Success.Should().BeTrue();
		result2.Success.Should().BeTrue();
		result1.Value!.Id.Should().NotBe(result2.Value!.Id);

		// Verify both exist in a database
		var allCategories = await _repository.GetCategories();
		allCategories.Success.Should().BeTrue();
		allCategories.Value.Should().HaveCount(2);
	}

	// Note: CategoryDtoValidator does not enforce a minimum length for CategoryName,
	// so short names like "A" or "AB" are considered valid by the validator.
	// If minimum length validation is needed, add a MinimumLength rule to CategoryDtoValidator.

	[Fact]
	public async Task HandleAsync_WithLongCategoryName_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var dto = new CategoryDto
		{
			CategoryName = "This is a very long category name with lots of words",
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CategoryName.Should().Be(dto.CategoryName);
	}

}
