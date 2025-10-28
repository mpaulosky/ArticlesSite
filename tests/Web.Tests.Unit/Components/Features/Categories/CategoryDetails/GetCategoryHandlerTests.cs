// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategoryHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using FluentAssertions;

using Microsoft.Extensions.Logging;

using MongoDB.Bson;

using NSubstitute;

using Shared.Entities;
using Shared.Interfaces;

using Web.Components.Features.Categories.CategoryDetails;

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

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Ok(category)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(objectId);
		result.Value.CategoryName.Should().Be("Test Category");
	}

	[Fact]
	public async Task HandleAsync_WithNullId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(null!);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithEmptyId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync(string.Empty);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithWhitespaceId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync("   ");

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WithInvalidObjectId_ShouldReturnFailure()
	{
		// Act
		var result = await _handler.HandleAsync("invalid-id");

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The ID is invalid or empty.");
	}

	[Fact]
	public async Task HandleAsync_WhenCategoryNotFound_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Fail("Category not found")));

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
		var failResult = Result<Category?>.Fail("Database error");
		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(failResult));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var objectId = ObjectId.GenerateNewId();
		_mockRepository.GetCategoryByIdAsync(objectId).Returns<Task<Result<Category?>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Connection failed");
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

		_mockRepository.GetCategoryByIdAsync(objectId).Returns(Task.FromResult(Result<Category?>.Ok(category)));

		// Act
		var result = await _handler.HandleAsync(objectId.ToString());

		// Assert
		result.Success.Should().BeTrue();
		var dto = result.Value!;

		dto.Id.Should().Be(objectId);
		dto.CategoryName.Should().Be("Test Category");
		dto.CreatedOn.Should().Be(createdOn);
		dto.ModifiedOn.Should().Be(modifiedOn);
		dto.IsArchived.Should().BeFalse();
	}

}
