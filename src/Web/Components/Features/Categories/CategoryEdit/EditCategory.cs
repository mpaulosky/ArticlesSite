// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryEdit;

/// <summary>
///   Contains functionality for editing a category within the system.
/// </summary>
public static class EditCategory
{

	public interface IEditCategoryHandler
	{

		Task<Result> HandleAsync(CategoryDto? request);

	}

	public class Handler : IEditCategoryHandler
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

		public async Task<Result> HandleAsync(CategoryDto? request)
		{

			if (request is null)
			{
				_logger.LogError("The request is null.");

				return Result.Fail("The request is null.");
			}

			if (string.IsNullOrWhiteSpace(request.CategoryName))
			{
				_logger.LogError("Category name cannot be empty or whitespace.");

				return Result.Fail("Category name cannot be empty or whitespace.");
			}

			if (request.Id == ObjectId.Empty)
			{
				_logger.LogError("Category ID is invalid or empty.");

				return Result.Fail("Category ID is invalid or empty.");
			}

			try
			{

				Result<Category?> getResult = await _repository.GetCategoryByIdAsync(request.Id);

				if (!getResult.Success || getResult.Value == null)
				{
					_logger.LogWarning("No category found with ID: {CategoryId}", request.Id);

					return Result.Fail(getResult.Error ?? "Category not found.");
				}

				Category? category = getResult.Value;
				category.CategoryName = request.CategoryName;
				category.ModifiedOn = DateTime.UtcNow;

				Result<Category> updateResult = await _repository.UpdateCategory(category);

				if (!updateResult.Success)
				{
					_logger.LogError("Failed to update category: {CategoryName}", request.CategoryName);

					return Result.Fail(updateResult.Error ?? "Failed to update category.");
				}

				_logger.LogInformation("Category updated successfully: {CategoryName}", request.CategoryName);

				return Result.Ok();

			}
			catch (Exception ex)
			{

				_logger.LogError(ex, "Failed to update category: {CategoryName}", request.CategoryName);

				return Result.Fail(ex.Message);

			}

		}

	}

}
