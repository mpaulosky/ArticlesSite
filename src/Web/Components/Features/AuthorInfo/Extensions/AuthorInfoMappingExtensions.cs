// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthorInfoMappingExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.AuthorInfo.Extensions;

/// <summary>
/// Extension methods for mapping AuthorInfo (currently no separate DTO exists).
/// Provided for consistency with mapping an extension pattern and future extensibility.
/// </summary>
public static class AuthorInfoMappingExtensions
{

	/// <summary>
	/// Creates a copy of AuthorInfo (defensive copy for immutability).
	/// AuthorInfo is a record, so this provides consistent copying semantics.
	/// </summary>
	/// <param name="authorInfo">The author info to copy.</param>
	/// <returns>A new AuthorInfo instance with the same values.</returns>
	public static Entities.AuthorInfo Copy(this Entities.AuthorInfo? authorInfo)
	{
		if (authorInfo is null)
		{
			return Entities.AuthorInfo.Empty;
		}

		return new Entities.AuthorInfo(authorInfo.UserId, authorInfo.Name);
	}

	/// <summary>
	/// Determines if the author info is empty or has no meaningful data.
	/// </summary>
	/// <param name="authorInfo">The author info to check.</param>
	/// <returns>True if the author info is null, empty, or has empty user ID and name; otherwise false.</returns>
	public static bool IsEmpty(this Entities.AuthorInfo? authorInfo)
	{
		return authorInfo is null ||
				(string.IsNullOrWhiteSpace(authorInfo.UserId) && string.IsNullOrWhiteSpace(authorInfo.Name));
	}

}
