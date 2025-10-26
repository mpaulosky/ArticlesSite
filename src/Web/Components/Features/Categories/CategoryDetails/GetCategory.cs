// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryDetails;

/// <summary>
///   Static class providing functionality for category creation.
/// </summary>
public static class GetCategory
{

	public interface IGetCategoryHandler
	{

		Task<Result<CategoryDto>> HandleAsync(string id);

	}

	/// <summary>
	///   Represents a handler for retrieving categories from the database.
	/// </summary>
	public class Handler : IGetCategoryHandler
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
		///   Handles retrieving a category asynchronously by its ObjectId.
		/// </summary>
		/// <param name="id">The category ObjectId as a string.</param>
		/// <returns>A <see cref="Result" /> representing the outcome of the operation.</returns>
		public async Task<Result<CategoryDto>> HandleAsync(string id)
		{

			if (string.IsNullOrWhiteSpace(id) || !ObjectId.TryParse(id, out ObjectId objectId))
			{
				_logger.LogError("The ID is invalid or empty.");

				return Result.Fail<CategoryDto>("The ID is invalid or empty.");
			}

			try
			{

				Result<Category?> result = await _repository.GetCategoryByIdAsync(objectId);

				if (!result.Success || result.Value is null)
				{
					_logger.LogWarning("Category not found: {CategoryId}", id);

					return Result.Fail<CategoryDto>(result.Error ?? "Category not found.");
				}

				_logger.LogInformation("Category was found successfully: {CategoryId}", id);

				return Result<CategoryDto>.Ok(new CategoryDto
				{
						Id = result.Value.Id,
						CategoryName = result.Value.CategoryName,
						CreatedOn = result.Value.CreatedOn ?? DateTimeOffset.UtcNow,
						ModifiedOn = result.Value.ModifiedOn,
						IsArchived = result.Value.IsArchived
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to find the category: {CategoryId}", id);

				return Result<CategoryDto>.Fail(ex.Message);
			}
		}

	}

}
