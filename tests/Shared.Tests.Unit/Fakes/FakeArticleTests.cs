//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     FakeArticleTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Fakes;

[ExcludeFromCodeCoverage]
public class FakeArticleTests
{

	[Fact]
	public void GetNewArticle_WithoutSeed_ShouldReturnValidArticle()
	{
		// Arrange & Act
		Article result = FakeArticle.GetNewArticle();

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
		result.CreatedOn.Should().NotBeNull();
	}

	[Fact]
	public void GetNewArticle_WithSeed_ShouldReturnValidArticle()
	{
		// Arrange & Act
		Article result = FakeArticle.GetNewArticle(true);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Category.Should().NotBeNull();
		result.Author.Should().NotBeNull();
	}

	[Fact]
	public void GetNewArticle_WithoutSeed_ShouldReturnDifferentArticles()
	{
		// Arrange & Act
		Article result1 = FakeArticle.GetNewArticle();
		Article result2 = FakeArticle.GetNewArticle();

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.Title.Should().NotBe(result2.Title);
	}

	[Fact]
	public void GetArticles_WithZeroCount_ShouldReturnEmptyList()
	{
		// Arrange & Act
		List<Article> result = FakeArticle.GetArticles(0);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetArticles_WithPositiveCount_ShouldReturnCorrectNumberOfArticles()
	{
		// Arrange
		const int count = 5;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
	}

	[Fact]
	public void GetArticles_WithSeed_ShouldReturnValidList()
	{
		// Arrange
		const int count = 3;

		// Act
		List<Article> result = FakeArticle.GetArticles(count, true);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
	}

	[Fact]
	public void GetArticles_WithoutSeed_ShouldReturnDifferentLists()
	{
		// Arrange
		const int count = 3;

		// Act
		List<Article> result1 = FakeArticle.GetArticles(count);
		List<Article> result2 = FakeArticle.GetArticles(count);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);
		result1[0].Title.Should().NotBe(result2[0].Title);
	}

	[Fact]
	public void GetArticles_ShouldReturnUniqueArticlesInList()
	{
		// Arrange
		const int count = 10;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().HaveCount(count);
		result.Select(a => a.Title).Distinct().Should().HaveCount(count, "all articles should have unique titles");
	}

	[Fact]
	public void GetArticles_AllArticles_ShouldHaveValidProperties()
	{
		// Arrange
		const int count = 5;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Title));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Introduction));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Content));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.Slug));
		result.Should().OnlyContain(a => !string.IsNullOrWhiteSpace(a.CoverImageUrl));
		result.Should().OnlyContain(a => a.Category != null);
		result.Should().OnlyContain(a => a.Author != null);
		result.Should().OnlyContain(a => a.CreatedOn != null);
	}

	[Fact]
	public void GetArticles_ShouldGenerateValidIsPublishedValues()
	{
		// Arrange
		const int count = 20;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().Contain(a => a.IsPublished);
		result.Should().Contain(a => !a.IsPublished);
		result.Where(a => a.IsPublished).Should().OnlyContain(a => a.PublishedOn != null);
		result.Where(a => !a.IsPublished).Should().OnlyContain(a => a.PublishedOn == null);
	}

	[Fact]
	public void GetArticles_ShouldHaveNullModifiedOn()
	{
		// Arrange
		const int count = 5;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().OnlyContain(a => a.ModifiedOn == null);
	}

	[Fact]
	public void GenerateFake_WithoutSeed_ShouldReturnConfiguredFaker()
	{
		// Arrange & Act
		Article result = FakeArticle.GetNewArticle();

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
		Article result = FakeArticle.GetNewArticle(true);

		// Assert
		result.Should().NotBeNull();
		result.Title.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetNewArticle_Slug_ShouldBeValidFormat()
	{
		// Arrange & Act
		Article result = FakeArticle.GetNewArticle();

		// Assert
		result.Slug.Should().NotBeNullOrWhiteSpace();

		result.Slug.Should()
				.MatchRegex(@"^[a-z0-9_]+$", "slug should only contain lowercase letters, numbers, and underscores");
	}

	[Fact]
	public void GetArticles_WithLargeCount_ShouldGenerateSuccessfully()
	{
		// Arrange
		const int count = 100;

		// Act
		List<Article> result = FakeArticle.GetArticles(count);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(a => a.Id != ObjectId.Empty);
	}

}
