// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Abstractions;
using Web.Infrastructure;
using Polly;
using Polly.Registry;
using System.Diagnostics;

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
	public class Handler : IEditArticleHandler
	{
		private readonly IArticleRepository repository;
		private readonly ILogger<Handler> logger;
		private readonly IValidator<ArticleDto>? validator;
		private readonly ConcurrencyOptions _concurrencyOptions;
		private readonly IAsyncPolicy<Result<Web.Components.Features.Articles.Entities.Article>> _concurrencyPolicy;
		private readonly IMetricsPublisher _metrics;

		public Handler(IArticleRepository repository, ILogger<Handler> logger, IValidator<ArticleDto>? validator, Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions> concurrencyOptions, IAsyncPolicy<Result<Web.Components.Features.Articles.Entities.Article>>? concurrencyPolicy = null, IMetricsPublisher? metrics = null)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			this.logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Handler>.Instance;
			this.validator = validator;
			_concurrencyOptions = concurrencyOptions?.Value ?? new ConcurrencyOptions();
			_concurrencyPolicy = concurrencyPolicy ?? Web.Infrastructure.ConcurrencyPolicies.CreatePolicy(_concurrencyOptions);
			_metrics = metrics ?? new Web.Infrastructure.NoOpMetricsPublisher();
		}

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

			var article = existingResult.Value;

			try
			{
				article.Update(dto.Title, dto.Introduction, dto.Content, dto.CoverImageUrl, dto.IsPublished, dto.PublishedOn, dto.IsArchived);
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

			// Use centralized Polly policy to execute update with retries. We provide an onRetryAction via Context
			var context = new Polly.Context();
			context["onRetryAction"] = (Func<Task>)(async () =>
			{
				logger.LogDebug("EditArticle: onRetryAction invoked for article {Id}", article.Id);
				var latest = await repository.GetArticleByIdAsync(article.Id);
				if (latest.Failure || latest.Value is null)
				{
					context["terminalFailure"] = Result.Fail<Article>(latest.Error ?? "Article not found");
					return;
				}

				article = latest.Value;
				try
				{
					article.Update(dto.Title, dto.Introduction, dto.Content, dto.CoverImageUrl, dto.IsPublished, dto.PublishedOn, dto.IsArchived);
				}
				catch (ArgumentException ex)
				{
					context["terminalFailure"] = Result.Fail<Article>(ex.Message);
					return;
				}
				article.Author = dto.Author;
				article.Category = dto.Category;
			});

			// Before executing policy, increment attempt
			_metrics.IncrementAttempt();

			var stopwatch = Stopwatch.StartNew();
			Result<Web.Components.Features.Articles.Entities.Article> policyResult;
			try
			{
				policyResult = await _concurrencyPolicy.ExecuteAsync((ctx) => repository.UpdateArticle(article), context);
			}
			finally
			{
				stopwatch.Stop();
				_metrics.RecordRequestLatency("ArticleUpdate", stopwatch.Elapsed);
			}

			// If the policy context contains a 'retryCount' value, record it
			if (context.TryGetValue("retryCount", out var rcObj) && rcObj is int rc)
			{
				_metrics.RecordRetryCount(rc);
			}

			// After retries check metrics
			if (context.TryGetValue("terminalFailure", out var term) && term is Result<Web.Components.Features.Articles.Entities.Article> terminal && terminal.Failure)
			{
				_metrics.IncrementConflict();
				if (terminal.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					return Result.Fail<ArticleDto>(terminal.Error ?? "Concurrency conflict: article was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency, terminal.Details);
				}
				return Result.Fail<ArticleDto>(terminal.Error ?? "Failed to update article");
			}

			if (policyResult.Failure)
			{
				_metrics.IncrementConflict();
				if (policyResult.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					return Result.Fail<ArticleDto>(policyResult.Error ?? "Concurrency conflict: article was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency, policyResult.Details);
				}
				return Result.Fail<ArticleDto>(policyResult.Error ?? "Failed to update article");
			}
			else
			{
				_metrics.IncrementSuccess();
			}

			// Note: onRetryAction is executed by the policy's onRetryAsync via the provided context; we already increment retry inside policy's onRetry by adding a retryCount in context if needed

			var updatedDto = new ArticleDto(article.Id, article.Slug, article.Title, article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, article.ModifiedOn, article.IsArchived, !article.IsArchived, article.Version);

			logger.LogInformation("EditArticle: Successfully updated article with ID: {Id}", article.Id);
			return Result.Ok(updatedDto);
		}
	}
}
