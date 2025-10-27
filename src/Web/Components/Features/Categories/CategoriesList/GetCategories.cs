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

	public interface IGetCategoriesHandler
	{
		Task<Result<IEnumerable<CategoryDto>>> HandleAsync();
	}

	public class Handler : IGetCategoriesHandler
	{

		private readonly ICategoryRepository _repository;
		private readonly ILogger<Handler> _logger;

		public Handler(ICategoryRepository repository, ILogger<Handler> logger)
		{
			_repository = repository;
			_logger = logger;
		}

		public async Task<Result<IEnumerable<CategoryDto>>> HandleAsync()
		{
			Result<IEnumerable<Category>?> result = await _repository.GetCategories();

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetCategories: Failed to retrieve categories. Error: {Error}", result.Error);
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

			_logger.LogInformation("GetCategories: Successfully retrieved {Count} categories", dtos.Count);
			return Result.Ok<IEnumerable<CategoryDto>>(dtos);
		}

	}

}
