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
		await SeedCategoriesAsync();
		await SeedArticleAsync();
	}

	private async Task SeedCategoriesAsync()
	{
		var categoriesCollection = _database.GetCollection<Category>("Categories");
		var count = await categoriesCollection.CountDocumentsAsync(FilterDefinition<Category>.Empty);

		if (count > 0)
		{
			_logger.LogInformation("Categories already seeded.");

			return;
		}

		var categories = JsonSerializer.Deserialize<List<Category>>(_categoriesJson);

		if (categories != null)
		{
			await categoriesCollection.InsertManyAsync(categories);
			_logger.LogInformation($"Seeded {categories.Count} categories.");
		}
	}

	private async Task SeedArticleAsync()
	{
		var articlesCollection = _database.GetCollection<Article>("Articles");
		var count = await articlesCollection.CountDocumentsAsync(FilterDefinition<Article>.Empty);

		if (count > 0)
		{
			_logger.LogInformation("Articles already seeded.");

			return;
		}

		var article = FakeArticle.GetNewArticle(true);
		await articlesCollection.InsertOneAsync(article);
		_logger.LogInformation("Seeded one article.");
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
