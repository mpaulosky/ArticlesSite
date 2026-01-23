// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using Shared.Abstractions;
using Web.Infrastructure;

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
			ILogger<Handler> logger,
			IValidator<ArticleDto> validator,
			Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions> concurrencyOptions
	) : IEditArticleHandler
	{
			private readonly ConcurrencyOptions _concurrencyOptions = concurrencyOptions?.Value ?? new ConcurrencyOptions();

		/// <inheritdoc />
		public async Task<Result<ArticleDto>> HandleAsync(ArticleDto? dto)
		{
			if (dto == null)
			{
				logger.LogWarning("EditArticle: Article DTO cannot be null");

				return Result.Fail<ArticleDto>("Article data cannot be null");
			}

			// Run validator if provided
			if (validator != null)
			{
				var validation = await validator.ValidateAsync(dto);
				if (!validation.IsValid)
				{
					string errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
					logger.LogWarning("EditArticle: Validation failed for article {Id}: {Errors}", dto.Id, errors);
					return Result.Fail<ArticleDto>(errors);
				}
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

			// Optimistic concurrency: retry a few times if we detect a concurrency conflict
			int attempt = 0;
			Result<Article> result;
			do
			{
				result = await repository.UpdateArticle(article);
				if (result.Failure && result.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					attempt++;
					if (attempt >= _concurrencyOptions.MaxRetries)
					{
						// give up and return the concurrency failure
						logger.LogWarning("EditArticle: Concurrency conflict after {Attempts} attempts for article {Id}", attempt, article.Id);
						return Result.Fail<ArticleDto>(result.Error ?? "Concurrency conflict: article was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency);
					}

					// Backoff before retrying: exponential backoff with jitter
					int baseMs = _concurrencyOptions.BaseDelayMilliseconds;
					int capMs = _concurrencyOptions.MaxDelayMilliseconds;
					int exponential = baseMs * (int)Math.Pow(2, attempt - 1);
					int delayMs = Math.Min(capMs, exponential);
					int jitter = Random.Shared.Next(0, _concurrencyOptions.JitterMilliseconds);
					int totalDelay = delayMs + jitter;
					logger.LogDebug("EditArticle: Concurrency conflict detected, retrying attempt {Attempt} after {Delay}ms for article {Id}", attempt, totalDelay, article.Id);
					await Task.Delay(totalDelay);

					// Reload latest article and reapply changes
					var latest = await repository.GetArticleByIdAsync(article.Id);
					if (latest.Failure || latest.Value is null)
					{
						return Result.Fail<ArticleDto>(latest.Error ?? "Article not found");
					}

					article = latest.Value;
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
						logger.LogWarning(ex, "EditArticle: Validation failed during retry update for article {Id}", dto.Id);
						return Result.Fail<ArticleDto>(ex.Message);
					}

					article.Author = dto.Author;
					article.Category = dto.Category;
					// loop and try update again
				}
				else
				{
					// either success or a non-concurrency failure
					break;
				}
			} while (true);

			if (result.Failure)
			{
				// If the repository indicates a failure, propagate a meaningful failure
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
