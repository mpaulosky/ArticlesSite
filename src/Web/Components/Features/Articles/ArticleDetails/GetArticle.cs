// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleDetails;

/// <summary>
///   Static class providing functionality for article creation.
/// </summary>
public static class GetArticle
{

	/// <summary>
	///   Interface for retrieving articles from the database.
	/// </summary>
	public interface IGetArticleHandler
	{

		Task<Result<ArticleDto>> HandleAsync(string id);

	}

	/// <summary>
	///   Represents a handler for retrieving articles from the database.
	/// </summary>
	public class Handler : IGetArticleHandler
	{

		private readonly IArticleRepository _repository;

		private readonly ILogger<Handler> _logger;

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
		///   Handles retrieving an article asynchronously by its ObjectId.
		/// </summary>
		/// <param name="id">The article ObjectId as a string.</param>
		/// <returns>A <see cref="Result" /> representing the outcome of the operation.</returns>
		public async Task<Result<ArticleDto>> HandleAsync(string id)
		{

			if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out var objectId))
			{

				_logger.LogError("The ID is invalid or empty.");

				return Result.Fail<ArticleDto>("The ID is invalid or empty.");
			}

			try
			{

				var result = await _repository.GetArticleByIdAsync(objectId);

				if (!result.Success || result.Value is null)
				{
					_logger.LogWarning("Article not found: {ArticleId}", id);

					return Result.Fail<ArticleDto>(result.Error ?? "Article not found.");
				}

				_logger.LogInformation("Article was found successfully: {ArticleId}", id);
				ArticleDto dto = new(
					result.Value.Id,
					result.Value.Slug,
					result.Value.Title,
					result.Value.Introduction,
					result.Value.Content,
					result.Value.CoverImageUrl,
					result.Value.Author,
					result.Value.Category,
					result.Value.IsPublished,
					result.Value.PublishedOn,
					result.Value.CreatedOn,
					result.Value.ModifiedOn,
					result.Value.IsArchived,
					false
				); 
				
				return Result.Ok(dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to find the article: {ArticleId}", id);

				return Fail(ex.Message);
			}
		}

	}

}