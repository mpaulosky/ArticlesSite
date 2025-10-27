// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryCreate;

public static class CreateCategory
{
	/// <summary>
	/// Interface for handling category creation operations.
	/// </summary>
	public interface ICreateCategoryHandler
	{
		/// <summary>
		/// Handles creation of a category.
		/// </summary>
		/// <param name="dto">The category DTO containing data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);
	}

	/// <summary>
	/// Command handler for creating a category.
	/// </summary>
	public class Handler(ICategoryRepository repository, ILogger<Handler> logger) : ICreateCategoryHandler
	{
		/// <summary>
		/// Handles creation of a category.
		/// </summary>
		/// <param name="dto">The category DTO containing data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		public async Task<Result<CategoryDto>> HandleAsync(CategoryDto dto)
		{
			if (dto is null)
			{
				logger.LogWarning("CreateCategory: Category DTO cannot be null");
				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

			if (string.IsNullOrWhiteSpace(dto.CategoryName))
			{
				logger.LogWarning("CreateCategory: Category name cannot be empty");
				return Result.Fail<CategoryDto>("Category name is required");
			}

			var category = new Category
			{
				Id = ObjectId.GenerateNewId(),
				CategoryName = dto.CategoryName,
				Slug = GenerateSlug(dto.CategoryName),
				CreatedOn = DateTimeOffset.UtcNow,
				ModifiedOn = null,
				IsArchived = false
			};

			Result<Category> result = await repository.AddCategory(category);

			if (result.Failure)
			{
				logger.LogWarning("CreateCategory: Failed to create category. Error: {Error}", result.Error);
				return Result.Fail<CategoryDto>(result.Error ?? "Failed to create category");
			}

			var createdDto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			logger.LogInformation("CreateCategory: Successfully created category with ID: {Id}", category.Id);
			return Result.Ok(createdDto);
		}

		/// <summary>
		/// Generates a slug from the category name.
		/// </summary>
		/// <param name="categoryName">The category name.</param>
		/// <returns>A slug-friendly string.</returns>
		private static string GenerateSlug(string categoryName)
		{
			return categoryName
				.ToLowerInvariant()
				.Replace(" ", "_")
				.Replace("-", "_");
		}
	}
}

