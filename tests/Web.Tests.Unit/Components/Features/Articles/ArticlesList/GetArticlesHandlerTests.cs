// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticlesHandlerTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Features.Articles.ArticlesList;

namespace Web.Tests.Unit.Components.Features.Articles.ArticlesList;

/// <summary>
///   Unit tests for GetArticles.Handler
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArticlesHandlerTests
{

	[Fact]
	public async Task HandleAsync_ShouldReturnOnlyNonArchivedByDefault()
	{
		// Arrange
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
				new("A1", "Intro", "Content", string.Empty, author, category) { IsArchived = false },
				new("A2", "Intro", "Content", string.Empty, author, category) { IsArchived = true }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Single(valueList);
		Assert.Equal("A1", valueList[0].Title);
	}

	[Fact]
	public async Task HandleAsync_ShouldIncludeArchivedIfRequested()
	{
		// Arrange
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
				new("A1", "Intro", "Content", string.Empty, author, category) { IsArchived = false },
				new("A2", "Intro", "Content", string.Empty, author, category) { IsArchived = true }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));

		// Act
		var result = await _handler.HandleAsync(null, false, true);

		// Assert
		Assert.True(result.Success);
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Equal(2, valueList.Count);
	}

	[Fact]
	public async Task HandleAsync_ShouldFilterByUser()
	{
		// Arrange
		var author1 = new AuthorInfo("user1", "Test Author");
		var author2 = new AuthorInfo("user2", "Other Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
				new("A1", "Intro", "Content", string.Empty, author1, category) { IsArchived = false },
				new("A2", "Intro", "Content", string.Empty, author2, category) { IsArchived = false }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = await _handler.HandleAsync(user, true);

		// Assert
		Assert.True(result.Success);
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Single(valueList);
		Assert.Equal(author1, valueList[0].Author);
	}

	[Fact]
	public async Task HandleAsync_ShouldFilterByUserAndIncludeArchived()
	{
		// Arrange
		var author1 = new AuthorInfo("user1", "Test Author");
		var author2 = new AuthorInfo("user2", "Other Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
				new("A1", "Intro", "Content", string.Empty, author1, category) { IsArchived = false },
				new("A2", "Intro", "Content", string.Empty, author1, category) { IsArchived = true },
				new("A3", "Intro", "Content", string.Empty, author2, category) { IsArchived = false }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user1") };
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = await _handler.HandleAsync(user, true, true);

		// Assert
		Assert.True(result.Success);
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Equal(2, valueList.Count);
		Assert.All(valueList, a => Assert.Equal(author1, a.Author));
	}

	private readonly IArticleRepository _mockRepository;

	private readonly GetArticles.Handler _handler;

	public GetArticlesHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		var mockLogger = Substitute.For<ILogger<GetArticles.Handler>>();
		_handler = new GetArticles.Handler(_mockRepository, mockLogger);
	}

	[Fact]
	public async Task HandleAsync_WithArticlesAvailable_ShouldReturnSuccess()
	{
		// Arrange
		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var articles = new List<Article>
		{
				new("Test 1", "Intro 1", "Content 1", string.Empty, author, category) { IsPublished = true },
				new("Test 2", "Intro 2", "Content 2", string.Empty, author, category) { IsPublished = false }
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Equal(2, valueList.Count);

		var firstDto = valueList[0];
		Assert.Equal("Test 1", firstDto.Title);
		Assert.Equal(author, firstDto.Author);
		Assert.Equal(category, firstDto.Category);
		Assert.True(firstDto.IsPublished);
	}

	[Fact]
	public async Task HandleAsync_WithEmptyList_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(new List<Article>())));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Error.Should().BeNull();
	}

	[Fact]
	public async Task HandleAsync_WhenRepositoryReturnsNull_ShouldReturnFailure()
	{
		// Arrange
		_mockRepository.GetArticles().Returns(Task.FromResult(Result<IEnumerable<Article>?>.Fail("Database error")));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Database error");
	}

	[Fact]
	public async Task HandleAsync_ShouldMapAllArticleProperties()
	{
		// Arrange
		var publishedOn = DateTimeOffset.UtcNow.AddDays(-5);
		var createdOn = DateTimeOffset.UtcNow.AddDays(-10);
		var modifiedOn = DateTimeOffset.UtcNow.AddDays(-2);

		var author = new AuthorInfo("user1", "Test Author");
		var category = new Category { CategoryName = "Tech" };

		var article =
				new Article("Test Title", "Test Intro", "Test Content", "https://example.com/image.jpg", author, category)
				{
						IsPublished = true,
						PublishedOn = publishedOn,
						CreatedOn = createdOn,
						ModifiedOn = modifiedOn,
						IsArchived = false
				};

		_mockRepository.GetArticles()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(new List<Article> { article })));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		var dto = valueList[0];

		Assert.Equal("test_title", dto.Slug);
		Assert.Equal("Test Title", dto.Title);
		Assert.Equal("Test Intro", dto.Introduction);
		Assert.Equal("Test Content", dto.Content);
		Assert.Equal("https://example.com/image.jpg", dto.CoverImageUrl);
		Assert.Equal(author, dto.Author);
		Assert.Equal(category, dto.Category);
		Assert.True(dto.IsPublished);
		Assert.Equal(publishedOn, dto.PublishedOn);
		Assert.Equal(createdOn, dto.CreatedOn);
		Assert.Equal(modifiedOn, dto.ModifiedOn);
		Assert.False(dto.IsArchived);
		Assert.True(dto.CanEdit); // Default value in DTO constructor
	}

	[Fact]
	public async Task HandleAsync_WithMultipleArticles_ShouldReturnAllInCorrectOrder()
	{
		// Arrange
		var articles = new List<Article>
		{
				new("First", "Intro", "Content", string.Empty, null, null),
				new("Second", "Intro", "Content", string.Empty, null, null),
				new("Third", "Intro", "Content", string.Empty, null, null)
		};

		_mockRepository.GetArticles().Returns(Task.FromResult(Result.Ok<IEnumerable<Article>?>(articles)));

		// Act
		var result = await _handler.HandleAsync();

		// Assert
		result.Success.Should().BeTrue();
		Assert.NotNull(result.Value);
		var valueList = result.Value.ToList();
		Assert.Equal(3, valueList.Count);
		Assert.Equal("First", valueList[0].Title);
		Assert.Equal("Second", valueList[1].Title);
		Assert.Equal("Third", valueList[2].Title);
	}

}