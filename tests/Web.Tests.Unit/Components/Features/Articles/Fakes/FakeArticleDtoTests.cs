//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     FakeArticleDtoTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

namespace Web.Components.Features.Articles.Fakes;

[ExcludeFromCodeCoverage]
public class FakeArticleDtoTests
{

	[Fact]
	public void GetNewArticleDto_WithoutSeed_ShouldReturnValidArticleDto()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Introduction.Should().NotBeNullOrWhiteSpace();
		result.Content.Should().NotBeNullOrWhiteSpace();
		result.Slug.Should().NotBeNullOrWhiteSpace();
		result.CoverImageUrl.Should().NotBeNullOrWhiteSpace();
		result.Category.Should().NotBeNull();
		result.Author.Should().NotBeNull();
		result.CreatedOn.Should().NotBe(default);
		result.ModifiedOn.Should().NotBe(default);
	}

	[Fact]
	public void GetNewArticleDto_WithSeed_ShouldReturnValidArticleDto()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto(true);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Category.Should().NotBeNull();
		result.Author.Should().NotBeNull();
	}

	[Fact]
	public void GetNewArticleDto_WithoutSeed_ShouldReturnDifferentArticleDtos()
	{
		// Arrange & Act
		ArticleDto result1 = FakeArticleDto.GetNewArticleDto();
		ArticleDto result2 = FakeArticleDto.GetNewArticleDto();

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.Title.Should().NotBe(result2.Title);
	}

	[Fact]
	public void GetArticleDtos_WithZeroCount_ShouldReturnEmptyList()
	{
		// Arrange & Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(0);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetArticleDtos_WithPositiveCount_ShouldReturnCorrectNumberOfArticleDtos()
	{
		// Arrange
		const int count = 5;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
	}

	[Fact]
	public void GetArticleDtos_WithSeed_ShouldReturnValidList()
	{
		// Arrange
		const int count = 3;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count, true);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
	}

	[Fact]
	public void GetArticleDtos_WithoutSeed_ShouldReturnDifferentLists()
	{
		// Arrange
		const int count = 3;

		// Act
		List<ArticleDto> result1 = FakeArticleDto.GetArticleDtos(count);
		List<ArticleDto> result2 = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);
		result1[0].Title.Should().NotBe(result2[0].Title);
	}

	[Fact]
	public void GetArticleDtos_AllArticleDtos_ShouldHaveValidProperties()
	{
		// Arrange
		const int count = 5;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Introduction));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Content));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Slug));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.CoverImageUrl));
		result.Should().OnlyContain(a => a.Category != null);
		result.Should().OnlyContain(a => a.Author != null);
	}

	[Fact]
	public void GetArticleDtos_ShouldGenerateValidIsPublishedValues()
	{
		// Arrange
		const int count = 20;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result.Should().Contain(a => a.IsPublished);
		result.Should().Contain(a => !a.IsPublished);
		result.Where(a => a.IsPublished).Should().OnlyContain(a => a.PublishedOn != null);
		result.Where(a => !a.IsPublished).Should().OnlyContain(a => a.PublishedOn == null);
	}

	[Fact]
	public void GenerateFake_WithoutSeed_ShouldReturnConfiguredFaker()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Category.Should().NotBeNull();
		result.Author.Should().NotBeNull();
	}

	[Fact]
	public void GenerateFake_WithSeed_ShouldProduceValidResults()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto(true);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetNewArticleDto_Slug_ShouldBeValidFormat()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto();

		// Assert
		result.Slug.Should().NotBeNullOrWhiteSpace();

		result.Slug.Should()
				.MatchRegex(@"^[a-z0-9_]+$", "slug should only contain lowercase letters, numbers, and underscores");
	}

	[Fact]
	public void GetArticleDtos_WithLargeCount_ShouldGenerateSuccessfully()
	{
		// Arrange
		const int count = 100;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
	}

	[Fact]
	public void GetNewArticleDto_CoverImageUrl_ShouldNotBeNullOrEmpty()
	{
		// Arrange & Act
		ArticleDto result = FakeArticleDto.GetNewArticleDto();

		// Assert
		result.CoverImageUrl.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void GetArticleDtos_ShouldReturnUniqueArticlesInList()
	{
		// Arrange
		const int count = 10;

		// Act
		List<ArticleDto> result = FakeArticleDto.GetArticleDtos(count);

		// Assert
		result.Should().HaveCount(count);
		result.Select(a => a.Title).Distinct().Should().HaveCount(count, "all articles should have unique titles");
	}

}
