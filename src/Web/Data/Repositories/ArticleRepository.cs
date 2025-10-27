// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArticleRepository.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

using System.Linq.Expressions;

namespace Web.Data.Repositories;

/// <summary>
///   Article repository implementation using native MongoDB.Driver with factory pattern
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
			IMongoDbContext context = _contextFactory.CreateDbContext();

			Article? article = await context.Articles
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
			IMongoDbContext context = _contextFactory.CreateDbContext();

			Article? article = await context.Articles
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
			IMongoDbContext context = _contextFactory.CreateDbContext();

			List<Article>? articles = await context.Articles
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
			IMongoDbContext context = _contextFactory.CreateDbContext();

			List<Article>? articles = await context.Articles
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
			IMongoDbContext context = _contextFactory.CreateDbContext();
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
			IMongoDbContext context = _contextFactory.CreateDbContext();
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
		IMongoDbContext context = _contextFactory.CreateDbContext();
		UpdateDefinition<Article>? update = Builders<Article>.Update.Set(a => a.IsArchived, true);
		await context.Articles.UpdateOneAsync(a => a.Slug == slug, update);
	}

}
