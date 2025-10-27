// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     MongoDbServiceExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Data;

/// <summary>
///   Provides extension methods for registering MongoDB-related services, context factories, and repositories
///   in a Blazor/ASP.NET Core application using Aspire service discovery.
/// </summary>
public static class MongoDbServiceExtensions
{

	/// <summary>
	///   Adds MongoDB services to the application's dependency injection container using Aspire service discovery.
	///   Registers IMongoClient, IMongoDatabase, MongoDbContext, and related repositories and handlers.
	/// </summary>
	/// <param name="builder">The <see cref="WebApplicationBuilder" /> to configure.</param>
	/// <returns>The configured <see cref="WebApplicationBuilder" /> for chaining.</returns>
	public static WebApplicationBuilder AddMongoDb(this WebApplicationBuilder builder)
	{
		IServiceCollection services = builder.Services;

		// Register IMongoClient using connection string from configuration
		services.AddSingleton<IMongoClient>(sp =>
		{
			var connectionString = builder.Configuration.GetConnectionString("articlesdb")
				?? throw new InvalidOperationException("MongoDB connection string 'articlesdb' not found.");
			return new MongoClient(connectionString);
		});

		// Manually register IMongoDatabase for DI
		services.AddScoped<IMongoDatabase>(sp =>
		{
			IMongoClient client = sp.GetRequiredService<IMongoClient>();

			// Use your database name here, e.g. "articlesdb"
			return client.GetDatabase("articlesdb");
		});

		// Register MongoDbContext directly using IMongoClient and IMongoDatabase
		services.AddScoped<IMongoDbContext>(sp =>
		{
			IMongoClient client = sp.GetRequiredService<IMongoClient>();
			IMongoDatabase database = sp.GetRequiredService<IMongoDatabase>();

			return new MongoDbContext(client, database.DatabaseNamespace.DatabaseName);
		});

		// Register a factory wrapper for the factory interface
		services.AddScoped<IMongoDbContextFactory>(sp =>
		{
			IMongoDbContext context = sp.GetRequiredService<IMongoDbContext>();

			return new RuntimeMongoDbContextFactory(context);
		});

		RegisterRepositoriesAndHandlers(services);

		return builder;
	}

	/// <summary>
	///   Registers MongoDB repositories and CQRS handlers for articles and categories.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection" /> to register services with.</param>
	private static void RegisterRepositoriesAndHandlers(IServiceCollection services)
	{
		// Register repositories
		services.AddScoped<IArticleRepository, ArticleRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();

		// Category Handlers
		services.AddScoped(
			typeof(Web.Components.Features.Categories.CategoryEdit.EditCategory.IEditCategoryHandler),
			typeof(Web.Components.Features.Categories.CategoryEdit.EditCategory.Handler));
		services.AddScoped(
			typeof(Web.Components.Features.Categories.CategoryDetails.GetCategory.IGetCategoryHandler),
			typeof(Web.Components.Features.Categories.CategoryDetails.GetCategory.Handler));
		services.AddScoped(
			typeof(Web.Components.Features.Categories.CategoryCreate.CreateCategory.ICreateCategoryHandler),
			typeof(Web.Components.Features.Categories.CategoryCreate.CreateCategory.Handler));
		services.AddScoped(
			typeof(Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler),
			typeof(Web.Components.Features.Categories.CategoriesList.GetCategories.Handler));
	}

	/// <summary>
	///   Runtime adapter that wraps the DI-resolved <see cref="IMongoDbContext" /> for use with
	///   <see cref="IMongoDbContextFactory" />.
	/// </summary>
	private sealed class RuntimeMongoDbContextFactory : IMongoDbContextFactory
	{

		private readonly IMongoDbContext _context;

		/// <summary>
		///   Initializes a new instance of the <see cref="RuntimeMongoDbContextFactory" /> class.
		/// </summary>
		/// <param name="context">The DI-resolved <see cref="IMongoDbContext" /> instance.</param>
		public RuntimeMongoDbContextFactory(IMongoDbContext context)
		{
			_context = context;
		}

		/// <summary>
		///   Returns the DI-resolved <see cref="IMongoDbContext" /> instance.
		/// </summary>
		/// <returns>The <see cref="IMongoDbContext" /> instance.</returns>
		public IMongoDbContext CreateDbContext()
		{
			// Return the context directly
			return _context;
		}

	}

}
