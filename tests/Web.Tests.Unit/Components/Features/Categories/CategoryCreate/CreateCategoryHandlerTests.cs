// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Features.Categories.CategoryCreate;

/// <summary>
///   Unit tests for CreateCategory.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerTests
{

	private readonly ICategoryRepository _mockRepository;

	private readonly ILogger<CreateCategory.Handler> _mockLogger;

	private readonly IValidator<CategoryDto> _mockValidator;

	private readonly CreateCategory.Handler _handler;

	public CreateCategoryHandlerTests()
	{
		_mockRepository = Substitute.For<ICategoryRepository>();
		_mockLogger = Substitute.For<ILogger<CreateCategory.Handler>>();
		_mockValidator = Substitute.For<IValidator<CategoryDto>>();
		_handler = new CreateCategory.Handler(_mockRepository, _mockLogger, _mockValidator);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		var validationResult = new FluentValidation.Results.ValidationResult();
		_mockValidator.ValidateAsync(categoryDto, Arg.Any<CancellationToken>()).Returns(validationResult);
		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result.Ok(new Category())));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddCategory(Arg.Is<Category>(c =>
			c.CategoryName == "Test Category"
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullRequest_ShouldReturnFailure()
	{
		CategoryDto? dto = null;
		// Act
		var result = await _handler.HandleAsync(dto!);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category data cannot be null");
		await _mockRepository.DidNotReceive().AddCategory(Arg.Any<Category>());
	}

	[Fact]
	public async Task HandleAsync_WithInvalidData_ShouldReturnValidationError()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		var validationFailure = new FluentValidation.Results.ValidationFailure("CategoryName", "Category name is required");
		var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });
		_mockValidator.ValidateAsync(categoryDto, Arg.Any<CancellationToken>()).Returns(validationResult);

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Contain("Category name is required");
		await _mockRepository.DidNotReceive().AddCategory(Arg.Any<Category>());
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryFails_ShouldReturnFailure()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		var validationResult = new FluentValidation.Results.ValidationResult();
		_mockValidator.ValidateAsync(categoryDto, Arg.Any<CancellationToken>()).Returns(validationResult);
		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Fail("Database error")));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapCategoryName()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Technology",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		var validationResult = new FluentValidation.Results.ValidationResult();
		_mockValidator.ValidateAsync(categoryDto, Arg.Any<CancellationToken>()).Returns(validationResult);
		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result.Ok(new Category())));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddCategory(Arg.Is<Category>(c =>
			c.CategoryName == "Technology"
		));
	}

}
