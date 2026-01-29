using System.Text.Json;

using Web.Components.Features.Articles.Fakes;

namespace Web.Services;

public class DatabaseSeeder
{

	private readonly IMongoDatabase _database;

	private readonly ILogger<DatabaseSeeder> _logger;

	public DatabaseSeeder(IMongoDatabase database, ILogger<DatabaseSeeder> logger)
	{

		_database = database;
		_logger = logger;

	}

	public async Task SeedAsync()
	{
		try
		{
			await SeedCategoriesAsync();
			await SeedArticleAsync();
			_logger.LogInformation("Database seeding completed successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during database seeding");
			throw;
		}
	}

	private async Task SeedCategoriesAsync()
	{
		try
		{
			var categoriesCollection = _database.GetCollection<Category>("Categories");
			var count = await categoriesCollection.CountDocumentsAsync(FilterDefinition<Category>.Empty);

			if (count > 0)
			{
				_logger.LogInformation("Categories already seeded");
				return;
			}

			var categories = JsonSerializer.Deserialize<List<Category>>(_categoriesJson);

			if (categories is not null && categories.Count > 0)
			{
				await categoriesCollection.InsertManyAsync(categories);
				_logger.LogInformation("Seeded {CategoryCount} categories", categories.Count);
			}
		}
		catch (JsonException ex)
		{
			_logger.LogError(ex, "Error deserializing categories JSON");
			throw;
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			_logger.LogInformation("Categories already exist, skipping seed");
		}
		catch (MongoException ex)
		{
			_logger.LogError(ex, "MongoDB error seeding categories");
			throw;
		}
	}

	private async Task SeedArticleAsync()
	{
		try
		{
			var articlesCollection = _database.GetCollection<Article>("Articles");
			var count = await articlesCollection.CountDocumentsAsync(FilterDefinition<Article>.Empty);

			if (count > 0)
			{
				_logger.LogInformation("Articles already seeded");
				return;
			}

			var article = FakeArticle.GetNewArticle(true);
			await articlesCollection.InsertOneAsync(article);
			_logger.LogInformation("Seeded one article");
		}
		catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
		{
			_logger.LogInformation("Articles already exist, skipping seed");
		}
		catch (MongoException ex)
		{
			_logger.LogError(ex, "MongoDB error seeding articles");
			throw;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error seeding articles");
			throw;
		}
	}

	private static readonly string _categoriesJson = @"[
    {
        ""_id"": { ""$oid"": ""677db927900ea4af1b500cab"" },
        ""CategoryName"": ""ASP.NET Core"",
        ""archived"": false,
        ""Slug"": ""asp.net-core"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db927900ea4af1b500cac"" },
        ""CategoryName"": ""Blazor Server"",
        ""archived"": false,
        ""Slug"": ""blazor-server"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db9bd900ea4af1b500cad"" },
        ""CategoryName"": ""Blazor WebAssembly"",
        ""archived"": false,
        ""Slug"": ""blazor-webassembly"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db9bd900ea4af1b500cae"" },
        ""CategoryName"": ""C#"",
        ""archived"": false,
        ""Slug"": ""c%23"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db9bd900ea4af1b500caf"" },
        ""CategoryName"": ""Entity Framework Core (EF Core)"",
        ""archived"": false,
        ""Slug"": ""entity-framework-core-(ef-core)"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db9bd900ea4af1b500cb0"" },
        ""CategoryName"": "".NET MAUI"",
        ""archived"": false,
        ""Slug"": "".net-maui"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    },
    {
        ""_id"": { ""$oid"": ""677db9bd900ea4af1b500cb1"" },
        ""CategoryName"": ""Other"",
        ""archived"": false,
        ""Slug"": ""other"",
        ""ArchivedBy"": { ""Email"": """", ""Name"": """", ""Roles"": [], ""UserId"": """" }
    }
]";

}
