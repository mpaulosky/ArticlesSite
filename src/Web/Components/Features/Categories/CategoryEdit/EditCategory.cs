// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryEdit;

public static class EditCategory
{

	/// <summary>
	/// Interface for handling category edit operations.
	/// </summary>
	public interface IEditCategoryHandler
	{

		/// <summary>
		/// Handles the edit operation for a category.
		/// </summary>
		/// <param name="dto">The category DTO containing updated data.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome of the edit operation.</returns>
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);

	}

	/// <summary>
	/// Command handler for editing a category.
	/// </summary>
	public class Handler
	(
			ICategoryRepository repository,
			ILogger<Handler> logger
	) : IEditCategoryHandler
	{

		/// <inheritdoc />
		public async Task<Result<CategoryDto>> HandleAsync(CategoryDto dto)
		{
			if (dto is null)
			{
				logger.LogWarning("EditCategory: Category DTO cannot be null");

				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

			if (string.IsNullOrWhiteSpace(dto.CategoryName))
			{
				logger.LogWarning("EditCategory: Category name cannot be empty");

				return Result.Fail<CategoryDto>("Category name is required");
			}

			Result<Category> existingResult = await repository.GetCategoryByIdAsync(dto.Id);

			if (existingResult.Failure)
			{
				logger.LogWarning("EditCategory: Category not found with ID: {Id}", dto.Id);

				return Result.Fail<CategoryDto>(existingResult.Error ?? "Category not found");
			}

			if (existingResult.Value is null)
			{
				logger.LogWarning("EditCategory: Category not found with ID: {Id}", dto.Id);

				return Result.Fail<CategoryDto>("Category not found");
			}

			Category category = existingResult.Value;
			string slug = category.CategoryName.GenerateSlug();
			category.Update(dto.CategoryName, slug, dto.IsArchived);
			Result<Category> result = await repository.UpdateCategory(category);

			if (result.Failure)
			{
				logger.LogWarning("EditCategory: Failed to update category. Error: {Error}", result.Error);

				return Result.Fail<CategoryDto>(result.Error ?? "Failed to update category");
			}

			var updatedDto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				Slug = category.Slug ?? string.Empty,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			logger.LogInformation("EditCategory: Successfully updated category with ID: {Id}", category.Id);

			return Result.Ok(updatedDto);
		}

	}

}
