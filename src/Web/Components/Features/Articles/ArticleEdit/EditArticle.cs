// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleEdit;

/// <summary>
///   Contains functionality for editing an article within the system.
/// </summary>
public static class EditArticle
{

	/// <summary>
	///   Interface for editing articles in the database.
	/// </summary>
	public interface IEditArticleHandler
	{

		Task<Result> HandleAsync(ArticleDto? request);

	}

	public class Handler : IEditArticleHandler
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

		public async Task<Result> HandleAsync(ArticleDto? request)
		{

			if (request is null)
			{

				_logger.LogError("The request is null.");

				return Result.Fail("The request is null.");

			}

			try
			{

				// First check if the article exists
				var getResult = await _repository.GetArticleByIdAsync(request.Id);

				if (!getResult.Success || getResult.Value is null)
				{
					_logger.LogWarning("Article not found for update: {ArticleId}", request.Id);

					return Result.Fail(getResult.Error ?? "Article not found.");
				}

				var existingArticle = getResult.Value;

				// Update the existing article using the Update method which regenerates the slug
				existingArticle.Update(
					request.Title,
					request.Introduction,
					request.Content,
					request.CoverImageUrl,
					request.IsPublished,
					request.PublishedOn,
					request.IsArchived
				);
				existingArticle.Author = request.Author;
				existingArticle.Category = request.Category;

				var updateResult = await _repository.UpdateArticle(existingArticle); if (!updateResult.Success)
				{
					_logger.LogError("Failed to update article: {Title}", request.Title);
					return Result.Fail(updateResult.Error ?? "Failed to update article.");
				}

				_logger.LogInformation("Article updated successfully: {Title}", request.Title);

				return Result.Ok();

			}
			catch (Exception ex)
			{

				// Avoid dereferencing the request in the error path (it may be null).
				_logger.LogError(ex, "Failed to update article: {Title}", request?.Title ?? "Unknown");

				return Result.Fail(ex.Message);

			}

		}

	}

}