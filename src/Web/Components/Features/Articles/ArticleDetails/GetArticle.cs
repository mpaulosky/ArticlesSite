// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleDetails;

public static class GetArticle
{

	/// <summary>
	/// Interface for handling article retrieval operations.
	/// </summary>
	public interface IGetArticleHandler
	{

		/// <summary>
		/// Handles retrieval of an article by identifier.
		/// </summary>
		/// <param name="id">The article identifier.</param>
		/// <returns>A <see cref="Result{ArticleDto}"/> representing the outcome.</returns>
		Task<Result<ArticleDto>> HandleAsync(ObjectId id);

	}

	/// <summary>
	/// Command handler for retrieving an article.
	/// </summary>
	public class Handler
	(
			IArticleRepository repository,
			ILogger<Handler> logger
	) : IGetArticleHandler
	{

		/// <inheritdoc />
		public async Task<Result<ArticleDto>> HandleAsync(ObjectId id)
		{
			if (id == ObjectId.Empty)
			{
				logger.LogWarning("GetArticle: Invalid article ID provided");

				return Result.Fail<ArticleDto>("Article ID cannot be empty");
			}

			Result<Article?> result = await repository.GetArticleByIdAsync(id);

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetArticle: Article not found with ID: {Id}", id);

				return Result.Fail<ArticleDto>(result.Error ?? "Article not found");
			}

			Article article = result.Value;

			var dto = new ArticleDto(
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
			);

			logger.LogInformation("GetArticle: Successfully retrieved article {Id}", id);

			return Result.Ok(dto);
		}

	}

}
