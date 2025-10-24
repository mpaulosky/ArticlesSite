// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FakeAuthorInfo.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Fakes;

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
	public static AuthorInfo GetNewAuthorInfo(bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate();

	}

	/// <summary>
	///   Generates a list of fake <see cref="AuthorInfo" /> objects.
	/// </summary>
	/// <param name="numberRequested">The number of <see cref="AuthorInfo" /> objects to generate.</param>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A list of fake <see cref="AuthorInfo" /> objects.</returns>
	public static List<AuthorInfo> GetAuthorInfos(int numberRequested, bool useSeed = false)
	{

		return GenerateFake(useSeed).Generate(numberRequested);

	}

	/// <summary>
	///   Generates a configured <see cref="Faker" /> for creating fake <see cref="AuthorInfo" /> objects.
	/// </summary>
	/// <param name="useSeed">Indicates whether to apply a fixed seed for deterministic results.</param>
	/// <returns>A configured <see cref="Faker{AuthorInfo}" /> instance.</returns>
	internal static Faker<AuthorInfo> GenerateFake(bool useSeed = false)
	{

		Faker<AuthorInfo>? fake = new Faker<AuthorInfo>()
				.CustomInstantiator(f => new AuthorInfo(
						f.Random.Guid().ToString(), // UserId - simulating Auth0 'sub' claim
						f.Name.FullName()           // Name - display name
				));

		return useSeed ? fake.UseSeed(Seed) : fake;

	}

}