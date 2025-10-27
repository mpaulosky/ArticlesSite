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

	public interface IGetArticlesHandler
	{
		Task<Result<IEnumerable<ArticleDto>>> HandleAsync();
	}

	public class Handler : IGetArticlesHandler
	{

		private readonly IArticleRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(IArticleRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<IEnumerable<ArticleDto>>> HandleAsync()
		{
			Result<IEnumerable<Article>?> result = await _repository.GetArticles();

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetArticles: Failed to retrieve articles. Error: {Error}", result.Error);
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

			_logger.LogInformation("GetArticles: Successfully retrieved {Count} articles", dtos.Count);
			return Result.Ok<IEnumerable<ArticleDto>>(dtos);
		}

	}

}
