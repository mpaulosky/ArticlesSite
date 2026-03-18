// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Features.Categories.CategoryDetails;

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

	[Fact]
	public async Task HandleAsync_WithSlug_RepositoryFailure_ReturnsFailure()
	{
		// Arrange
		_mockRepository.GetCategory("nope").Returns(Task.FromResult(Result.Fail<Category>("not found")));

		// Act
		var result = await _handler.HandleAsync("nope");

		// Assert
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("not found");
		var logCalls = _mockLogger.ReceivedCalls();
		logCalls.Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments().Any(a => a?.ToString()?.Contains("Category not found with slug") == true)).Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithSlug_RepositoryReturnsNull_ReturnsNotFound()
	{
		// Arrange
		_mockRepository.GetCategory("nopers").Returns(Task.FromResult<Result<Category>>((Result<Category>)(object)Result.Ok((Category?)null)));

		// Act
		var result = await _handler.HandleAsync("nopers");

		// Assert
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category not found");
		var logCalls = _mockLogger.ReceivedCalls();
		logCalls.Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments().Any(a => a?.ToString()?.Contains("Category not found with slug") == true)).Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithSlug_RepositoryReturnsCategory_ReturnsDtoAndLogsInfo()
	{
		// Arrange
		var category = new Category
		{
			CategoryName = "SlugCat",
			Slug = "slug-cat",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-1),
			IsArchived = false
		};

		_mockRepository.GetCategory("slug-cat").Returns(Task.FromResult(Result.Ok(category)));

		// Act
		var result = await _handler.HandleAsync("slug-cat");

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.CategoryName.Should().Be("SlugCat");
		var logCalls = _mockLogger.ReceivedCalls();
		logCalls.Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments().Any(a => a?.ToString()?.Contains("Successfully retrieved category with slug") == true)).Should().BeTrue();
	}

	[Fact]
	public async Task HandleAsync_WithNonObjectId_UsesSlugPath()
	{
		// Arrange
		_mockRepository.GetCategory("my-slug").Returns(Task.FromResult(Result.Ok(new Category { CategoryName = "My" })));

		// Act
		var result = await _handler.HandleAsync("my-slug");

		// Assert
		result.Success.Should().BeTrue();
		var calls = _mockRepository.ReceivedCalls();
		calls.Any(c => c.GetMethodInfo().Name == nameof(ICategoryRepository.GetCategory) && c.GetArguments().OfType<string>().Any(s => s == "my-slug")).Should().BeTrue();
		calls.Any(c => c.GetMethodInfo().Name == nameof(ICategoryRepository.GetCategoryByIdAsync)).Should().BeFalse();
	}

	[Fact]
	public async Task HandleByIdAsync_WhenRepositoryFails_ReturnsFailure()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_mockRepository.GetCategoryByIdAsync(id).Returns(Task.FromResult(Result.Fail<Category>("no id")));

		// Act
		var result = await _handler.HandleByIdAsync(id);

		// Assert
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("no id");
	}

	[Fact]
	public async Task HandleByIdAsync_WhenRepositoryReturnsNull_ReturnsNotFound()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		_mockRepository.GetCategoryByIdAsync(id).Returns(Task.FromResult<Result<Category>>((Result<Category>)(object)Result.Ok((Category?)null)));

		// Act
		var result = await _handler.HandleByIdAsync(id);

		// Assert
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Category not found");
	}

	[Fact]
	public async Task HandleByIdAsync_WithCategory_ReturnsDtoAndLogsInfo()
	{
		// Arrange
		var id = ObjectId.GenerateNewId();
		var category = new Category { Id = id, CategoryName = "ById", Slug = "by-id", IsArchived = true };
		_mockRepository.GetCategoryByIdAsync(id).Returns(Task.FromResult(Result.Ok(category)));

		// Act
		var result = await _handler.HandleByIdAsync(id);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(id);
		result.Value.CategoryName.Should().Be("ById");
		var logCalls = _mockLogger.ReceivedCalls();
		logCalls.Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments().Any(a => a?.ToString()?.Contains("Successfully retrieved category with ID") == true)).Should().BeTrue();
	}
}
