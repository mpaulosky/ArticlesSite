// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticles.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

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
		/// <returns>A <see cref="Result{IEnumerable{ArticleDto}}"/> representing the outcome.</returns>
		Task<Result<IEnumerable<ArticleDto>>> HandleAsync();
	}

	/// <summary>
	/// Command handler for retrieving all articles.
	/// </summary>
	public class Handler(IArticleRepository repository, ILogger<Handler> logger) : IGetArticlesHandler
	{
		/// <inheritdoc />
		public async Task<Result<IEnumerable<ArticleDto>>> HandleAsync()
		{
			Result<IEnumerable<Article>?> result = await repository.GetArticles();

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetArticles: Failed to retrieve articles. Error: {Error}", result.Error);
				return Result.Fail<IEnumerable<ArticleDto>>(result.Error ?? "Failed to retrieve articles");
			}

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
				!article.IsArchived
			)).ToList();

			logger.LogInformation("GetArticles: Successfully retrieved {Count} articles", dtos.Count);
			return Result.Ok<IEnumerable<ArticleDto>>(dtos);
		}
	}

}
