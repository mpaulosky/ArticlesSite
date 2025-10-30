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
		/// Handles retrieval of all articles.
		/// </summary>
		/// <param name="user">The current user's claims principal for authorization.</param>
		/// <returns>A <see cref="Result{T}"/> representing the outcome.</returns>
		Task<Result<IEnumerable<ArticleDto>>> HandleAsync(ClaimsPrincipal? user = null);

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
		public async Task<Result<IEnumerable<ArticleDto>>> HandleAsync(ClaimsPrincipal? user = null)
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

			var dtos = result.Value.Select(article => new ArticleDto(
					article.Id,
					article.Slug,
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
					DetermineCanEdit(article, currentUserId, isAdmin)
			)).ToList();

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

			// Admins can edit any non-archived article
			if (isAdmin)
				return true;

			// Authors can edit their own non-archived articles
			if (!string.IsNullOrEmpty(currentUserId) && article.Author?.UserId == currentUserId)
				return true;

			return false;
		}

	}

}
