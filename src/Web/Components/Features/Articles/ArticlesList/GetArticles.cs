// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticles.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticlesList;

/// <summary>
///   Static class providing functionality for article creation.
/// </summary>
public static class GetArticles
{

	/// <summary>
	///   Interface for retrieving articles from the database.
	/// </summary>
	public interface IGetArticlesHandler
	{

		Task<Result<IEnumerable<ArticleDto>>> HandleAsync(bool excludeArchived = false);

	}

	/// <summary>
	///   Represents a handler for retrieving articles from the database.
	/// </summary>
	public class Handler : IGetArticlesHandler
	{

		private readonly ILogger<Handler> _logger;

		private readonly IArticleRepository _repository;

		/// <summary>
		///   Initializes a new instance of the <see cref="Handler" /> class.
		/// </summary>
		/// <param name="repository">The article repository.</param>
		/// <param name="logger">The logger instance.</param>
		public Handler(IArticleRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		/// <summary>
		///   Handles retrieving all articles asynchronously, with an option to exclude archived articles.
		/// </summary>
		/// <param name="excludeArchived">If true, excludes articles where Archived is true.</param>
		/// <returns>A <see cref="Result" /> representing the outcome of the operation.</returns>
		public async Task<Result<IEnumerable<ArticleDto>>> HandleAsync(bool excludeArchived = false)
		{
			try
			{

				Result<IEnumerable<Article>?> result = await _repository.GetArticles();

				if (!result.Success || result.Value is null)
				{
					_logger.LogWarning("No articles found.");

					return Result<IEnumerable<ArticleDto>>.Fail(result.Error ?? "No articles found.");
				}

				List<Article> articles = result.Value.ToList();

				if (articles.Count == 0)
				{
					_logger.LogWarning("No articles found.");

					return Result<IEnumerable<ArticleDto>>.Fail("No articles found.");
				}

				_logger.LogInformation("Articles retrieved successfully. Count: {Count}", articles.Count);

				return Result<IEnumerable<ArticleDto>>.Ok(articles.Select(a => new ArticleDto(
						a.Id, a.Slug, a.Title, a.Introduction, a.Content, a.CoverImageUrl,
						a.Author, a.Category, a.IsPublished, a.PublishedOn, a.CreatedOn, a.ModifiedOn, a.IsArchived, false
				)));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve articles.");

				return Result<IEnumerable<ArticleDto>>.Fail(ex.Message);
			}
		}

	}

}
