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

	public interface IEditCategoryHandler
	{
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);
	}

	public class Handler : IEditCategoryHandler
	{

		private readonly ICategoryRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(ICategoryRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<CategoryDto>> HandleAsync(CategoryDto dto)
		{
			if (dto is null)
			{
				_logger.LogWarning("EditCategory: Category DTO cannot be null");
				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

			if (string.IsNullOrWhiteSpace(dto.CategoryName))
			{
				_logger.LogWarning("EditCategory: Category name cannot be empty");
				return Result.Fail<CategoryDto>("Category name is required");
			}

			Result<Category?> existingResult = await _repository.GetCategoryByIdAsync(dto.Id);

			if (existingResult.Failure || existingResult.Value is null)
			{
				_logger.LogWarning("EditCategory: Category not found with ID: {Id}", dto.Id);
				return Result.Fail<CategoryDto>(existingResult.Error ?? "Category not found");
			}

			Category category = existingResult.Value;

			string slug = GenerateSlug(dto.CategoryName);
			category.Update(dto.CategoryName, slug, dto.IsArchived);

			Result<Category> result = await _repository.UpdateCategory(category);

			if (result.Failure)
			{
				_logger.LogWarning("EditCategory: Failed to update category. Error: {Error}", result.Error);
				return Result.Fail<CategoryDto>(result.Error ?? "Failed to update category");
			}

			var updatedDto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			_logger.LogInformation("EditCategory: Successfully updated category with ID: {Id}", category.Id);
			return Result.Ok(updatedDto);
		}

		private static string GenerateSlug(string categoryName)
		{
			return categoryName
				.ToLowerInvariant()
				.Replace(" ", "_")
				.Replace("-", "_");
		}

	}

}
