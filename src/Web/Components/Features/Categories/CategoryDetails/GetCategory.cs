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
		Task<Result<CategoryDto>> HandleAsync(string slug);
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

		public async Task<Result<CategoryDto>> HandleAsync(string slug)
		{
			if (string.IsNullOrWhiteSpace(slug))
			{
				_logger.LogWarning("GetCategory: Invalid slug provided");
				return Result.Fail<CategoryDto>("Category slug cannot be empty");
			}

			Result<Category?> result = await _repository.GetCategory(slug);

			if (result.Failure || result.Value is null)
			{
				_logger.LogWarning("GetCategory: Category not found with slug: {Slug}", slug);
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

			_logger.LogInformation("GetCategory: Successfully retrieved category with slug: {Slug}", slug);
			return Result.Ok(dto);
		}

	}

}
