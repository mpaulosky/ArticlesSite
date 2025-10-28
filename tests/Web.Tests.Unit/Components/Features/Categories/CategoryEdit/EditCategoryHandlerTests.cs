// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using NSubstitute;

using Shared.Abstractions;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;

using Web.Components.Features.Categories.CategoryEdit;

namespace Web.Tests.Unit.Components.Features.Categories.CategoryEdit;

/// <summary>
///   Unit tests for EditCategory.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class EditCategoryHandlerTests
{

	private readonly ICategoryRepository _mockRepository;

	private readonly ILogger<EditCategory.Handler> _mockLogger;

	private readonly EditCategory.Handler _handler;

	public EditCategoryHandlerTests()
	{
		_mockRepository = Substitute.For<ICategoryRepository>();
		_mockLogger = Substitute.For<ILogger<EditCategory.Handler>>();
		_handler = new EditCategory.Handler(_mockRepository, _mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithValidRequest_ShouldReturnSuccess()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var existingCategory = new Category
		{
			Id = objectId,
			CategoryName = "Old Category"
		};

		var categoryDto = new CategoryDto
		{
			Id = objectId,
			CategoryName = "Updated Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Ok(existingCategory)));
		_mockRepository.UpdateCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Ok(new Category())));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).GetCategoryByIdAsync(objectId);
		await _mockRepository.Received(1).UpdateCategory(Arg.Is<Category>(c =>
			c.CategoryName == "Updated Category" &&
			c.ModifiedOn != null
		));
	}

	[Fact]
	public async Task HandleAsync_WithNullRequest_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(null);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The request is null.");
		await _mockRepository.DidNotReceive().GetCategoryByIdAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task HandleAsync_WithEmptyCategoryName_ShouldReturnFailure()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category name cannot be empty or whitespace.");
		await _mockRepository.DidNotReceive().GetCategoryByIdAsync(Arg.Any<ObjectId>());
	}

	[Fact]
	public async Task HandleAsync_WithWhitespaceCategoryName_ShouldReturnFailure()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "   ",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category name cannot be empty or whitespace.");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyObjectId_ShouldReturnFailure()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Empty,
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WhenCategoryNotFound_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var categoryDto = new CategoryDto
		{
			Id = objectId,
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Fail("Category not found")));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Category not found");
		await _mockRepository.DidNotReceive().UpdateCategory(Arg.Any<Category>());
	}

	[Fact]
	public async Task HandleAsync_WhenUpdateFails_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var existingCategory = new Category { Id = objectId, CategoryName = "Old Category" };
		var categoryDto = new CategoryDto
		{
			Id = objectId,
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Ok(existingCategory)));
		_mockRepository.UpdateCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Fail("Update failed")));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Update failed");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var categoryDto = new CategoryDto
		{
			Id = objectId,
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns<Task<Result<Category?>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Connection failed");
	}

	[Fact]
	public async Task HandleAsync_ShouldUpdateCategoryNameAndModifiedOn()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		var existingCategory = new Category
		{
			Id = objectId,
			CategoryName = "Old Category",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-10)
		};

		var categoryDto = new CategoryDto
		{
			Id = objectId,
			CategoryName = "Brand New Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Ok(existingCategory)));
		_mockRepository.UpdateCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Ok(new Category())));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).UpdateCategory(Arg.Is<Category>(c =>
			c.CategoryName == "Brand New Category" &&
			c.ModifiedOn != null
		));
	}

}
