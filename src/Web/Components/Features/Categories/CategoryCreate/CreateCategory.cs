// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryCreate;

/// <summary>
///   Static class providing functionality for category creation.
/// </summary>
public static class CreateCategory
{

	public interface ICreateCategoryHandler
	{

		Task<Result> HandleAsync(CategoryDto? request);

	}

	/// <summary>
	///   Represents a handler for creating new categories in the database.
	/// </summary>
	public class Handler : ICreateCategoryHandler
	{

		private readonly ILogger<Handler> _logger;

		private readonly ICategoryRepository _repository;

		/// <summary>
		///   Initializes a new instance of the <see cref="Handler" /> class.
		/// </summary>
		/// <param name="repository">The category repository.</param>
		/// <param name="logger">The logger instance.</param>
		public Handler(ICategoryRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		/// <summary>
		///   Handles the creation of a new category asynchronously.
		/// </summary>
		/// <param name="request">The category DTO.</param>
		/// <returns>A <see cref="Result" /> indicating success or failure.</returns>
		public async Task<Result> HandleAsync(CategoryDto? request)
		{

			if (request is null)
			{
				_logger.LogError("The request is null.");

				return Result.Fail("The request is null.");
			}

			try
			{

				Category category = new() { CategoryName = request.CategoryName };

				Result<Category> result = await _repository.AddCategory(category);

				if (!result.Success)
				{
					_logger.LogError("Failed to create category: {CategoryName}", request.CategoryName);

					return Result.Fail(result.Error ?? "Failed to create category.");
				}

				_logger.LogInformation("Category created successfully: {CategoryName}", request.CategoryName);

				return Result.Ok();
			}
			catch (Exception ex)
			{

				_logger.LogError(ex, "Failed to create category: {CategoryName}", request.CategoryName);

				return Result.Fail("An error occurred while creating the category: " + ex.Message);

			}
		}

	}

}
