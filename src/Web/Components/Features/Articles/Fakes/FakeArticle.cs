// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using Web.Components.Features.AuthorInfo.Fakes;

namespace Web.Components.Features.Articles.Fakes;

/// <summary>
///   Provides fake data generation methods for the <see cref="Article" /> entity.
/// </summary>
public static class FakeArticle
{

	private const int Seed = 621;

	/// <summary>
	///   Generates a new fake <see cref="Article" /> object.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A single fake <see cref="Article" /> object.</returns>
	public static Article GetNewArticle(bool useSeed = false)
	{

		Article? article = GenerateFake(useSeed).Generate();

		// No AuthorId or CategoryId needed for MongoDB
		return article;

	}

	/// <summary>
	///   Generates a list of fake <see cref="Article" /> objects.
	/// </summary>
	/// <param name="numberRequested">The number of <see cref="Article" /> objects to generate.</param>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A list of fake <see cref="Article" /> objects.</returns>
	public static List<Article> GetArticles(int numberRequested, bool useSeed = false)
	{
		List<Article> articles = new();

		// Reuse a single Faker instance within this call to ensure unique items in the list.
		// For seeded runs, create a fresh seeded instance per call so repeated calls yield the same sequence.
		Faker<Article>? faker = GenerateFake(useSeed);

		// Ensure CreatedOn/ModifiedOn are deterministic for seeded list generation across separate calls
		if (useSeed)
		{
			faker = faker
					.RuleFor(f => f.CreatedOn, _ => GetStaticDate())
					.RuleFor(f => f.ModifiedOn, _ => null);
		}

		for (int i = 0; i < numberRequested; i++)
		{
			Article? article = faker.Generate();

			// No AuthorId or CategoryId needed for MongoDB
			articles.Add(article);
		}

		return articles;

	}

	/// <summary>
	///   Generates a Faker instance configured to generate fake <see cref="Article" /> objects.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>Configured Faker <see cref="Article" /> instance.</returns>
	private static Faker<Article> GenerateFake(bool useSeed = false)
	{
		Faker<Article>? fake = new Faker<Article>()
				.RuleFor(a => a.Id, _ => ObjectId.GenerateNewId())
				.RuleFor(a => a.Title, (f, _) => f.WaffleTitle())
				.RuleFor(a => a.Introduction, (f, _) => f.Lorem.Sentence())
				.RuleFor(a => a.Content, (f, _) => f.WaffleMarkdown(5))
				.RuleFor(a => a.Slug, (_, a) => a.Title.GenerateSlug())
				.RuleFor(a => a.CoverImageUrl, (f, _) => f.Image.PicsumUrl())
				.RuleFor(a => a.IsPublished, (f, _) => f.Random.Bool())
				.RuleFor(a => a.PublishedOn, (_, a) => a.IsPublished ? DateTime.Now : (DateTime?)null)
				.RuleFor(a => a.Category, FakeCategory.GetNewCategory(useSeed))
				.RuleFor(a => a.Author, FakeAuthorInfo.GetNewAuthorInfo(useSeed))
				.RuleFor(a => a.CreatedOn, DateTimeOffset.UtcNow)
				.RuleFor(a => a.ModifiedOn, _ => null);

		return useSeed ? fake.UseSeed(Seed) : fake;

	}

}
