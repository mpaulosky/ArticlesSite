//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     FakeCategoryDtoTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Fakes;

[ExcludeFromCodeCoverage]
public class FakeCategoryDtoTests
{
	[Fact]
	public void GetNewCategoryDto_WithoutSeed_ShouldReturnValidCategoryDto()
	{
		// Arrange & Act
		CategoryDto result = FakeCategoryDto.GetNewCategoryDto();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.CreatedOn.Should().NotBe(default);
		result.ModifiedOn.Should().NotBe(default);
	}

	[Fact]
	public void GetNewCategoryDto_WithSeed_ShouldReturnValidCategoryDto()
	{
		// Arrange & Act
		CategoryDto result = FakeCategoryDto.GetNewCategoryDto(useSeed: true);

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetNewCategoryDto_WithoutSeed_ShouldReturnDifferentCategoryDtos()
	{
		// Arrange & Act
		CategoryDto result1 = FakeCategoryDto.GetNewCategoryDto();
		CategoryDto result2 = FakeCategoryDto.GetNewCategoryDto();

		// Assert
		result1.Should().NotBeNull();
		result2.Should().NotBeNull();
		result1.Id.Should().NotBe(result2.Id);
	}

	[Fact]
	public void GetCategoriesDto_WithZeroCount_ShouldReturnEmptyList()
	{
		// Arrange & Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(0);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetCategoriesDto_WithPositiveCount_ShouldReturnCorrectNumberOfCategoryDtos()
	{
		// Arrange
		const int count = 5;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
	}

	[Fact]
	public void GetCategoriesDto_WithSeed_ShouldReturnValidList()
	{
		// Arrange
		const int count = 3;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count, useSeed: true);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
	}

	[Fact]
	public void GetCategoriesDto_WithoutSeed_ShouldReturnDifferentLists()
	{
		// Arrange
		const int count = 10;

		// Act
		List<CategoryDto> result1 = FakeCategoryDto.GetCategoriesDto(count);
		List<CategoryDto> result2 = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result1.Should().HaveCount(count);
		result2.Should().HaveCount(count);
		result1[0].Id.Should().NotBe(result2[0].Id);
	}

	[Fact]
	public void GetCategoriesDto_AllCategoryDtos_ShouldHaveValidProperties()
	{
		// Arrange
		const int count = 5;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
		result.Should().OnlyContain(c => !string.IsNullOrWhiteSpace(c.CategoryName));
		result.Should().OnlyContain(c => c.CreatedOn != default);
		result.Should().OnlyContain(c => c.ModifiedOn != null);
	}

	[Fact]
	public void GetCategoriesDto_ShouldGenerateValidIsArchivedValues()
	{
		// Arrange
		const int count = 20;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result.Should().Contain(c => c.IsArchived);
		result.Should().Contain(c => !c.IsArchived);
	}

	[Fact]
	public void GenerateFake_WithoutSeed_ShouldReturnConfiguredFaker()
	{
		// Arrange & Act
		var faker = FakeCategoryDto.GenerateFake();
		CategoryDto result = faker.Generate();

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().NotBe(ObjectId.Empty);
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public void GenerateFake_WithSeed_ShouldProduceValidResults()
	{
		// Arrange & Act
		var faker = FakeCategoryDto.GenerateFake(useSeed: true);
		CategoryDto result = faker.Generate();

		// Assert
		result.Should().NotBeNull();
		result.CategoryName.Should().NotBeNullOrWhiteSpace();
		result.Id.Should().NotBe(ObjectId.Empty);
	}

	[Fact]
	public void GetCategoriesDto_WithLargeCount_ShouldGenerateSuccessfully()
	{
		// Arrange
		const int count = 100;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result.Should().HaveCount(count);
		result.Should().OnlyContain(c => c.Id != ObjectId.Empty);
	}

	[Fact]
	public void GetNewCategoryDto_CreatedOn_ShouldBeDateTime()
	{
		// Arrange
		DateTime before = DateTime.Now.AddSeconds(-1);

		// Act
		CategoryDto result = FakeCategoryDto.GetNewCategoryDto();

		// Assert
		DateTime after = DateTime.Now.AddSeconds(1);
		result.CreatedOn.Should().BeAfter(before);
		result.CreatedOn.Should().BeBefore(after);
	}

	[Fact]
	public void GetNewCategoryDto_ModifiedOn_ShouldBeDateTime()
	{
		// Arrange
		DateTime before = DateTime.Now.AddSeconds(-1);

		// Act
		CategoryDto result = FakeCategoryDto.GetNewCategoryDto();

		// Assert
		DateTime after = DateTime.Now.AddSeconds(1);
		result.ModifiedOn.Should().BeAfter(before);
		result.ModifiedOn.Should().BeBefore(after);
	}

	[Fact]
	public void GetCategoriesDto_ShouldHaveUniqueIds()
	{
		// Arrange
		const int count = 20;

		// Act
		List<CategoryDto> result = FakeCategoryDto.GetCategoriesDto(count);

		// Assert
		result.Should().HaveCount(count);
		result.Select(c => c.Id).Distinct().Should().HaveCount(count, "all category DTOs should have unique IDs");
	}

	[Fact]
	public void GetNewCategoryDto_CategoryName_ShouldBeFromPredefinedList()
	{
		// Arrange
		var validCategoryNames = new[]
		{
			"ASP.NET Core",
			"Blazor Server",
			"Blazor WebAssembly",
			"C# Programming",
			"Entity Framework Core (EF Core)",
			".NET MAUI",
			"General Programming",
			"Web Development",
			"Other .NET Topics"
		};
		const int iterations = 50;
		var categoryNames = new List<string>();

		// Act
		for (int i = 0; i < iterations; i++)
		{
			CategoryDto category = FakeCategoryDto.GetNewCategoryDto();
			categoryNames.Add(category.CategoryName);
		}

		// Assert
		categoryNames.Should().OnlyContain(name => validCategoryNames.Contains(name));
	}
}