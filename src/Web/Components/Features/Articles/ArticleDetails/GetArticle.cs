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

	public interface IGetArticleHandler
	{
		Task<Result<ArticleDto>> HandleAsync(string id);
	}

	public class Handler : IGetArticleHandler
	{

		private readonly IArticleRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(IArticleRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<ArticleDto>> HandleAsync(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				_logger.LogWarning("GetArticle: Invalid article ID provided");
				return Result.Fail<ArticleDto>("Article ID cannot be empty");
			}

			if (!ObjectId.TryParse(id, out ObjectId objectId))
			{
				_logger.LogWarning("GetArticle: Invalid ObjectId format: {Id}", id);
				return Result.Fail<ArticleDto>("Invalid article ID format");
			}

			Result<Article?> result = await _repository.GetArticleByIdAsync(objectId);

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetArticle: Article not found with ID: {Id}", id);
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

			_logger.LogInformation("GetArticle: Successfully retrieved article {Id}", id);
			return Result.Ok(dto);
		}

	}

}
