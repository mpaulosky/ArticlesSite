// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Fakes;

/// <summary>
///   Provides fake data generation methods for the <see cref="Category" /> entity.
/// </summary>
public class FakeCategory
{

	private const int Seed = 621;

	/// <summary>
	///   Generates a new fake <see cref="Category" /> object.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A single fake <see cref="Category" /> object.</returns>
	public static Category GetNewCategory(bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate();

	}

	/// <summary>
	///   Generates a list of fake <see cref="Category" /> objects.
	/// </summary>
	/// <param name="numberRequested">The number of <see cref="Category" /> objects to generate.</param>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A list of fake <see cref="Category" /> objects.</returns>
	public static List<Category> GetCategories(int numberRequested, bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate(numberRequested);

	}

	/// <summary>
	///   Generates a Faker Categories instance configured to generate fake Categories objects.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A configured Faker Categories instance.</returns>
	internal static Faker<Category> GenerateFake(bool useSeed = false)
	{

		Faker<Category>? fake = new Faker<Category>()
				.RuleFor(x => x.Id, _ => ObjectId.GenerateNewId())
				.RuleFor(x => x.CategoryName, _ => GetRandomCategoryName())
				.RuleFor(x => x.Slug, f => f.Lorem.Slug())
				.RuleFor(x => x.CreatedOn, _ => DateTimeOffset.UtcNow)
				.RuleFor(x => x.ModifiedOn, _ => null);

		return useSeed ? fake.UseSeed(Seed) : fake;
	}

}
