// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleMappingExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.Extensions;

/// <summary>
/// Extension methods for mapping between Article entities and DTOs.
/// </summary>
public static class ArticleMappingExtensions
{

	/// <summary>
	/// Maps an Article entity to an ArticleDto.
	/// </summary>
	/// <param name="article">The article entity.</param>
	/// <param name="canEdit">Whether the current user can edit this article.</param>
	/// <returns>A new ArticleDto instance.</returns>
	public static ArticleDto ToDto(this Article article, bool canEdit = false)
	{
		return new ArticleDto(
			article.Id,
			article.Slug,
			article.Title,
			article.Introduction,
			article.Content,
			article.CoverImageUrl,
			article.Author,
			article.Category,
			article.IsPublished,
			article.PublishedOn,
			article.CreatedOn,
			article.ModifiedOn,
			article.IsArchived,
			canEdit,
			article.Version
		);
	}

	/// <summary>
	/// Maps an ArticleDto to an Article entity.
	/// </summary>
	/// <param name="dto">The article DTO.</param>
	/// <returns>A new Article entity instance.</returns>
	public static Article ToEntity(this ArticleDto dto)
	{
		return new Article
		{
			Id = dto.Id,
			Slug = dto.Slug,
			Title = dto.Title,
			Introduction = dto.Introduction,
			Content = dto.Content,
			CoverImageUrl = dto.CoverImageUrl,
			Author = dto.Author,
			Category = dto.Category,
			IsPublished = dto.IsPublished,
			PublishedOn = dto.PublishedOn,
			CreatedOn = dto.CreatedOn,
			ModifiedOn = dto.ModifiedOn,
			IsArchived = dto.IsArchived,
			Version = dto.Version
		};
	}

}
