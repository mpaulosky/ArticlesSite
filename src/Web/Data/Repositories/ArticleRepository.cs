// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Data.Repositories;

/// <summary>
/// Article repository implementation using native MongoDB.Driver with factory pattern
/// </summary>
public class ArticleRepository : IArticleRepository
{
	private readonly IMongoDbContextFactory _contextFactory;

	public ArticleRepository(IMongoDbContextFactory contextFactory)
	{
		_contextFactory = contextFactory;
	}

	public async Task<Result<Article?>> GetArticleByIdAsync(ObjectId id)
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			var article = await context.Articles
				.Find(a => a.Id == id)
				.FirstOrDefaultAsync();
			return Result.Ok<Article?>(article);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article?>($"Error getting article by id: {ex.Message}");
		}
	}

	public async Task<Result<Article?>> GetArticle(string dateString, string slug)
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			var article = await context.Articles
				.Find(a => a.Slug == slug && !a.IsArchived)
				.FirstOrDefaultAsync();
			return Result.Ok<Article?>(article);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article?>($"Error getting article: {ex.Message}");
		}
	}

	public async Task<Result<IEnumerable<Article>?>> GetArticles()
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			var articles = await context.Articles
				.Find(a => !a.IsArchived)
				.ToListAsync();
			return Result.Ok<IEnumerable<Article>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Article>?>($"Error getting articles: {ex.Message}");
		}
	}

	public async Task<Result<IEnumerable<Article>?>> GetArticles(Expression<Func<Article, bool>> where)
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			var articles = await context.Articles
				.Find(where)
				.ToListAsync();
			return Result.Ok<IEnumerable<Article>?>(articles);
		}
		catch (Exception ex)
		{
			return Result.Fail<IEnumerable<Article>?>($"Error getting articles: {ex.Message}");
		}
	}

	public async Task<Result<Article>> AddArticle(Article post)
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			await context.Articles.InsertOneAsync(post);
			return Result.Ok(post);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article>($"Error adding article: {ex.Message}");
		}
	}

	public async Task<Result<Article>> UpdateArticle(Article post)
	{
		try
		{
			var context = _contextFactory.CreateDbContext();
			await context.Articles.ReplaceOneAsync(a => a.Id == post.Id, post);
			return Result.Ok(post);
		}
		catch (Exception ex)
		{
			return Result.Fail<Article>($"Error updating article: {ex.Message}");
		}
	}

	public async Task ArchiveArticle(string slug)
	{
		var context = _contextFactory.CreateDbContext();
		var update = Builders<Article>.Update.Set(a => a.IsArchived, true);
		await context.Articles.UpdateOneAsync(a => a.Slug == slug, update);
	}
}