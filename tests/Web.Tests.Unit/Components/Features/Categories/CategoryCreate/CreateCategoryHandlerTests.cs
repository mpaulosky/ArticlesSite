// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategoryHandlerTests.cs
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

using Web.Components.Features.Categories.CategoryCreate;

namespace Web.Tests.Unit.Components.Features.Categories.CategoryCreate;

/// <summary>
///   Unit tests for CreateCategory.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCategoryHandlerTests
{

	private readonly ICategoryRepository _mockRepository;

	private readonly ILogger<CreateCategory.Handler> _mockLogger;

	private readonly CreateCategory.Handler _handler;

	public CreateCategoryHandlerTests()
	{
		_mockRepository = Substitute.For<ICategoryRepository>();
		_mockLogger = Substitute.For<ILogger<CreateCategory.Handler>>();
		_handler = new CreateCategory.Handler(_mockRepository, _mockLogger);
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

		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Ok(new Category())));

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
		// Act
		var result = await _handler.HandleAsync(null);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("The request is null.");
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

		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Fail("Database error")));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.GenerateNewId(),
			CategoryName = "Test Category",
			CreatedOn = DateTimeOffset.UtcNow,
			IsArchived = false
		};

		_mockRepository.AddCategory(Arg.Any<Category>()).Returns<Task<Result<Category>>>(x => throw new InvalidOperationException("Connection failed"));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().StartWith("An error occurred while creating the category:");
		result.Error.Should().Contain("Connection failed");
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

		_mockRepository.AddCategory(Arg.Any<Category>()).Returns(Task.FromResult(Result<Category>.Ok(new Category())));

		// Act
		var result = await _handler.HandleAsync(categoryDto);

		// Assert
		result.Success.Should().BeTrue();
		await _mockRepository.Received(1).AddCategory(Arg.Is<Category>(c =>
			c.CategoryName == "Technology"
		));
	}

}
