// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategory.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoryDetails;

public static class GetCategory
{

	public interface IGetCategoryHandler
	{
		Task<Result<CategoryDto>> HandleAsync(string identifier);
		Task<Result<CategoryDto>> HandleByIdAsync(ObjectId id);
	}

	public class Handler : IGetCategoryHandler
	{

		private readonly ICategoryRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(ICategoryRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<CategoryDto>> HandleAsync(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				_logger.LogWarning("GetCategory: Invalid identifier provided");
				return Result.Fail<CategoryDto>("Category identifier cannot be empty");
			}

			// Try to parse as ObjectId first
			if (ObjectId.TryParse(identifier, out ObjectId objectId))
			{
				return await HandleByIdAsync(objectId);
			}

			// Otherwise treat as slug
			Result<Category?> result = await _repository.GetCategory(identifier);

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetCategory: Category not found with slug: {Slug}", identifier);
				return Result.Fail<CategoryDto>(result.Error ?? "Category not found");
			}

			Category category = result.Value;

			var dto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			_logger.LogInformation("GetCategory: Successfully retrieved category with slug: {Slug}", identifier);
			return Result.Ok(dto);
		}

		public async Task<Result<CategoryDto>> HandleByIdAsync(ObjectId id)
		{
			Result<Category?> result = await _repository.GetCategoryByIdAsync(id);

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetCategory: Category not found with ID: {Id}", id);
				return Result.Fail<CategoryDto>(result.Error ?? "Category not found");
			}

			Category category = result.Value;

			var dto = new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			};

			_logger.LogInformation("GetCategory: Successfully retrieved category with ID: {Id}", id);
			return Result.Ok(dto);
		}

	}

}
