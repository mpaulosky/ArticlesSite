// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using System.Linq.Expressions;

namespace Web.Data.Repositories;

/// <summary>
/// Category repository implementation using native MongoDB.Driver with factory pattern.
/// </summary>
public class CategoryRepository
(
		IMongoDbContextFactory contextFactory
) : ICategoryRepository
{

	/// <summary>
	/// Gets a category by its unique identifier.
	/// </summary>
	/// <param name="id">The ObjectId of the category.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the category if found, or an error message.</returns>
	public async Task<Result<Category>> GetCategoryByIdAsync(ObjectId id)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			Category? category = await context.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();

			if (category is null)
				return Result.Fail<Category>("Category not found");

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error getting category by id: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets a category by its slug.
	/// </summary>
	/// <param name="slug">The slug of the category.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the category if found, or an error message.</returns>
	public async Task<Result<Category>> GetCategory(string slug)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			Category? category = await context.Categories.Find(c => c.Slug == slug && !c.IsArchived).FirstOrDefaultAsync();

			if (category is null)
				return Result.Fail<Category>("Category not found");

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error getting category: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets all non-archived categories.
	/// </summary>
	/// <returns>A <see cref="Result{T}"/> containing a collection of Category or an error message.</returns>
	public async Task<Result<IEnumerable<Category>>> GetCategories()
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			List<Category>? categories = await context.Categories.Find(c => !c.IsArchived).ToListAsync();

			if (categories is null)
				return Result.Fail<IEnumerable<Category>>("No categories found");

			return Result.Ok<IEnumerable<Category>>(categories);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Category>>($"Error getting categories: {ex.Message}");
		}
	}

	/// <summary>
	/// Gets categories matching a specified predicate.
	/// </summary>
	/// <param name="where">The predicate to filter categories.</param>
	/// <returns>A <see cref="Result{T}"/> containing a filtered collection of Category or an error message.</returns>
	public async Task<Result<IEnumerable<Category>>> GetCategories(Expression<Func<Category, bool>> where)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			List<Category>? categories = await context.Categories.Find(where).ToListAsync();

			if (categories is null)
				return Result.Fail<IEnumerable<Category>>("No categories found");

			return Result.Ok<IEnumerable<Category>>(categories);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Category>>($"Error getting categories: {ex.Message}");
		}
	}

	/// <summary>
	/// Adds a new category to the database.
	/// </summary>
	/// <param name="category">The category to add.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the added category or an error message.</returns>
	public async Task<Result<Category>> AddCategory(Category category)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Categories.InsertOneAsync(category);

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error adding category: {ex.Message}");
		}
	}

	/// <summary>
	/// Updates an existing category in the database.
	/// </summary>
	/// <param name="category">The category to update.</param>
	/// <returns>A <see cref="Result{Category}"/> containing the updated category or an error message.</returns>
	public async Task<Result<Category>> UpdateCategory(Category category)
	{
		try
		{
			IMongoDbContext context = contextFactory.CreateDbContext();
			await context.Categories.ReplaceOneAsync(c => c.Id == category.Id, category);

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error updating category: {ex.Message}");
		}
	}

	/// <summary>
	/// Archives a category by its slug.
	/// </summary>
	/// <param name="slug">The slug of the category to archive.</param>
	public async Task ArchiveCategory(string slug)
	{
		IMongoDbContext context = contextFactory.CreateDbContext();
		UpdateDefinition<Category>? update = Builders<Category>.Update.Set(c => c.IsArchived, true);
		await context.Categories.UpdateOneAsync(c => c.Slug == slug, update);
	}

}
