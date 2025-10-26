// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategories.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoriesList;

/// <summary>
///   Static class providing functionality for category creation.
/// </summary>
public static class GetCategories
{

	/// <summary>
	///   Represents a handler for retrieving categories from the database.
	/// </summary>
	public interface IGetCategoriesHandler
	{

		Task<Result<IEnumerable<CategoryDto>>> HandleAsync(bool excludeArchived = false);

	}

	public class Handler : IGetCategoriesHandler
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
		///   Handles retrieving all categories asynchronously, with an option to exclude archived categories.
		/// </summary>
		/// <param name="excludeArchived">If true, excludes categories where Archived is true.</param>
		/// <returns>A <see cref="Result" /> representing the outcome of the operation.</returns>
		public async Task<Result<IEnumerable<CategoryDto>>> HandleAsync(bool excludeArchived = false)
		{
			try
			{

				Result<IEnumerable<Category>?> result = await _repository.GetCategories();

				if (!result.Success || result.Value is null)
				{
					_logger.LogWarning("No categories found.");

					return Result<IEnumerable<CategoryDto>>.Fail(result.Error ?? "No categories found.");
				}

				List<Category> categories = result.Value.ToList();

				if (categories.Count == 0)
				{
					_logger.LogWarning("No categories found.");

					return Result<IEnumerable<CategoryDto>>.Fail("No categories found.");
				}

				_logger.LogInformation("Categories retrieved successfully. Count: {Count}", categories.Count);

				return Result<IEnumerable<CategoryDto>>.Ok(categories.Select(c => new CategoryDto
				{
						Id = c.Id,
						CategoryName = c.CategoryName,
						CreatedOn = c.CreatedOn ?? DateTimeOffset.UtcNow,
						ModifiedOn = c.ModifiedOn,
						IsArchived = c.IsArchived
				}));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to retrieve categories.");

				return Result<IEnumerable<CategoryDto>>.Fail(ex.Message);
			}
		}

	}

}
