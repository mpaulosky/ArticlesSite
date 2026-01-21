// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeCategoryDto.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using Web.Components.Features.Categories.Models;

namespace Web.Components.Features.Categories.Fakes;

/// <summary>
///   Provides fake data generation methods for the <see cref="CategoryDto" /> entity.
/// </summary>
public static class FakeCategoryDto
{

	private const int Seed = 621;

	/// <summary>
	///   Generates a new fake <see cref="CategoryDto" /> object.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A single fake <see cref="CategoryDto" /> object.</returns>
	public static CategoryDto GetNewCategoryDto(bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate();

	}

	/// <summary>
	///   Generates a list of fake <see cref="CategoryDto" /> objects.
	/// </summary>
	/// <param name="numberRequested">The number of <see cref="CategoryDto" /> objects to generate.</param>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A list of fake <see cref="CategoryDto" /> objects.</returns>
	public static List<CategoryDto> GetCategoriesDto(int numberRequested, bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate(numberRequested);

	}

	/// <summary>
	///   Generates a configured <see cref="Faker" /> instance to create fake <see cref="CategoryDto" /> objects.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A configured <see cref="Faker{CategoryDto}" /> instance.</returns>
	private static Faker<CategoryDto> GenerateFake(bool useSeed = false)
	{

		Faker<CategoryDto>? fake = new Faker<CategoryDto>()
				.RuleFor(x => x.Id, _ => ObjectId.GenerateNewId())
				.RuleFor(x => x.CategoryName, _ => GetRandomCategoryName())
				.RuleFor(x => x.IsArchived, f => f.Random.Bool())
				.RuleFor(x => x.CreatedOn, _ => DateTime.Now)
				.RuleFor(x => x.ModifiedOn, _ => DateTime.Now);

		return useSeed ? fake.UseSeed(Seed) : fake;

	}

}
