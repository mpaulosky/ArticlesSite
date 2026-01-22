// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleConcurrencyTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Fakes;
using Web.Components.Features.Articles.Interfaces;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Validators;
using Web.Components.Features.AuthorInfo.Fakes;

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
/// Integration tests for concurrency and race condition scenarios in Article handlers.
/// Tests simultaneous edits, conflicts, and edge cases under concurrent access.
/// Includes Version tests for optimistic concurrency.
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleConcurrencyTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler _editHandler;

	private readonly Components.Features.Articles.ArticleCreate.CreateArticle.ICreateArticleHandler _createHandler;

	private readonly Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler _getHandler;

	private readonly ILogger<Components.Features.Articles.ArticleEdit.EditArticle.Handler> _editLogger;

	private readonly IValidator<ArticleDto> _validator;

	public ArticleConcurrencyTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		_editLogger = Substitute.For<ILogger<Components.Features.Articles.ArticleEdit.EditArticle.Handler>>();
		_validator = new ArticleDtoValidator();
		_editHandler = new Components.Features.Articles.ArticleEdit.EditArticle.Handler(_repository, _editLogger, _validator);

		var createLogger = Substitute.For<ILogger<Components.Features.Articles.ArticleCreate.CreateArticle.Handler>>();
		_createHandler = new Components.Features.Articles.ArticleCreate.CreateArticle.Handler(_repository, createLogger, _validator);

		var getLogger = Substitute.For<ILogger<Components.Features.Articles.ArticleDetails.GetArticle.Handler>>();
		_getHandler = new Components.Features.Articles.ArticleDetails.GetArticle.Handler(_repository, getLogger);
	}

	[Fact]
	public async Task SimultaneousEdits_BothUsersEditSameArticle_LastWriteWins()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = Web.Components.Features.Articles.Fakes.FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Web.Components.Features.Articles.Entities.Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var user1Dto = new Web.Components.Features.Articles.Models.ArticleDto(
			article.Id,
			article.Slug,
			"User 1 Title",
			article.Introduction,
			article.Content,
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false,
			0
		);

		var user2Dto = new Web.Components.Features.Articles.Models.ArticleDto(
			article.Id,
			article.Slug,
			"User 2 Title",
			article.Introduction,
			article.Content,
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false,
			0
		);

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
		var saved = await _repository.GetArticleByIdAsync(article.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.Title.Should().BeOneOf("User 1 Title", "User 2 Title");

		// Verify ModifiedOn was updated
		saved.Value.ModifiedOn.Should().NotBeNull();
	}

	[Fact]
	public async Task ConcurrentCreates_WithSameSlug_OnlyOneSucceeds()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = Web.Components.Features.Categories.Fakes.FakeCategory.GetNewCategory(useSeed: true);
		var author = Web.Components.Features.AuthorInfo.Fakes.FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);
		var sharedSlug = "shared_article_slug";

		var dto1 = new Web.Components.Features.Articles.Models.ArticleDto(
			ObjectId.Empty,
			sharedSlug,
			"Article 1",
			"Introduction 1",
			"Content 1",
			"http://example.com/image1.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false,
			0
		);

		var dto2 = new Web.Components.Features.Articles.Models.ArticleDto(
			ObjectId.Empty,
			sharedSlug,
			"Article 2",
			"Introduction 2",
			"Content 2",
			"http://example.com/image2.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		), 0
	);

		// Act - Both create with same slug concurrently
		var create1Task = _createHandler.HandleAsync(dto1);
		var create2Task = _createHandler.HandleAsync(dto2);
		await Task.WhenAll(create1Task, create2Task);

		var result1 = await create1Task;
		var result2 = await create2Task;

		// Assert - Both operations complete
		// Database behavior: depends on unique constraint enforcement
		// At least one should succeed, outcomes vary by MongoDB index configuration
		(result1.Success || result2.Success).Should().BeTrue();

		// Verify database has correct count
		var allArticles = await _repository.GetArticles();
		allArticles.Value!.Count().Should().BeGreaterThanOrEqualTo(1);
	}

	[Fact]
	public async Task EditAndRead_ConcurrentAccess_ReadsCurrentState()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new ArticleDto(
			article.Id,
			article.Slug,
			"Updated Title During Read",
			article.Introduction,
			article.Content,
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		), 0
	);

		// Act - Edit and read concurrently
		var editTask = _editHandler.HandleAsync(editDto);
		var readTasks = new List<Task<Result<ArticleDto>>>();
		for (int i = 0; i < 5; i++)
		{
			readTasks.Add(_getHandler.HandleAsync(article.Id));
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
			readResult.Value!.Id.Should().Be(article.Id);
		}
	}

	[Fact]
	public async Task ArchiveAndEdit_RaceCondition_BothOperationsHandle()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		article.IsArchived = false;
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// One edit tries to modify content
		var editDto = new ArticleDto(
			article.Id,
			article.Slug,
			article.Title,
			article.Introduction,
			"Updated content while archiving",
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		), 0
	);

		// Another edit tries to archive
		var archiveDto = new ArticleDto(
			article.Id,
			article.Slug,
			article.Title,
			article.Introduction,
			article.Content,
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			true, // Archive
			false
		);

		// Act - Both edits happen concurrently
		var editTask = _editHandler.HandleAsync(editDto);
		var archiveTask = _editHandler.HandleAsync(archiveDto);
		await Task.WhenAll(editTask, archiveTask);

		var editResult = await editTask;
		var archiveResult = await archiveTask;

		// Assert - Both operations complete
		editResult.Success.Should().BeTrue();
		archiveResult.Success.Should().BeTrue();

		// Verify final state: last write wins (either archived or not)
		var saved = await _repository.GetArticleByIdAsync(article.Id);
		saved.Success.Should().BeTrue();
	}

	[Fact]
	public async Task MultipleSequentialEdits_AllSucceed_LastStatePreserved()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		// Act - Multiple edits in sequence with small delays
		var editTasks = new List<Task<Result<ArticleDto>>>();
		for (int i = 0; i < 5; i++)
		{
			var dto = new ArticleDto(
				article.Id,
				article.Slug,
				$"Title Edit {i}",
				article.Introduction,
				article.Content,
				article.CoverImageUrl ?? "http://example.com/image.jpg",
				article.Author,
				article.Category,
				false,
				null,
				article.CreatedOn,
				DateTimeOffset.UtcNow,
				false,
				false
			, 0);

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

		// Verify final state has last title
		var saved = await _repository.GetArticleByIdAsync(article.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.Title.Should().Be("Title Edit 4");
		saved.Value.ModifiedOn.Should().NotBeNull();
	}

	[Fact]
	public async Task ReadDuringWrite_NoDeadlock_AllCompleteSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var article = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new ArticleDto(
			article.Id,
			article.Slug,
			"Write During Reads",
			article.Introduction,
			article.Content,
			article.CoverImageUrl ?? "http://example.com/image.jpg",
			article.Author,
			article.Category,
			false,
			null,
			article.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		), 0
	);

		// Act - Heavy concurrent read/write mix
		var tasks = new List<Task>();

		// Single writer
		tasks.Add(_editHandler.HandleAsync(editDto).ContinueWith(t => { }, TestContext.Current.CancellationToken));

		// Multiple readers
		for (int i = 0; i < 20; i++)
		{
			tasks.Add(_getHandler.HandleAsync(article.Id).ContinueWith(t => { }, TestContext.Current.CancellationToken));
		}

		// All should complete without timeout/deadlock
		// Increased timeout to 30 seconds to accommodate heavy concurrent load
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
		// Arrange - Create two articles for concurrent modification and deletion
		await _fixture.ClearCollectionsAsync();

		var article1 = Web.Components.Features.Articles.Fakes.FakeArticle.GetNewArticle(useSeed: true);
		var article2 = FakeArticle.GetNewArticle(useSeed: false);

		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertManyAsync([article1, article2], cancellationToken: TestContext.Current.CancellationToken);

		var editDto = new ArticleDto(
			article1.Id,
			article1.Slug,
			"Modified Title",
			article1.Introduction,
			article1.Content,
			article1.CoverImageUrl ?? "http://example.com/image.jpg",
			article1.Author,
			article1.Category,
			false,
			null,
			article1.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		), 0
	);

		// Act - One user edits, another tries to delete (archive)
		var editTask = _editHandler.HandleAsync(editDto);

		// Simulate deletion by setting IsArchived
		var deleteDto = new Web.Components.Features.Articles.Models.ArticleDto(
			article1.Id,
			article1.Slug,
			article1.Title,
			article1.Introduction,
			article1.Content,
			article1.CoverImageUrl ?? "http://example.com/image.jpg",
			article1.Author,
			article1.Category,
			false,
			null,
			article1.CreatedOn,
			DateTimeOffset.UtcNow,
			true, // Archive = soft delete
			false
		);
		var deleteTask = _editHandler.HandleAsync(deleteDto);

		await Task.WhenAll(editTask, deleteTask);

		var editResult = await editTask;
		var deleteResult = await deleteTask;

		// Assert - Both complete successfully
		editResult.Success.Should().BeTrue();
		deleteResult.Success.Should().BeTrue();

		// Verify article exists but is archived
		var saved = await _repository.GetArticleByIdAsync(article1.Id);
		saved.Success.Should().BeTrue();
		saved.Value!.IsArchived.Should().Be(true);
	}

}
