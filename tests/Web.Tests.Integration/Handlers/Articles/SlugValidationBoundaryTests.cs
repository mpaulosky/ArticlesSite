// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     SlugValidationBoundaryTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Web.Components.Features.Articles.Interfaces;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Validators;
using Web.Components.Features.AuthorInfo.Fakes;

namespace Web.Tests.Integration.Handlers.Articles;

/// <summary>
/// Integration tests for slug validation boundary cases.
/// Tests regex pattern: ^[a-z0-9_]+$
/// Valid: lowercase letters, numbers, underscores only
/// Invalid: uppercase, spaces, special characters
/// </summary>
[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class SlugValidationBoundaryTests
{

	private readonly MongoDbFixture _fixture;

	private readonly IArticleRepository _repository;

	private readonly Components.Features.Articles.ArticleCreate.CreateArticle.ICreateArticleHandler _createHandler;

	private readonly ILogger<Components.Features.Articles.ArticleCreate.CreateArticle.Handler> _logger;

	private readonly IValidator<ArticleDto> _validator;

	public SlugValidationBoundaryTests(MongoDbFixture fixture)
	{
		_fixture = fixture;
		_repository = new ArticleRepository(_fixture.ContextFactory);
		_logger = Substitute.For<ILogger<Components.Features.Articles.ArticleCreate.CreateArticle.Handler>>();
		_validator = new ArticleDtoValidator();
		_createHandler = new Components.Features.Articles.ArticleCreate.CreateArticle.Handler(_repository, _logger, _validator);
	}

	[Fact]
	public async Task CreateArticle_WithValidSlugLowercase_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "simple_lowercase_slug";
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Slug.Should().Be(validSlug);
	}

	[Fact]
	public async Task CreateArticle_WithValidSlugNumbers_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "article_2025_part_1";
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Value!.Slug.Should().Be(validSlug);
	}

	[Fact]
	public async Task CreateArticle_WithValidSlugUnderscores_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "_____"; // Only underscores
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithValidSlugNumbers0To9_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "0123456789"; // All numbers
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugUppercase_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "Invalid_Uppercase_SLUG"; // Contains uppercase
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
		result.Error.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task CreateArticle_WithSlugSpaces_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "slug with spaces"; // Contains spaces
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugHyphen_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "slug-with-hyphens"; // Contains hyphens
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugDots_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "slug.with.dots"; // Contains dots
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugSpecialCharacters_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "slug!@#$%^&*()"; // Contains special characters
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugUnicodeCharacters_FailsValidation()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var invalidSlug = "slug_café_ñandú"; // Contains unicode characters
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugMixedValidCharacters_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "a1b2c3_d4e5f6"; // Mix of letters, numbers, underscores
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
	}

	[Fact]
	public async Task CreateArticle_WithSlugConsecutiveUnderscores_CreatesSuccessfully()
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var validSlug = "slug___with___underscores";
		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
	}

	[Theory]
	[InlineData("valid_slug")]
	[InlineData("slug123")]
	[InlineData("a")]
	[InlineData("123")]
	[InlineData("_")]
	[InlineData("abc_def_123")]
	public async Task CreateArticle_WithValidSlugPatterns_CreatesSuccessfully(string validSlug)
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			validSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Success.Should().BeTrue(because: $"Slug '{validSlug}' should match pattern ^[a-z0-9_]+$");
	}

	[Theory]
	[InlineData("UPPERCASE")]
	[InlineData("with-hyphens")]
	[InlineData("with spaces")]
	[InlineData("with.dots")]
	[InlineData("with!special")]
	[InlineData("café")]
	public async Task CreateArticle_WithInvalidSlugPatterns_FailsValidation(string invalidSlug)
	{
		// Arrange
		await _fixture.ClearCollectionsAsync();

		var category = FakeCategory.GetNewCategory(useSeed: true);
		var author = FakeAuthorInfo.GetNewAuthorInfo(useSeed: true);

		var dto = new ArticleDto(
			ObjectId.Empty,
			invalidSlug,
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
			false, 0`r`n`t);

		// Act
		var result = await _createHandler.HandleAsync(dto);

		// Assert
		result.Should().NotBeNull();
		result.Failure.Should().BeTrue(because: $"Slug '{invalidSlug}' should NOT match pattern ^[a-z0-9_]+$");
	}

}
