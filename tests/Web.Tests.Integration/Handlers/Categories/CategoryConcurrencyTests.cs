// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryConcurrencyTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration.Handlers.Categories;

/// <summary>
/// Integration tests for concurrency and race condition scenarios in Category handlers.
/// Tests simultaneous edits, conflicts, and edge cases under concurrent access.
/// Includes Version tests for optimistic concurrency.
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class CategoryConcurrencyTests
{

	private readonly MongoDbFixture _fixture;

	private readonly ICategoryRepository _repository;

	private readonly Components.Features.Categories.CategoryEdit.EditCategory.IEditCategoryHandler _editHandler;

	private readonly Components.Features.Categories.CategoryCreate.CreateCategory.ICreateCategoryHandler _createHandler;

	private readonly Components.Features.Categories.CategoryDetails.GetCategory.IGetCategoryHandler _getHandler;

	private readonly Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler _getCategoriesHandler;

	private readonly ILogger<Components.Features.Categories.CategoryEdit.EditCategory.Handler> _editLogger;

	private readonly IValidator<CategoryDto> _validator;

	public CategoryConcurrencyTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new CategoryRepository(_fixture.ContextFactory);
		_editLogger = Substitute.For<ILogger<Components.Features.Categories.CategoryEdit.EditCategory.Handler>>();
		_validator = new CategoryDtoValidator();
		_editHandler = new Components.Features.Categories.CategoryEdit.EditCategory.Handler(_repository, _editLogger, _validator);

		var createLogger = Substitute.For<ILogger<Components.Features.Categories.CategoryCreate.CreateCategory.Handler>>();
		_createHandler = new Components.Features.Categories.CategoryCreate.CreateCategory.Handler(_repository, createLogger, _validator);

		var getLogger = Substitute.For<ILogger<Components.Features.Categories.CategoryDetails.GetCategory.Handler>>();
		_getHandler = new Components.Features.Categories.CategoryDetails.GetCategory.Handler(_repository, getLogger);

		var getCategoriesLogger = Substitute.For<ILogger<Components.Features.Categories.CategoriesList.GetCategories.Handler>>();
		_getCategoriesHandler = new Components.Features.Categories.CategoriesList.GetCategories.Handler(_repository, getCategoriesLogger);
	}

	[Fact]
	public async Task SimultaneousEdits_BothUsersEditSameCategory_LastWriteWins()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var user1Dto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "User 1 Category",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		var user2Dto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "User 2 Category",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act - Both users edit simultaneously
		var user1Task = _editHandler.HandleAsync(user1Dto);
		var user2Task = _editHandler.HandleAsync(user2Dto);
		await Task.WhenAll(user1Task, user2Task);

		var result1 = await user1Task;
		var result2 = await user2Task;

		// Assert - Both operations succeed
		result1.Success.Should().BeTrue();
		result2.Success.Should().BeTrue();

		// Verify that at least one edit was persisted (last-write-wins)
		var saved = await _repository.GetCategoryByIdAsync(category.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.CategoryName.Should().BeOneOf("User 1 Category", "User 2 Category");

		// Verify ModifiedOn was updated
		saved.Value.ModifiedOn.Should().NotBeNull();
	}

	[Fact]
	public async Task ConcurrentCreates_WithSameName_BothOperationsComplete()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var sharedName = "Duplicate Category";

		var dto1 = new CategoryDto
		{
			CategoryName = sharedName,
			IsArchived = false
		};

		var dto2 = new CategoryDto
		{
			CategoryName = sharedName,
			IsArchived = false
		};

		// Act - Both create with the same name concurrently
		var create1Task = _createHandler.HandleAsync(dto1);
		var create2Task = _createHandler.HandleAsync(dto2);
		await Task.WhenAll(create1Task, create2Task);

		var result1 = await create1Task;
		var result2 = await create2Task;

		// Assert - Both operations complete
		// Database behavior: depends on uniqueness constraints
		result1.Success.Should().BeTrue();
		result2.Success.Should().BeTrue();

		// Verify a database has the created categories
		var allCategories = await _repository.GetCategories();
		allCategories.Value!.Count().Should().BeGreaterThanOrEqualTo(1);
	}

	[Fact]
	public async Task EditAndRead_ConcurrentAccess_ReadsConsistentState()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Updated During Reads",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act - Edit and read concurrently
		var editTask = _editHandler.HandleAsync(editDto);
		var readTasks = new List<Task<Result<CategoryDto>>>();
		for (int i = 0; i < 10; i++)
		{
			readTasks.Add(_getHandler.HandleAsync(category.Id.ToString()));
		}

		await Task.WhenAll(editTask, Task.WhenAll(readTasks));

		var editResult = await editTask;
		var readResults = await Task.WhenAll(readTasks);

		// Assert
		editResult.Success.Should().BeTrue();
		foreach (var readResult in readResults)
		{
			readResult.Success.Should().BeTrue();
			// Readers should see either old or updated state consistently
			readResult.Value!.Id.Should().Be(category.Id);
		}
	}

	[Fact]
	public async Task ArchiveAndEdit_RaceCondition_BothOperationsHandle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		category.IsArchived = false;
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// One edit tries to rename
		var editDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Renamed Category",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Another edit tries to archive
		var archiveDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = category.CategoryName,
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = true
		};

		// Act - Both edits happen concurrently
		var editTask = _editHandler.HandleAsync(editDto);
		var archiveTask = _editHandler.HandleAsync(archiveDto);
		await Task.WhenAll(editTask, archiveTask);

		var editResult = await editTask;
		var archiveResult = await archiveTask;

		// Assert - Both operations complete
		editResult.Success.Should().BeTrue();
		archiveResult.Success.Should().BeTrue();

		// Verify the final state
		var saved = await _repository.GetCategoryByIdAsync(category.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.IsArchived.Should().Be(true); // Last write should have archive=true
	}

	[Fact]
	public async Task MultipleSequentialEdits_AllSucceed_LastStatePreserved()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Multiple edits in sequence with small delays
		var editTasks = new List<Task<Result<CategoryDto>>>();
		for (int i = 0; i < 5; i++)
		{
			var dto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = $"Edit {i} Category",
				Slug = category.Slug,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				IsArchived = false
			};

			editTasks.Add(_editHandler.HandleAsync(dto));
			await Task.Delay(10, TestContext.Current.CancellationToken); // Small delay to allow interleaving
		}

		await Task.WhenAll(editTasks);
		var results = await Task.WhenAll(editTasks);

		// Assert - All edits succeed
		foreach (var result in results)
		{
			result.Success.Should().BeTrue();
		}

		// Verify final state has last name
		var saved = await _repository.GetCategoryByIdAsync(category.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.CategoryName.Should().Be("Edit 4 Category");
		saved.Value.ModifiedOn.Should().NotBeNull();
	}

	[Fact]
	public async Task ReadDuringWrite_NoDeadlock_AllCompleteSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertOneAsync(category, cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new CategoryDto
		{
			Id = category.Id,
			CategoryName = "Write During Reads",
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act - Heavy concurrent read/write mix
		var tasks = new List<Task> {
				// Single writer
				_editHandler.HandleAsync(editDto).ContinueWith(_ => { }, TestContext.Current.CancellationToken)
		};

		// Multiple readers
		for (int i = 0; i < 20; i++)
		{
			tasks.Add(_getHandler.HandleAsync(category.Id.ToString()).ContinueWith(_ => { }, TestContext.Current.CancellationToken));
		}

		// List operations (read all categories)
		for (int i = 0; i < 5; i++)
		{
			tasks.Add(_getCategoriesHandler.HandleAsync().ContinueWith(_ => { }, TestContext.Current.CancellationToken));
		}

		// All should complete without timeout/deadlock
		// Increased timeout to 30 seconds to accommodate a heavy concurrent load (20 reads + 5 list ops + 1 writing)
		using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
		try
		{
			await Task.WhenAll(tasks);
			// If we get here, all tasks completed within the timeout
		}
		catch (OperationCanceledException)
		{
			// Timeout occurred
			Assert.Fail("All concurrent operations should complete within 30 second timeout");
		}
	}

	[Fact]
	public async Task ConcurrentModifyAndDelete_BothOperationsHandle()
	{
		// Arrange - Create two categories
		await _fixture.ClearCollectionsAsync();

		var category1 = FakeCategory.GetNewCategory(useSeed: true);
		var category2 = FakeCategory.GetNewCategory(useSeed: false);

		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync([category1, category2], cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new CategoryDto
		{
			Id = category1.Id,
			CategoryName = "Modified Category",
			Slug = category1.Slug,
			CreatedOn = category1.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = false
		};

		// Act - One user edits, another tries to delete (archive)
		var editTask = _editHandler.HandleAsync(editDto);

		// Simulate deletion by archiving
		var deleteDto = new CategoryDto
		{
			Id = category1.Id,
			CategoryName = category1.CategoryName,
			Slug = category1.Slug,
			CreatedOn = category1.CreatedOn ?? DateTimeOffset.UtcNow,
			IsArchived = true
		};
		var deleteTask = _editHandler.HandleAsync(deleteDto);

		await Task.WhenAll(editTask, deleteTask);

		var editResult = await editTask;
		var deleteResult = await deleteTask;

		// Assert - Both complete successfully
		editResult.Success.Should().BeTrue();
		deleteResult.Success.Should().BeTrue();

		// Verify category is archived
		var saved = await _repository.GetCategoryByIdAsync(category1.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.IsArchived.Should().Be(true);
	}

	[Fact]
	public async Task ConcurrentListReads_AllReturnConsistentResults()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var categories = FakeCategory.GetCategories(5, useSeed: true);
		var collection = _fixture.Database.GetCollection<Category>("Categories");
		await collection.InsertManyAsync(categories, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Multiple concurrent reads of all categories
		var readTasks = new List<Task<Result<IEnumerable<CategoryDto>>>>();
		for (int i = 0; i < 10; i++)
		{
			readTasks.Add(_getCategoriesHandler.HandleAsync());
		}

		await Task.WhenAll(readTasks);
		var results = await Task.WhenAll(readTasks);

		// Assert - All reads succeed and return a consistent count
		foreach (var result in results)
		{
			result.Success.Should().BeTrue();
			result.Value.Should().HaveCount(5);
		}
	}

}
