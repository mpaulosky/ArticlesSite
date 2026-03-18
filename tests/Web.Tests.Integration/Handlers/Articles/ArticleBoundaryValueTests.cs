// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleBoundaryValueTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Microsoft.Extensions.Options;
using Web.Infrastructure;

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
/// Integration tests for boundary value scenarios in Article handlers.
/// Tests property length limits and edge cases.
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleBoundaryValueTests
{

	private readonly MongoDbFixture _fixture;

	private readonly CreateArticle.ICreateArticleHandler _createHandler;

	private readonly Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler _editHandler;

	public ArticleBoundaryValueTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		IArticleRepository repository = new ArticleRepository(_fixture.ContextFactory);
		ILogger<CreateArticle.Handler> logger = Substitute.For<ILogger<CreateArticle.Handler>>();
		IValidator<ArticleDto> validator = new ArticleDtoValidator();
		_createHandler = new CreateArticle.Handler(repository, logger, validator);

		var editLogger = Substitute.For<ILogger<Components.Features.Articles.ArticleEdit.EditArticle.Handler>>();
		var options = Options.Create(new ConcurrencyOptions());
		_editHandler = new Components.Features.Articles.ArticleEdit.EditArticle.Handler(repository, editLogger, validator, options);
	}

	[Fact]
	public async Task CreateArticle_WithTitleAtMaxLength_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var maxLengthTitle = new string('a', 100); // Max allowed: 100 chars
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			maxLengthTitle,
			"Test Introduction",
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Title.Should().Be(maxLengthTitle);
		result.Value.Title.Length.Should().Be(100);
	}

	[Fact]
	public async Task CreateArticle_WithTitleExceedingMaxLength_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var oversizedTitle = new string('a', 101); // Exceeds max by 1 char
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			oversizedTitle,
			"Test Introduction",
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task CreateArticle_WithIntroductionAtMaxLength_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var maxLengthIntroduction = new string('b', 200); // Max allowed: 200 chars
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			maxLengthIntroduction,
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Introduction.Length.Should().Be(200);
	}

	[Fact]
	public async Task CreateArticle_WithIntroductionExceedingMaxLength_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var oversizedIntroduction = new string('b', 201); // Exceeds max by 1 char
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			oversizedIntroduction,
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task CreateArticle_WithContentAtMaxLength_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var maxLengthContent = new string('c', 4000); // Max allowed: 4000 chars
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			"Test Introduction",
			maxLengthContent,
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Content.Length.Should().Be(4000);
	}

	[Fact]
	public async Task CreateArticle_WithContentExceedingMaxLength_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var oversizedContent = new string('c', 10001); // Exceeds max by 1 char
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			"Test Introduction",
			oversizedContent,
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task CreateArticle_WithCoverImageUrlAtMaxLength_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var maxLengthUrl = "http://example.com/" + new string('a', 181); // Total 200 chars
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			"Test Introduction",
			"Test Content",
			maxLengthUrl,
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.CoverImageUrl.Length.Should().Be(200);
	}

	[Fact]
	public async Task CreateArticle_WithCoverImageUrlExceedingMaxLength_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var oversizedUrl = "http://example.com/" + new string('a', 182); // Total 201 chars
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			"test_article",
			"Test Title",
			"Test Introduction",
			"Test Content",
			oversizedUrl,
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugAtMaxLength_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var maxLengthSlug = new string('a', 200); // Max allowed: 200 chars (lowercase letters only)
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			maxLengthSlug,
			"Test Title",
			"Test Introduction",
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Slug.Length.Should().Be(200);
	}

	[Fact]
	public async Task CreateArticle_WithSlugExceedingMaxLength_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var oversizedSlug = new string('a', 201); // Exceeds max by 1 char
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			oversizedSlug,
			"Test Title",
			"Test Introduction",
			"Test Content",
			"http://example.com/image.jpg",
			author,
			category,
			false,
			null,
			DateTimeOffset.UtcNow,
			null,
			false,
			false
		, 0
	);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task EditArticle_WithMaxLengthProperties_UpdatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var existingArticle = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(existingArticle, cancellationToken: TestContext.Current.CancellationToken);

		var maxLengthTitle = new string('x', 100);
		var maxLengthIntro = new string('y', 200);
		var maxLengthContent = new string('z', 4000);

		var dto = new ArticleDto(
			existingArticle.Id,
			existingArticle.Slug,
			maxLengthTitle,
			maxLengthIntro,
			maxLengthContent,
			existingArticle.CoverImageUrl,
			existingArticle.Author,
			existingArticle.Category,
			false,
			null,
			existingArticle.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		, 0
	);

		// Act
		var result = await _editHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Title.Length.Should().Be(100);
		result.Value.Introduction.Length.Should().Be(200);
		result.Value.Content.Length.Should().Be(4000);
	}

	[Fact]
	public async Task EditArticle_WithExceedingLengthTitle_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var existingArticle = FakeArticle.GetNewArticle(useSeed: true);
		var collection = _fixture.Database.GetCollection<Article>("Articles");
		await collection.InsertOneAsync(existingArticle, cancellationToken: TestContext.Current.CancellationToken);

		var oversizedTitle = new string('x', 101); // Over limit

		var dto = new ArticleDto(
			existingArticle.Id,
			existingArticle.Slug,
			oversizedTitle,
			"Test Intro",
			"Test Content",
			existingArticle.CoverImageUrl,
			existingArticle.Author,
			existingArticle.Category,
			false,
			null,
			existingArticle.CreatedOn,
			DateTimeOffset.UtcNow,
			false,
			false
		, 0
	);

		// Act
		var result = await _editHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

}
