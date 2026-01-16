// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoriesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Features.Categories.CategoriesList;

namespace Web.Tests.Unit.Components.Features.Categories.CategoriesList;

/// <summary>
///   Unit tests for GetCategories.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCategoriesHandlerTests
{

	private readonly ICategoryRepository _mockRepository;

	private readonly ILogger<GetCategories.Handler> _mockLogger;

	private readonly GetCategories.Handler _handler;

	public GetCategoriesHandlerTests()
	{
		_mockRepository = Substitute.For<ICategoryRepository>();
		_mockLogger = Substitute.For<ILogger<GetCategories.Handler>>();
		_handler = new GetCategories.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithCategoriesAvailable_ShouldReturnSuccess()
	{
		// Arrange
		var categories = new List<Category>
		{
				new() { CategoryName = "Tech", Slug = "tech", IsArchived = false },
				new() { CategoryName = "Business", Slug = "business", IsArchived = false }
		};

		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(categories)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCount(2);

		var firstDto = result.Value.First();
		firstDto.CategoryName.Should().Be("Tech");
		firstDto.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_WithEmptyList_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Fail<IEnumerable<Category>>("No categories found")));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("No categories found");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Fail<IEnumerable<Category>>("Database error")));

		// Act
		var result = await _handler.HandleAsync(includeArchived: false);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllCategoryProperties()
	{
		// Arrange
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var category = new Category
		{
			CategoryName = "Test Category",
			Slug = "test-category",
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		_mockRepository.GetCategories()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(new List<Category> { category })));

		// Act
		var result = await _handler.HandleAsync(includeArchived: false);

		// Assert
		result.Success.Should().BeTrue();
		var dto = result.Value!.First();
		dto.CategoryName.Should().Be("Test Category");
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
	}

	[Fact]
	public async Task HandleAsync_WithMultipleCategories_ShouldReturnAllInCorrectOrder()
	{
		// Arrange
		var categories = new List<Category>
		{
				new() { CategoryName = "First", Slug = "first" },
				new() { CategoryName = "Second", Slug = "second" },
				new() { CategoryName = "Third", Slug = "third" }
		};

		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(categories)));

		// Act
		var result = await _handler.HandleAsync(includeArchived: false);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		var dtos = result.Value.ToList();
		dtos[0].CategoryName.Should().Be("First");
		dtos[1].CategoryName.Should().Be("Second");
		dtos[2].CategoryName.Should().Be("Third");
	}

	[Fact]
	public async Task HandleAsync_WithIncludeArchivedFalse_ShouldExcludeArchivedCategories()
	{
		// Arrange
		var categories = new List<Category>
		{
				new() { CategoryName = "Active1", Slug = "active1", IsArchived = false },
				new() { CategoryName = "Archived1", Slug = "archived1", IsArchived = true },
				new() { CategoryName = "Active2", Slug = "active2", IsArchived = false },
				new() { CategoryName = "Archived2", Slug = "archived2", IsArchived = true }
		};

		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(categories)));

		// Act
		var result = await _handler.HandleAsync(includeArchived: false);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(2);
		result.Value.Should().OnlyContain(c => !c.IsArchived);
		result.Value.Should().Contain(c => c.CategoryName == "Active1");
		result.Value.Should().Contain(c => c.CategoryName == "Active2");
	}

	[Fact]
	public async Task HandleAsync_WithIncludeArchivedTrue_ShouldIncludeAllCategories()
	{
		// Arrange
		var categories = new List<Category>
		{
				new() { CategoryName = "Active1", Slug = "active1", IsArchived = false },
				new() { CategoryName = "Archived1", Slug = "archived1", IsArchived = true },
				new() { CategoryName = "Active2", Slug = "active2", IsArchived = false }
		};

		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(categories)));

		// Act
		var result = await _handler.HandleAsync(includeArchived: true);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(3);
		result.Value.Should().Contain(c => c.IsArchived);
		result.Value.Should().Contain(c => c.CategoryName == "Active1");
		result.Value.Should().Contain(c => c.CategoryName == "Archived1");
		result.Value.Should().Contain(c => c.CategoryName == "Active2");
	}

	[Fact]
	public async Task HandleAsync_DefaultParameter_ShouldExcludeArchivedCategories()
	{
		// Arrange
		var categories = new List<Category>
		{
				new() { CategoryName = "Active", Slug = "active", IsArchived = false },
				new() { CategoryName = "Archived", Slug = "archived", IsArchived = true }
		};

		_mockRepository.GetCategories().Returns(Task.FromResult(Result.Ok<IEnumerable<Category>>(categories)));

		// Act
		var result = await _handler.HandleAsync(); // Default parameter should be false

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().HaveCount(1);
		result.Value.Should().OnlyContain(c => !c.IsArchived);
		result.Value.First().CategoryName.Should().Be("Active");
	}

}