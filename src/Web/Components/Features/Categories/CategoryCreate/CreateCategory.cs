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
	public class Handler(
		ICategoryRepository repository, 
		ILogger<Handler> logger,
		IValidator<CategoryDto> validator) : ICreateCategoryHandler
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

			// Server-side validation using FluentValidation
			var validationResult = await validator.ValidateAsync(dto);
			if (!validationResult.IsValid)
			{
				var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
				logger.LogWarning("CreateCategory: Validation failed. Errors: {Errors}", errors);
				return Result.Fail<CategoryDto>(errors);
			}

			var category = new Category
			{
				Id = ObjectId.GenerateNewId(),
				CategoryName = dto.CategoryName,
				Slug = dto.CategoryName.GenerateSlug(),
				CreatedOn = DateTimeOffset.UtcNow,
				ModifiedOn = null,
				IsArchived = dto.IsArchived
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
				Slug = category.Slug ?? string.Empty	,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			logger.LogInformation("CreateCategory: Successfully created category with ID: {Id}", category.Id);
			return Result.Ok(createdDto);
		}

	}
}

