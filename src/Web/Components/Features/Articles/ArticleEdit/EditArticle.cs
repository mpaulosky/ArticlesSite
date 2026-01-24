// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

// Removed redundant usings: common namespaces are in Web/GlobalUsings.cs

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
		private readonly Web.Infrastructure.ConcurrencyOptions _concurrencyOptions;
		private readonly IAsyncPolicy<Result<Web.Components.Features.Articles.Entities.Article>> _concurrencyPolicy;
		private readonly Web.Infrastructure.IMetricsPublisher _metrics;

		public Handler(IArticleRepository repository, ILogger<Handler> logger, IValidator<ArticleDto>? validator, Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions> concurrencyOptions, IAsyncPolicy<Result<Web.Components.Features.Articles.Entities.Article>>? concurrencyPolicy = null, Web.Infrastructure.IMetricsPublisher? metrics = null)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			this.logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Handler>.Instance;
			this.validator = validator;
			_concurrencyOptions = concurrencyOptions?.Value ?? new Web.Infrastructure.ConcurrencyOptions();
			_concurrencyPolicy = concurrencyPolicy ?? Web.Infrastructure.ConcurrencyPolicies.CreatePolicy(_concurrencyOptions);
			_metrics = metrics ?? new Web.Infrastructure.NoOpMetricsPublisher();
		}

		// Backwards-compatible constructor for tests and callers that previously used the older signature
		public Handler(IArticleRepository repository, ILogger<Handler> logger, IValidator<ArticleDto>? validator)
				: this(repository, logger, validator, Microsoft.Extensions.Options.Options.Create(new Web.Infrastructure.ConcurrencyOptions()), null, null)
		{
		}

		/// <inheritdoc />
		public async Task<Result<ArticleDto>> HandleAsync(ArticleDto? dto)
		{
			if (dto == null)
			{
				logger.LogInformation("EditArticle: Article DTO cannot be null");
				return Result.Fail<ArticleDto>("Article data cannot be null");
			}

			// Run validator if provided; always ensure concrete validator rules are applied
			// Ensure the target article exists before applying validation; tests expect NotFound when editing non-existent ids
			Result<Article?> existingResult = await repository.GetArticleByIdAsync(dto.Id);

			if (existingResult.Failure || existingResult.Value is null)
			{
				logger.LogInformation("EditArticle: Article not found with ID: {Id}", dto.Id);
				Console.Error.WriteLine($"[SERVER-LOG] EditArticle: Article not found ID={dto.Id} Error={existingResult.Error}");
				return Result.Fail<ArticleDto>(existingResult.Error ?? "Article not found");
			}

			// Run validator if provided; always ensure concrete validator rules are applied
			var concreteArticleValidator = new Web.Components.Features.Articles.Validators.ArticleDtoValidator();
			FluentValidation.Results.ValidationResult concreteValidation = concreteArticleValidator.Validate(dto) ?? new FluentValidation.Results.ValidationResult();

			if (validator != null)
			{
				var injectedValidation = await validator.ValidateAsync(dto);
				if (injectedValidation != null && !injectedValidation.IsValid)
				{
					var errors = string.Join("; ", injectedValidation.Errors.Select(e => e.ErrorMessage));
					logger.LogInformation("EditArticle: Validation failed for article {Id}: {Errors}", dto.Id, errors);
					Console.Error.WriteLine($"[SERVER-LOG] EditArticle: injected validation failed ID={dto.Id} Errors={errors}");
					return Result.Fail<ArticleDto>(errors);
				}
			}

			if (!concreteValidation.IsValid)
			{
				var errors = string.Join("; ", concreteValidation.Errors.Select(e => e?.ErrorMessage));
				logger.LogInformation("EditArticle: Validation failed for article {Id}: {Errors}", dto.Id, errors);
				Console.Error.WriteLine($"[SERVER-LOG] EditArticle: concrete validation failed ID={dto.Id} Errors={errors}");
				return Result.Fail<ArticleDto>(errors);
			}

			var article = existingResult.Value;

			try
			{
				article.Update(dto.Title, dto.Introduction, dto.Content, dto.CoverImageUrl, dto.IsPublished, dto.PublishedOn, dto.IsArchived);
			}
			catch (ArgumentException ex)
			{
				logger.LogInformation(ex, "EditArticle: Validation failed during update for article {Id}", dto.Id);
				Console.Error.WriteLine($"[SERVER-LOG] EditArticle: Validation exception ID={dto.Id} Message={ex.Message}");
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
				logger.LogInformation("EditArticle.onRetryAction invoked for article {Id}. Current local Version={Version}", article.Id, article.Version);
				var latest = await repository.GetArticleByIdAsync(article.Id);
				if (latest.Failure || latest.Value is null)
				{
					context["terminalFailure"] = Result.Fail<Article>(latest.Error ?? "Article not found");
					return;
				}

				article = latest.Value;
				logger.LogInformation("EditArticle.onRetryAction loaded latest article {Id} Version={Version}", article.Id, article.Version);
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

			logger.LogInformation("EditArticle.HandleAsync: Calling UpdateArticle for article {Id} with Version={Version} DTO.Version={DtoVersion}", article.Id, article.Version, dto.Version);
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			Result<Web.Components.Features.Articles.Entities.Article> policyResult;
			try
			{
				logger.LogDebug("EditArticle: Executing concurrency policy for article {Id} (attempt)", article.Id);
				policyResult = await _concurrencyPolicy.ExecuteAsync((ctx) => repository.UpdateArticle(article), context);
				logger.LogDebug("EditArticle: Concurrency policy completed for article {Id}. Success={Success} ErrorCode={ErrorCode}", article.Id, !policyResult.Failure, policyResult.ErrorCode);
				if (!policyResult.Failure && policyResult.Value is not null)
				{
					logger.LogDebug("EditArticle: Repository returned article version {Version} for article {Id}", policyResult.Value.Version, article.Id);
				}
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
				Console.Error.WriteLine($"[SERVER-LOG] EditArticle: terminal failure for ID={article.Id} Error={terminal.Error} Code={terminal.ErrorCode}");
				if (terminal.ErrorCode == global::Shared.Abstractions.ResultErrorCode.Concurrency)
				{
					return Result.Fail<ArticleDto>(terminal.Error ?? "Concurrency conflict: article was modified by another process", global::Shared.Abstractions.ResultErrorCode.Concurrency, terminal.Details);
				}
				return Result.Fail<ArticleDto>(terminal.Error ?? "Failed to update article");
			}

			if (policyResult.Failure)
			{
				_metrics.IncrementConflict();
				Console.Error.WriteLine($"[SERVER-LOG] EditArticle: policyResult failure for ID={article.Id} Error={policyResult.Error} Code={policyResult.ErrorCode}");
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

			// Use the authoritative article returned by the repository/policy when available
			var authoritative = policyResult.Value ?? article;
			var updatedDto = new ArticleDto(authoritative.Id, authoritative.Slug, authoritative.Title, authoritative.Introduction, authoritative.Content, authoritative.CoverImageUrl, authoritative.Author, authoritative.Category, authoritative.IsPublished, authoritative.PublishedOn, authoritative.CreatedOn, authoritative.ModifiedOn, authoritative.IsArchived, !authoritative.IsArchived, authoritative.Version);

			logger.LogInformation("EditArticle: Successfully updated article with ID: {Id}", article.Id);
			Console.Error.WriteLine($"[SERVER-LOG] EditArticle: Success ID={article.Id} NewVersion={authoritative.Version}");
			return Result.Ok(updatedDto);
		}
	}
}
