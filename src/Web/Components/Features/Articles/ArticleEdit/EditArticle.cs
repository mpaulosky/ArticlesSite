// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleEdit;

public static class EditArticle
{

	/// <summary>
	/// Interface for handling article edit operations.
	/// </summary>
	public interface IEditArticleHandler
	{

		/// <summary>
		/// Handles the edit operation for an article.
		/// </summary>
		/// <param name="dto">The article DTO containing updated data.</param>
		/// <returns>A <see cref="Result{ArticleDto}"/> representing the outcome.</returns>
		Task<Result<ArticleDto>> HandleAsync(ArticleDto? dto);

	}

	/// <summary>
	/// Command handler for editing an article.
	/// </summary>
	public class Handler
	(
			IArticleRepository repository,
			ILogger<Handler> logger
	) : IEditArticleHandler
	{

		/// <inheritdoc />
		public async Task<Result<ArticleDto>> HandleAsync(ArticleDto? dto)
		{
			if (dto == null)
			{
				logger.LogWarning("EditArticle: Article DTO cannot be null");

				return Result.Fail<ArticleDto>("Article data cannot be null");
			}

			Result<Article?> existingResult = await repository.GetArticleByIdAsync(dto.Id);

			if (existingResult.Failure || existingResult.Value is null)
			{
				logger.LogWarning("EditArticle: Article not found with ID: {Id}", dto.Id);

				return Result.Fail<ArticleDto>(existingResult.Error ?? "Article not found");
			}

			Article article = existingResult.Value;

			try
			{
				article.Update(
						dto.Title,
						dto.Introduction,
						dto.Content,
						dto.CoverImageUrl,
						dto.IsPublished,
						dto.PublishedOn,
						dto.IsArchived
				);
			}
			catch (ArgumentException ex)
			{
				logger.LogWarning(ex, "EditArticle: Validation failed during update for article {Id}", dto.Id);
				return Result.Fail<ArticleDto>(ex.Message);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "EditArticle: Unexpected error during update for article {Id}", dto.Id);
				return Result.Fail<ArticleDto>("Failed to update article");
			}

			article.Author = dto.Author;
			article.Category = dto.Category;

			Result<Article> result = await repository.UpdateArticle(article);

			if (result.Failure)
			{
				logger.LogWarning("EditArticle: Failed to update article. Error: {Error}", result.Error);

				return Result.Fail<ArticleDto>(result.Error ?? "Failed to update article");
			}

			var updatedDto = new ArticleDto(
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
					!article.IsArchived,
					article.Version
			);

			logger.LogInformation("EditArticle: Successfully updated article with ID: {Id}", article.Id);

			return Result.Ok(updatedDto);
		}

	}

}
