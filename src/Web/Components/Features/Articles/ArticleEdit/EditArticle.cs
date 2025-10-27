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

	public interface IEditArticleHandler
	{
		Task<Result<ArticleDto>> HandleAsync(ArticleDto dto);
	}

	public class Handler : IEditArticleHandler
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
				_logger.LogWarning("EditArticle: Article DTO cannot be null");
				return Result.Fail<ArticleDto>("Article data cannot be null");
			}

			Result<Article?> existingResult = await _repository.GetArticleByIdAsync(dto.Id);

			if (existingResult.Failure || existingResult.Value is null)
			{
				_logger.LogWarning("EditArticle: Article not found with ID: {Id}", dto.Id);
				return Result.Fail<ArticleDto>(existingResult.Error ?? "Article not found");
			}

			Article article = existingResult.Value;

			article.Update(
				dto.Title,
				dto.Introduction,
				dto.Content,
				dto.CoverImageUrl,
				dto.IsPublished,
				dto.PublishedOn,
				dto.IsArchived
			);

			article.Author = dto.Author;
			article.Category = dto.Category;

			Result<Article> result = await _repository.UpdateArticle(article);

			if (result.Failure)
			{
				_logger.LogWarning("EditArticle: Failed to update article. Error: {Error}", result.Error);
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
				!article.IsArchived
			);

			_logger.LogInformation("EditArticle: Successfully updated article with ID: {Id}", article.Id);
			return Result.Ok(updatedDto);
		}

	}

}
