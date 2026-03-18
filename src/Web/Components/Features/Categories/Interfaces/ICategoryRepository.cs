// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ICategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Components.Features.Categories.Interfaces;

/// <summary>
/// Interface for accessing category data from the MongoDB database.
/// </summary>
public interface ICategoryRepository
{


	/// <summary>
	/// Gets a category by its unique identifier.
	/// </summary>
	/// <param name="id">The ObjectId of the category.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the category if found, or an error message.</returns>
	Task<Result<Category>> GetCategoryByIdAsync(ObjectId id);

	/// <summary>
	/// Gets a category by its slug.
	/// </summary>
	/// <param name="slug">The slug of the category.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the category if found, or an error message.</returns>
	Task<Result<Category>> GetCategory(string slug);

	/// <summary>
	/// Gets all non-archived categories.
	/// </summary>
	/// <returns>A <see cref="Result{T}"/> containing a collection of Category or an error message.</returns>
	Task<Result<IEnumerable<Category>>> GetCategories();

	/// <summary>
	/// Gets categories matching a specified predicate.
	/// </summary>
	/// <param name="where">The predicate to filter categories.</param>
	/// <returns>A <see cref="Result{T}"/> containing a filtered collection of Category or an error message.</returns>
	Task<Result<IEnumerable<Category>>> GetCategories(Expression<Func<Category, bool>> where);

	/// <summary>
	/// Adds a new category to the database.
	/// </summary>
	/// <param name="category">The category to add.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the added category or an error message.</returns>
	Task<Result<Category>> AddCategory(Category category);

	/// <summary>
	/// Updates an existing category in the database.
	/// Implementations should use ReplaceOneAsync for updates and filter by Id (e.g., Id ==) and Version when implemented.
	/// </summary>
	/// <param name="category">The category to update.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the updated category or an error message.</returns>
	Task<Result<Category>> UpdateCategory(Category category);

	/// <summary>
	/// Archives a category by its slug.
	/// </summary>
	/// <param name="slug">The slug of the category to archive.</param>
	/// <returns>An awaitable task.</returns>
	Task ArchiveCategory(string slug);

}
