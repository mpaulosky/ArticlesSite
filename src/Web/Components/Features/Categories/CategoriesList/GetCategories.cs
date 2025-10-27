// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     GetCategories.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.CategoriesList;

public static class GetCategories
{

	/// <summary>
	/// Interface for handling get categories operations.
	/// </summary>
	public interface IGetCategoriesHandler
	{
		/// <summary>
		/// Handles retrieval of all categories.
		/// </summary>
		/// <returns>A <see cref="Result{IEnumerable{CategoryDto}}"/> representing the outcome.</returns>
		Task<Result<IEnumerable<CategoryDto>>> HandleAsync();
	}

	/// <summary>
	/// Command handler for retrieving all categories.
	/// </summary>
	public class Handler(ICategoryRepository repository, ILogger<Handler> logger) : IGetCategoriesHandler
	{
		/// <inheritdoc />
		public async Task<Result<IEnumerable<CategoryDto>>> HandleAsync()
		{
			Result<IEnumerable<Category>?> result = await repository.GetCategories();

			if (result.Failure || result.Value is null)
			{
				logger.LogWarning("GetCategories: Failed to retrieve categories. Error: {Error}", result.Error);
				return Result.Fail<IEnumerable<CategoryDto>>(result.Error ?? "Failed to retrieve categories");
			}

			var dtos = result.Value.Select(category => new CategoryDto
			{
				Id = category.Id,
				CategoryName = category.CategoryName,
				CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
				ModifiedOn = category.ModifiedOn,
				IsArchived = category.IsArchived
			}).ToList();

			logger.LogInformation("GetCategories: Successfully retrieved {Count} categories", dtos.Count);
			return Result.Ok<IEnumerable<CategoryDto>>(dtos);
		}
	}

}
