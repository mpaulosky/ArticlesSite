// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Helpers.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Shared
// =======================================================

namespace Shared.Helpers;

/// <summary>
///   Provides helper methods for common operations.
/// </summary>
public static partial class Helpers
{

	private static readonly DateTimeOffset StaticDate = new(2025, 1, 1, 8, 0, 0, TimeSpan.Zero);

	/// <summary>
	///   Gets a static date for testing purposes.
	/// </summary>
	/// <returns>A static date of January 1, 2025, at 08:00 AM.</returns>
	public static DateTimeOffset GetStaticDate()
	{
		return StaticDate;
	}

	/// <summary>
	///   Converts a string to a URL-friendly slug.
	/// </summary>
	/// <param name="item">The string to convert to a slug.</param>
	/// <returns>A URL-encoded slug.</returns>
	public static string GetSlug(this string item)
	{

		string slug = MyRegex().Replace(item.Trim().ToLower(), "_");

		return HttpUtility.UrlEncode(slug);
	}

	[GeneratedRegex(@"[^a-z0-9]+")] private static partial Regex MyRegex();

	/// <summary>
	///   Gets a random category name from predefined categories.
	/// </summary>
	/// <returns>A random category name.</returns>
	public static string GetRandomCategoryName()
	{

		List<string> categories =
		[
				MyCategories.First,
				MyCategories.Second,
				MyCategories.Third,
				MyCategories.Fourth,
				MyCategories.Fifth,
				MyCategories.Sixth,
				MyCategories.Seventh,
				MyCategories.Eighth,
				MyCategories.Ninth
		];

		return categories[new Random().Next(categories.Count)];

	}

}