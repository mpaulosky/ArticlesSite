// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthorInfo.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using Web.Components.Features.AuthorInfo.Entities;

namespace Web.Components.Features.AuthorInfo.Fakes;

/// <summary>
///   Provides fake data generation methods for the <see cref="AuthorInfo" /> record.
/// </summary>
public static class FakeAuthorInfo
{

	private const int Seed = 621;

	/// <summary>
	///   Generates a new fake <see cref="AuthorInfo" /> object.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A single fake <see cref="AuthorInfo" /> object.</returns>
	public static Entities.AuthorInfo GetNewAuthorInfo(bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate();

	}

	/// <summary>
	///   Generates a list of fake <see cref="AuthorInfo" /> objects.
	/// </summary>
	/// <param name="numberRequested">The number of <see cref="AuthorInfo" /> objects to generate.</param>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A list of fake <see cref="AuthorInfo" /> objects.</returns>
	public static List<Entities.AuthorInfo> GetAuthorInfos(int numberRequested, bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate(numberRequested);

	}

	/// <summary>
	///   Generates a configured <see cref="Faker" /> for creating fake <see cref="AuthorInfo" /> objects.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A configured <see cref="Faker{AuthorInfo}" /> instance.</returns>
	private static Faker<Entities.AuthorInfo> GenerateFake(bool useSeed = false)
	{

		Faker<Entities.AuthorInfo>? fake = new Faker<Entities.AuthorInfo>()
				.CustomInstantiator(f => new Entities.AuthorInfo(
						f.Random.Guid().ToString(), // UserId - simulating Auth0 'sub' claim
						f.Name.FullName()           // Name - display name
				));

		return useSeed ? fake.UseSeed(Seed) : fake;

	}

}
