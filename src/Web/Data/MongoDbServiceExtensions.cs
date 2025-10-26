// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web
// =======================================================

namespace Web.Data;

/// <summary>
/// Provides extension methods for registering MongoDB-related services, context factories, and repositories
/// in a Blazor/ASP.NET Core application using Aspire service discovery and Aspire.MongoDB.Driver.
/// </summary>
public static class MongoDbServiceExtensions
{

	/// <summary>
	/// Adds MongoDB services to the application's dependency injection container using Aspire service discovery and Aspire.MongoDB.Driver.
	/// Registers IMongoClient, IMongoDatabase, MongoDbContext, and related repositories and handlers.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder"/> for chaining.</returns>
	public static WebApplicationBuilder AddMongoDb(this WebApplicationBuilder builder)
	{
		var services = builder.Services;

		// Aspire.MongoDB.Driver automatically registers both IMongoClient and IMongoDatabase
		// when AddMongoDBClient is called with a connection name that matches the database resource
		// defined in the AppHost (via AddDatabase)
		builder.AddMongoDBClient(ArticleConnect);

		// Manually register IMongoDatabase for DI
		services.AddScoped<IMongoDatabase>(sp =>
		{
			var client = sp.GetRequiredService<IMongoClient>();

			// Use your database name here, e.g. "articlesdb"
			return client.GetDatabase(DatabaseName);
		});

		// Register MongoDbContext directly using Aspire-provided IMongoClient and IMongoDatabase
		services.AddScoped<IMongoDbContext>(sp =>
		{
			var client = sp.GetRequiredService<IMongoClient>();
			var database = sp.GetRequiredService<IMongoDatabase>();

			return new MongoDbContext(client, database.DatabaseNamespace.DatabaseName);
		});

		// Register factory wrapper for the factory interface
		services.AddScoped<IMongoDbContextFactory>(sp =>
		{
			var context = sp.GetRequiredService<IMongoDbContext>();
			return new RuntimeMongoDbContextFactory(context);
		});

		RegisterRepositoriesAndHandlers(services);

		return builder;
	}

	/// <summary>
	/// Runtime adapter that wraps the DI-resolved <see cref="IMongoDbContext"/> for use with <see cref="IMongoDbContextFactory"/>.
	/// </summary>
	private sealed class RuntimeMongoDbContextFactory : IMongoDbContextFactory
	{
		private readonly IMongoDbContext _context;

		/// <summary>
		/// Initializes a new instance of the <see cref="RuntimeMongoDbContextFactory"/> class.
		/// </summary>
		/// <param name="context">The DI-resolved <see cref="IMongoDbContext"/> instance.</param>
		public RuntimeMongoDbContextFactory(IMongoDbContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Returns the DI-resolved <see cref="IMongoDbContext"/> instance.
		/// </summary>
		/// <returns>The <see cref="IMongoDbContext"/> instance.</returns>
		public IMongoDbContext CreateDbContext()
		{
			// Return the context directly
			return _context;
		}
	}

	/// <summary>
	/// Registers MongoDB repositories and CQRS handlers for articles and categories.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to register services with.</param>
	private static void RegisterRepositoriesAndHandlers(IServiceCollection services)
	{
		// Register repositories
		services.AddScoped<IArticleRepository, ArticleRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();

		// Articles
		services.AddScoped<ICreateArticleHandler, CreateArticle.Handler>();
		services.AddScoped<IEditArticleHandler, EditArticle.Handler>();
		services.AddScoped<IGetArticleHandler, GetArticle.Handler>();
		services.AddScoped<IGetArticlesHandler, GetArticles.Handler>();

		// Categories
		services.AddScoped<ICreateCategoryHandler, CreateCategory.Handler>();
		services.AddScoped<IEditCategoryHandler, EditCategory.Handler>();
		services.AddScoped<IGetCategoryHandler, GetCategory.Handler>();
		services.AddScoped<IGetCategoriesHandler, GetCategories.Handler>();
	}


}