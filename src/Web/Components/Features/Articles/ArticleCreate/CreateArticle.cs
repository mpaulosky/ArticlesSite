// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleCreate;

public static class CreateArticle
{

	/// <summary>
	/// Interface for handling article creation operations.
	/// </summary>
	public interface ICreateArticleHandler
	{

		/// <summary>
		/// Handles creation of an article.
		/// </summary>
		/// <param name="dto">The article DTO containing data.</param>
		/// <returns>A <see cref="Result{ArticleDto}"/> representing the outcome.</returns>
		Task<Result<ArticleDto>> HandleAsync(ArticleDto dto);

	}

	/// <summary>
	/// Command handler for creating an article.
	/// </summary>
	public class Handler : ICreateArticleHandler
	{
		private readonly IArticleRepository _repository;
		private readonly ILogger<Handler> _logger;
		private readonly IValidator<ArticleDto>? _validator;

		public Handler(IArticleRepository repository, ILogger<Handler> logger, IValidator<ArticleDto>? validator)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<Handler>.Instance;
			_validator = validator;
		}

		/// <inheritdoc />
		public async Task<Result<ArticleDto>> HandleAsync(ArticleDto dto)
		{

			if (dto is null)
			{
				_logger.LogWarning("CreateArticle: Article DTO cannot be null");
				return Result.Fail<ArticleDto>("Article data cannot be null");
			}

			if (_validator is not null)
			{
				var validation = await _validator.ValidateAsync(dto);
				if (!validation.IsValid)
				{
					string errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
					_logger.LogWarning("CreateArticle: Validation failed for article {Id}: {Errors}", dto.Id, errors);
					return Result.Fail<ArticleDto>(errors);
				}
			}

			var article = new Article(
				dto.Title,
				dto.Introduction,
				dto.Content,
				dto.CoverImageUrl,
				dto.Author,
				dto.Category,
				dto.IsPublished,
				dto.PublishedOn,
				false,
				dto.Slug // Added missing 'slug' argument
			);

			Result<Article> result = await _repository.AddArticle(article);

			if (result.Failure)
			{
				_logger.LogWarning("CreateArticle: Failed to create article. Error: {Error}", result.Error);
				return Result.Fail<ArticleDto>(result.Error ?? "Failed to create article");
			}

			var createdDto = new ArticleDto(
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
				true,
				article.Version
			);

			_logger.LogInformation("CreateArticle: Successfully created article with ID: {Id}", article.Id);
			return Result.Ok(createdDto);

		}

	}

}
