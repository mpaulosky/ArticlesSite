// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     IArticleRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Articles.Interfaces;

/// <summary>
/// Interface for accessing article data from the MongoDB database.
/// </summary>
public interface IArticleRepository
{

	/// <summary>
	/// Gets an article by its unique identifier.
	/// </summary>
	/// <param name="id">The ObjectId of the article.</param>
	/// <returns>A <see cref="Result{Article?}"/> containing the article if found, or an error message.</returns>
	Task<Result<Article?>> GetArticleByIdAsync(ObjectId id);

	/// <summary>
	/// Gets an article by its date string and slug.
	/// </summary>
	/// <param name="dateString">The date string of the article.</param>
	/// <param name="slug">The slug of the article.</param>
	/// <returns>A <see cref="Result{Article?}"/> containing the article if found, or an error message.</returns>
	Task<Result<Article?>> GetArticle(string dateString, string slug);

	/// <summary>
	/// Gets all non-archived articles.
	/// </summary>
	/// <returns>A <see cref="Result{IEnumerable{Article}?}"/> containing the articles or an error message.</returns>
	Task<Result<IEnumerable<Article>?>> GetArticles();

	/// <summary>
	/// Gets articles matching a specified predicate.
	/// </summary>
	/// <param name="where">The predicate to filter articles.</param>
	/// <returns>A <see cref="Result{IEnumerable{Article}?}"/> containing the filtered articles or an error message.</returns>
	Task<Result<IEnumerable<Article>?>> GetArticles(Expression<Func<Article, bool>> where);

	/// <summary>
	/// Adds a new article to the database.
	/// </summary>
	/// <param name="post">The article to add.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the added article or an error message.</returns>
	Task<Result<Article>> AddArticle(Article post);

	/// <summary>
	/// Updates an existing article in the database.
	/// </summary>
	/// <param name="post">The article to update.</param>
	/// <returns>A <see cref="Result{Article}"/> containing the updated article or an error message.</returns>
	Task<Result<Article>> UpdateArticle(Article post);

	/// <summary>
	/// Archives an article by its slug.
	/// </summary>
	/// <param name="slug">The slug of the article to archive.</param>
	/// <returns>An awaitable task.</returns>
	Task ArchiveArticle(string slug);

}
