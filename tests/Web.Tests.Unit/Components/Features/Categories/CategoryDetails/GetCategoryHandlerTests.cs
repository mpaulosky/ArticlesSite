// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Tests.Unit.Components.Features.Categories.CategoryDetails;

/// <summary>
///   Unit tests for GetCategory.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class GetCategoryHandlerTests
{

	private readonly ICategoryRepository _mockRepository;

	private readonly ILogger<GetCategory.Handler> _mockLogger;

	private readonly GetCategory.Handler _handler;

	public GetCategoryHandlerTests()
	{
		_mockRepository = Substitute.For<ICategoryRepository>();
		_mockLogger = Substitute.For<ILogger<GetCategory.Handler>>();
		_handler = new GetCategory.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidId_ShouldReturnSuccess()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();

		var category = new Category
		{
			Id = objectId,
			CategoryName = "Test Category",
			Slug = "test-category",
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result.Ok(category)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Id.Should().Be(objectId);
		result.Value.CategoryName.Should().Be("Test Category");
	}

	[Fact]
	public async Task HandleAsync_WithNullId_ShouldReturnFailure()
	{
		string? id = null;
		// Act
		var result = await _handler.HandleAsync(id!);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category identifier cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(string.Empty);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category identifier cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WithWhitespaceId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync("   ");

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category identifier cannot be empty");
	}

	[Fact]
	public async Task HandleAsync_WhenCategoryNotFound_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();

		_mockRepository.GetCategoryByIdAsync(objectId)
				.Returns(Task.FromResult(Result.Fail<Category>("Category not found")));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category not found");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var failResult = Result.Fail<Category>("Database error");
		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(failResult));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllCategoryProperties()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var category = new Category
		{
			Id = objectId,
			CategoryName = "Test Category",
			Slug = "test-category",
			CreatedOn = createdOn,
			ModifiedOn = modifiedOn,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result.Ok(category)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		var dto = result.Value;
		dto.Id.Should().Be(objectId);
		dto.CategoryName.Should().Be("Test Category");
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
	}

}