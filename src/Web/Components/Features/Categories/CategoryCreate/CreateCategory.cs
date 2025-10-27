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

	public interface ICreateCategoryHandler
	{
		Task<Result<CategoryDto>> HandleAsync(CategoryDto dto);
	}

	public class Handler : ICreateCategoryHandler
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
				_logger.LogWarning("CreateCategory: Category DTO cannot be null");
				return Result.Fail<CategoryDto>("Category data cannot be null");
			}

			if (string.IsNullOrWhiteSpace(dto.CategoryName))
			{
				_logger.LogWarning("CreateCategory: Category name cannot be empty");
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

			Result<Category> result = await _repository.AddCategory(category);

			if (result.Failure)
			{
				_logger.LogWarning("CreateCategory: Failed to create category. Error: {Error}", result.Error);
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

			_logger.LogInformation("CreateCategory: Successfully created category with ID: {Id}", category.Id);
			return Result.Ok(createdDto);
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
