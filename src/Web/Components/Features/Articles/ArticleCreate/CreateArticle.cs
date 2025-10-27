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

	public interface ICreateArticleHandler
	{
		Task<Result<ArticleDto>> HandleAsync(ArticleDto dto);
	}

	public class Handler : ICreateArticleHandler
	{

		private readonly IArticleRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(IArticleRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<ArticleDto>> HandleAsync(ArticleDto dto)
		{
			if (dto is null)
			{
				_logger.LogWarning("CreateArticle: Article DTO cannot be null");
				return Result.Fail<ArticleDto>("Article data cannot be null");
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
				false
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
				true
			);

			_logger.LogInformation("CreateArticle: Successfully created article with ID: {Id}", article.Id);
			return Result.Ok(createdDto);
		}

	}

}
