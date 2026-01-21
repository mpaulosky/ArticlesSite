// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryMappingExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using Web.Components.Features.Categories.Models;

namespace Web.Components.Features.Categories.Extensions;

/// <summary>
/// Extension methods for mapping between Category entities and DTOs.
/// </summary>
public static class CategoryMappingExtensions
{

	/// <summary>
	/// Maps a Category entity to a CategoryDto.
	/// </summary>
	/// <param name="category">The category entity.</param>
	/// <returns>A new CategoryDto instance.</returns>
	public static CategoryDto ToDto(this Category category)
	{
		ArgumentNullException.ThrowIfNull(category);

		return new CategoryDto
		{
			Id = category.Id,
			CategoryName = category.CategoryName,
			Slug = category.Slug,
			CreatedOn = category.CreatedOn ?? DateTimeOffset.UtcNow,
			ModifiedOn = category.ModifiedOn,
			IsArchived = category.IsArchived,
			Version = category.Version
		};
	}

	/// <summary>
	/// Maps a CategoryDto to a Category entity.
	/// </summary>
	/// <param name="dto">The category DTO.</param>
	/// <returns>A new Category entity instance.</returns>
	public static Category ToEntity(this CategoryDto dto)
	{
		ArgumentNullException.ThrowIfNull(dto);

		return new Category
		{
			CategoryName = dto.CategoryName,
			Slug = dto.Slug,
			CreatedOn = dto.CreatedOn,
			ModifiedOn = dto.ModifiedOn,
			IsArchived = dto.IsArchived,
			Version = dto.Version
		};
	}

	/// <summary>
	/// Maps a collection of Category entities to a collection of CategoryDtos.
	/// Optimized for batch operations such as GetCategories.
	/// </summary>
	/// <param name="categories">The collection of category entities.</param>
	/// <returns>A collection of CategoryDto instances.</returns>
	public static IEnumerable<CategoryDto> ToDtos(this IEnumerable<Category> categories)
	{
		ArgumentNullException.ThrowIfNull(categories);

		return categories.Select(c => c.ToDto());
	}

}
