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

	/// <summary>
	/// Interface for handling category retrieval operations.
	/// </summary>
	public interface IGetCategoryHandler
	{

		/// <summary>
		/// Handles retrieval of a category by identifier.
		/// </summary>
		/// <param name="identifier">The category identifier.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		Task<Result<CategoryDto>> HandleAsync(string identifier);

		/// <summary>
		/// Handles retrieval of a category by ObjectId.
		/// </summary>
		/// <param name="id">The ObjectId of the category.</param>
		/// <returns>A <see cref="Result{CategoryDto}"/> representing the outcome.</returns>
		Task<Result<CategoryDto>> HandleByIdAsync(ObjectId id);

	}

	/// <summary>
	/// Command handler for retrieving a category.
	/// </summary>
	public class Handler(ICategoryRepository repository, ILogger<Handler> logger) : IGetCategoryHandler
	{
		/// <inheritdoc />
		public async Task<Result<CategoryDto>> HandleAsync(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				logger.LogWarning("GetCategory: Invalid identifier provided");
				return Result.Fail<CategoryDto>("Category identifier cannot be empty");
			}

			// Try to parse as ObjectId first
			if (ObjectId.TryParse(identifier, out ObjectId objectId))
			{
				return await HandleByIdAsync(objectId);
			}

			// Otherwise treat as slug
			Result<Category?> result = await repository.GetCategory(identifier);

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetCategory: Category not found with slug: {Slug}", identifier);
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

			logger.LogInformation("GetCategory: Successfully retrieved category with slug: {Slug}", identifier);
			return Result.Ok(dto);
		}

		/// <inheritdoc />
		public async Task<Result<CategoryDto>> HandleByIdAsync(ObjectId id)
		{
			Result<Category?> result = await repository.GetCategoryByIdAsync(id);

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetCategory: Category not found with ID: {Id}", id);
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

			logger.LogInformation("GetCategory: Successfully retrieved category with ID: {Id}", id);
			return Result.Ok(dto);
		}
	}

}
