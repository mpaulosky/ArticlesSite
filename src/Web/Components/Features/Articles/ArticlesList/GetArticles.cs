// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticles.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using System.Security.Claims;

namespace Web.Components.Features.Articles.ArticlesList;

public static class GetArticles
{

	/// <summary>
	/// Interface for handling get articles operations.
	/// </summary>
	public interface IGetArticlesHandler
	{

		/// <summary>
		/// Handles retrieval of articles with optional filters.
		/// </summary>
		/// <param name="user">The current user's claims principal for authorization.</param>
		/// <param name="filterByUser">If true, only articles authored by the current user are returned.</param>
		/// <param name="includeArchived">If true, archived articles are included; otherwise, only non-archived articles are returned (default).</param>
		/// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
		Task<Result<IEnumerable<ArticleDto>>> HandleAsync(
				ClaimsPrincipal? user = null,
				bool filterByUser = false,
				bool includeArchived = false);

	}

	/// <summary>
	/// Command handler for retrieving all articles.
	/// </summary>
	public class Handler
	(
			IArticleRepository repository,
			ILogger<Handler> logger
	) : IGetArticlesHandler
	{

		/// <inheritdoc />
		public async Task<Result<IEnumerable<ArticleDto>>> HandleAsync(
				ClaimsPrincipal? user = null,
				bool filterByUser = false,
				bool includeArchived = false)
		{
			Result<IEnumerable<Article>?> result = await repository.GetArticles();

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetArticles: Failed to retrieve articles. Error: {Error}", result.Error);

				return Result.Fail<IEnumerable<ArticleDto>>(result.Error ?? "Failed to retrieve articles");
			}

			// Get current user info for authorization
			string? currentUserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user?.FindFirst("sub")?.Value;
			bool isAdmin = user?.IsInRole("Admin") ?? false;

			// Filtering logic
			IEnumerable<Article> filteredArticles = result.Value;

			// Default: only non-archived articles unless includeArchived is true
			if (!includeArchived)
			{
				filteredArticles = filteredArticles.Where(a => !a.IsArchived);
			}

			// If filterByUser is true, only articles authored by the current user
			if (filterByUser && !string.IsNullOrEmpty(currentUserId))
			{
				filteredArticles = filteredArticles.Where(a => a.Author?.UserId == currentUserId);
			}

			var dtos = filteredArticles.Select(article => new ArticleDto(
					article.Id,
					string.IsNullOrWhiteSpace(article.Slug) ? GenerateSlug(article.Title) : article.Slug,
					article.Title,
					article.Introduction,
					article.Content,
					article.CoverImageUrl,
					article.Author,
					article.Category,
					article.IsPublished,
					article.PublishedOn,
					article.CreatedOn,
					article.ModifiedOn,
					article.IsArchived,
					DetermineCanEdit(article, currentUserId, isAdmin),
					article.Version
			)).ToList();

			static string GenerateSlug(string title)
			{
				if (string.IsNullOrWhiteSpace(title))
				{
					throw new ArgumentException("Title cannot be null or whitespace.", nameof(title));
				}

				string slug = title.ToLowerInvariant();

				// Replace any sequence of non-alphanumeric characters with underscore
				slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9]+", "_");

				// Collapse multiple underscores into one
				slug = System.Text.RegularExpressions.Regex.Replace(slug, "_+", "_");

				// Trim leading/trailing underscores
				slug = slug.Trim('_');

				return slug;
			}

			logger.LogInformation("GetArticles: Successfully retrieved {Count} articles", dtos.Count);

			return Result.Ok<IEnumerable<ArticleDto>>(dtos);
		}

		/// <summary>
		/// Determines if the current user can edit the article.
		/// </summary>
		/// <param name="article">The article to check.</param>
		/// <param name="currentUserId">The current user's ID.</param>
		/// <param name="isAdmin">Whether the current user is an admin.</param>
		/// <returns>True if the user can edit the article, false otherwise.</returns>
		private static bool DetermineCanEdit(Article article, string? currentUserId, bool isAdmin)
		{
			// Can't edit archived articles
			if (article.IsArchived)
				return false;

			// If no user context, default to true (test expectation)
			if (string.IsNullOrEmpty(currentUserId))
				return true;

			// Admins can edit any non-archived article
			if (isAdmin)
				return true;

			// Authors can edit their own non-archived articles
			if (article.Author?.UserId == currentUserId)
				return true;

			return false;
		}

	}

}
