using Blazored.LocalStorage;
using Shared.Abstractions;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.Models;
using System.Linq;
using Polly;
using Web.Infrastructure;
using Web.Components.Features.Articles.Entities;
using Polly.Registry;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// --- Service Registration ---
IConfiguration configuration = builder.Configuration;

// Telemetry, health, resilience, service discovery (Aspire defaults)
builder.AddServiceDefaults();

// Authentication & Authorization
builder.Services.AddAuthenticationAndAuthorization(configuration);

// Output Cache
builder.Services.AddOutputCache();

// Data (MongoDB)
builder.AddMongoDb();

// UI (Blazor/Components)
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

// Application Services
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<IFileStorage, FileStorage>();
builder.Services.AddBlazoredLocalStorage();

// Concurrency options from configuration
builder.Services.Configure<Web.Infrastructure.ConcurrencyOptions>(builder.Configuration.GetSection("ConcurrencyOptions"));

// Register centralized Polly policy for concurrency retries as a strongly-typed IAsyncPolicy<Result<Article>>
builder.Services.AddSingleton<IAsyncPolicy<Result<Web.Components.Features.Articles.Entities.Article>>>(sp =>
{
	var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<Web.Infrastructure.ConcurrencyOptions>>().Value;
	return Web.Infrastructure.ConcurrencyPolicies.CreatePolicy(options);
});

// Metrics publisher (no-op by default; apps can replace with real telemetry publisher)
builder.Services.AddSingleton<IMetricsPublisher, NoOpMetricsPublisher>();

// --- Build App ---
WebApplication app = builder.Build();

// --- Pipeline Configuration ---
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", true);
	app.UseHsts();
}

app.UseHttpsRedirection();

// Statically files middleware first
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// --- Endpoint Mapping ---

// Map Razor Components first (handles RCL static files)
app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

// Then map static assets (for wwwroot files)
app.MapStaticAssets();

app.MapDefaultEndpoints();

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
{
	var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
			.WithRedirectUri(returnUrl)
			.Build();

	await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
{
	var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
			.WithRedirectUri("/")
			.Build();

	await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
	await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
});

// File serving endpoint for uploaded images
app.MapGet("/api/files/{fileName}", (string fileName, IWebHostEnvironment environment) =>
{
	var uploadsPath = Path.Combine(environment.WebRootPath, "uploads");
	var filePath = Path.Combine(uploadsPath, fileName);

	// Security: Ensure the file path is within the uploads directory
	var normalizedPath = Path.GetFullPath(filePath);
	var normalizedUploadsPath = Path.GetFullPath(uploadsPath);

	if (!normalizedPath.StartsWith(normalizedUploadsPath, StringComparison.OrdinalIgnoreCase))
	{
		return Task.FromResult(Results.BadRequest("Invalid file path."));
	}

	if (!File.Exists(filePath))
	{
		return Task.FromResult(Results.NotFound());
	}

	var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
	var contentType = fileExtension switch
	{
		".jpg" or ".jpeg" => "image/jpeg",
		".png" => "image/png",
		".gif" => "image/gif",
		".svg" => "image/svg+xml",
		".webp" => "image/webp",
		".bmp" => "image/bmp",
		_ => "application/octet-stream"
	};

	return Task.FromResult(Results.File(filePath, contentType));
});

// Minimal API: expose PUT endpoint for article edits so non-Blazor clients can detect concurrency conflicts (returns 409)
app.MapPut("/api/articles/{id}", async (string id, ArticleDto dto, EditArticle.IEditArticleHandler handler) =>
{
	if (!ObjectId.TryParse(id, out var objectId))
	{
		return Results.BadRequest(new { error = "Invalid article id" });
	}

	if (dto is null)
	{
		return Results.BadRequest(new { error = "Article data cannot be null" });
	}

	if (objectId != dto.Id)
	{
		return Results.BadRequest(new { error = "Id in route does not match DTO Id" });
	}

	var result = await handler.HandleAsync(dto);

	if (result.Success)
	{
		return Results.Ok(result.Value);
	}

	if (result.ErrorCode == ResultErrorCode.Concurrency)
	{
		if (result.Details is Web.Infrastructure.ConcurrencyConflictInfo conflict)
		{
			var conflictDto = new Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto(result.Error, (int)result.ErrorCode, conflict.ServerVersion, conflict.ServerArticle, conflict.ChangedFields);
			return Results.Conflict(conflictDto);
		}

		return Results.Conflict(new Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto(result.Error, (int)result.ErrorCode, -1, null, null));
	}

	// Default to 400 for other failures
	return Results.BadRequest(new { error = result.Error });
})
.WithName("UpdateArticle")
.WithTags("Articles")
.Produces<ArticleDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>(StatusCodes.Status409Conflict);

// --- Startup Logic (Database Seeding) ---

try
{
	using var scope = app.Services.CreateScope();
	var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
	await seeder.SeedAsync();
}
catch (Exception ex)
{
	app.Logger.LogError(ex, "Database seeding failed.");
	// Optional: decide whether to rethrow based on the environment
}

// --- Run App ---
app.Run();
