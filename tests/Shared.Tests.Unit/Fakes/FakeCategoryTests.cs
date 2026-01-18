//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     FakeCategoryTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Fakes;

[ExcludeFromCodeCoverage]
public class FakeCategoryTests
{

	[Fact]
	public void GetNewCategory_WithoutSeed_ShouldReturnValidCategory()
	{
		// Arrange & Act
		Category result = FakeCategory.GetNewCategory();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.CreatedOn.Should().NotBeNull();
		result.ModifiedOn.Should().BeNull();
	}

	[Fact]
	public void GetNewCategory_WithSeed_ShouldReturnValidCategory()
	{
		// Arrange & Act
		Category result = FakeCategory.GetNewCategory(true);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetNewCategory_WithoutSeed_ShouldReturnDifferentCategories()
	{
		// Arrange & Act
		Category result1 = FakeCategory.GetNewCategory();
		Category result2 = FakeCategory.GetNewCategory();

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();

		// Note: CategoryName might be the same due to limited category names in GetRandomCategoryName
		result1.Id.Should().NotBe(result2.Id);
	}

	[Fact]
	public void GetCategories_WithZeroCount_ShouldReturnEmptyList()
	{
		// Arrange & Act
		List<Category> result = FakeCategory.GetCategories(0);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetCategories_WithPositiveCount_ShouldReturnCorrectNumberOfCategories()
	{
		// Arrange
		const int count = 5;

		// Act
		List<Category> result = FakeCategory.GetCategories(count);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
	}

	[Fact]
	public void GetCategories_WithSeed_ShouldReturnValidList()
	{
		// Arrange
		const int count = 3;

		// Act
		List<Category> result = FakeCategory.GetCategories(count, true);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
	}

	[Fact]
	public void GetCategories_WithoutSeed_ShouldReturnDifferentLists()
	{
		// Arrange
		const int count = 10;

		// Act
		List<Category> result1 = FakeCategory.GetCategories(count);
		List<Category> result2 = FakeCategory.GetCategories(count);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);

		// Check that at least the IDs are different
		result1[0].Id.Should().NotBe(result2[0].Id);
	}

	[Fact]
	public void GetCategories_AllCategories_ShouldHaveValidProperties()
	{
		// Arrange
		const int count = 5;

		// Act
		List<Category> result = FakeCategory.GetCategories(count);

		// Assert
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
		result.Should().OnlyContain(c => c.CreatedOn != null);
		result.Should().OnlyContain(c => c.ModifiedOn == null);
	}

	[Fact]
	public void GenerateFake_WithoutSeed_ShouldReturnConfiguredFaker()
	{
		// Arrange & Act
		Category result = FakeCategory.GetNewCategory();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void GenerateFake_WithSeed_ShouldProduceValidResults()
	{
		// Arrange & Act
		Category result = FakeCategory.GetNewCategory(true);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetCategories_WithLargeCount_ShouldGenerateSuccessfully()
	{
		// Arrange
		const int count = 100;

		// Act
		List<Category> result = FakeCategory.GetCategories(count);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
	}

	[Fact]
	public void GetNewCategory_CreatedOn_ShouldBeUtcNow()
	{
		// Arrange
		DateTimeOffset before = DateTimeOffset.UtcNow.AddSeconds(-1);

		// Act
		Category result = FakeCategory.GetNewCategory();

		// Assert
		DateTimeOffset after = DateTimeOffset.UtcNow.AddSeconds(1);
		result.CreatedOn.Should().NotBeNull();
		result.CreatedOn!.Value.Should().BeAfter(before);
		result.CreatedOn!.Value.Should().BeBefore(after);
	}

	[Fact]
	public void GetNewCategory_ModifiedOn_ShouldBeNull()
	{
		// Arrange & Act
		Category result = FakeCategory.GetNewCategory();

		// Assert
		result.ModifiedOn.Should().BeNull();
	}

	[Fact]
	public void GetCategories_ShouldHaveUniqueIds()
	{
		// Arrange
		const int count = 20;

		// Act
		List<Category> result = FakeCategory.GetCategories(count);

		// Assert
		result.Should().HaveCount(count);
		result.Select(c => c.Id).Distinct().Should().HaveCount(count, "all categories should have unique IDs");
	}

	[Fact]
	public void GetNewCategory_CategoryName_ShouldBeFromPredefinedList()
	{
		// Arrange
		string[] validCategoryNames = new[]
		{
				"ASP.NET Core", "Blazor Server", "Blazor WebAssembly", "C# Programming", "Entity Framework Core (EF Core)",
				".NET MAUI", "General Programming", "Web Development", "Other .NET Topics"
		};

		const int iterations = 50;
		List<string> categoryNames = new();

		// Act
		for (int i = 0; i < iterations; i++)
		{
			Category category = FakeCategory.GetNewCategory();
			categoryNames.Add(category.CategoryName);
		}

		// Assert
		categoryNames.Should().OnlyContain(name => validCategoryNames.Contains(name));
	}

}
