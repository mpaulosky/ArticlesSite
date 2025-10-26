// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CategoryRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

#region

using System.Linq.Expressions;

#endregion

namespace Web.Data.Repositories;

/// <summary>
///   Category repository implementation using native MongoDB.Driver with factory pattern
/// </summary>
public class CategoryRepository : ICategoryRepository
{

	private readonly IMongoDbContextFactory _contextFactory;

	public CategoryRepository(IMongoDbContextFactory contextFactory)
	{
		_contextFactory = contextFactory;
	}

	public async Task<Result<Category?>> GetCategoryByIdAsync(ObjectId id)
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			Category? category = await context.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();

			return Result.Ok<Category?>(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category?>($"Error getting category by id: {ex.Message}");
		}
	}

	public async Task<Result<Category?>> GetCategory(string slug)
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			Category? category = await context.Categories.Find(c => c.Slug == slug && !c.IsArchived).FirstOrDefaultAsync();

			return Result.Ok<Category?>(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category?>($"Error getting category: {ex.Message}");
		}
	}

	public async Task<Result<IEnumerable<Category>?>> GetCategories()
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			List<Category>? categories = await context.Categories.Find(c => !c.IsArchived).ToListAsync();

			return Result.Ok<IEnumerable<Category>?>(categories);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Category>?>($"Error getting categories: {ex.Message}");
		}
	}

	public async Task<Result<IEnumerable<Category>?>> GetCategories(Expression<Func<Category, bool>> where)
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			List<Category>? categories = await context.Categories.Find(where).ToListAsync();

			return Result.Ok<IEnumerable<Category>?>(categories);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Category>?>($"Error getting categories: {ex.Message}");
		}
	}

	public async Task<Result<Category>> AddCategory(Category category)
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			await context.Categories.InsertOneAsync(category);

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error adding category: {ex.Message}");
		}
	}

	public async Task<Result<Category>> UpdateCategory(Category category)
	{
		try
		{
			IMongoDbContext context = _contextFactory.CreateDbContext();
			await context.Categories.ReplaceOneAsync(c => c.Id == category.Id, category);

			return Result.Ok(category);
		}
		catch (Exception ex)
		{
			return Result.Fail<Category>($"Error updating category: {ex.Message}");
		}
	}

	public async Task ArchiveCategory(string slug)
	{
		IMongoDbContext context = _contextFactory.CreateDbContext();
		UpdateDefinition<Category>? update = Builders<Category>.Update.Set(c => c.IsArchived, true);
		await context.Categories.UpdateOneAsync(c => c.Slug == slug, update);
	}

}
