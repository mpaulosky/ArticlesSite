// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateArticle.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.ArticleCreate;

/// <summary>
///   Static class providing functionality for article creation.
/// </summary>
public static class CreateArticle
{

	/// <summary>
	///   Interface for creating articles in the database.
	/// </summary>
	public interface ICreateArticleHandler
	{

		Task<Result> HandleAsync(ArticleDto? request);

	}

	/// <summary>
	///   Represents a handler for creating new articles in the database.
	/// </summary>
	public class Handler : ICreateArticleHandler
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
		///   Handles the creation of a new article asynchronously.
		/// </summary>
		/// <param name="request">The article DTO.</param>
		/// <returns>A <see cref="Result" /> indicating success or failure.</returns>
		public async Task<Result> HandleAsync(ArticleDto? request)
		{

			if (request is null)
			{
				return Result.Fail("The request is null.");
			}

			try
			{
				// Manually map DTO to an entity to avoid relying on Mapster in unit tests
				Article article = new(
						request.Title,
						request.Introduction,
						request.Content,
						request.CoverImageUrl,
						request.Author, // Pass AuthorInfo? directly
						request.Category, // Pass Category? directly
						request.IsPublished,
						request.PublishedOn,
						request.IsArchived
				);

				var result = await _repository.AddArticle(article);

				if (!result.Success)
				{
					_logger.LogError("Failed to create article: {Title}", request.Title);
					return Result.Fail(result.Error ?? "Failed to create article.");
				}

				_logger.LogInformation("Article created successfully: {Title}", request.Title);

				return Result.Ok();
			}
			catch (Exception ex)
			{

				_logger.LogError(ex, "Failed to create article");

				return Result.Fail("An error occurred while creating the article: " + ex.Message);

			}

		}

	}

}