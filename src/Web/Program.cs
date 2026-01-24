using Blazored.LocalStorage;
// Removed redundant usings moved to Web/GlobalUsings.cs

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Ensure console logging is available during tests so diagnostic logs surface in CI/test output
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
	options.SingleLine = true;
	options.TimestampFormat = "hh:mm:ss ";
});

// Response capture middleware moved to after app is built to avoid using 'app' before declaration.
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);

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

// Response capture middleware: for non-success responses capture and log the response body for diagnostics
app.Use(async (context, next) =>
{
	var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

	// Keep original body
	var originalBody = context.Response.Body;
	await using var memStream = new MemoryStream();
	context.Response.Body = memStream;

	try
	{
		await next();

		// If non-success, attempt to read body
		if (context.Response.StatusCode >= 400)
		{
			memStream.Seek(0, SeekOrigin.Begin);
			using var reader = new StreamReader(memStream, leaveOpen: true);
			string bodyText = await reader.ReadToEndAsync();
			logger.LogWarning("Response {StatusCode} for {Method} {Path} with body: {Body}", context.Response.StatusCode, context.Request.Method, context.Request.Path, bodyText);
			memStream.Seek(0, SeekOrigin.Begin);
		}

		// Copy back to original response stream
		memStream.Seek(0, SeekOrigin.Begin);
		await memStream.CopyToAsync(originalBody);
	}
	finally
	{
		context.Response.Body = originalBody;
	}
});

// Statically files middleware first
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// Diagnostic middleware: log incoming requests and response status codes to make integration test runs observable
app.Use(async (context, next) =>
{
	var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
	logger.LogInformation("Incoming request {Method} {Path}", context.Request.Method, context.Request.Path);
	// Also write to a temporary file so integration test harness can inspect server logs
	try
	{
		var entry = $"[{DateTimeOffset.UtcNow:O}] Incoming {context.Request.Method} {context.Request.Path}\n";
		// Write to current directory for local runs
		try
		{
			var logPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "articlesite-integration.log");
			await System.IO.File.AppendAllTextAsync(logPath, entry);
		}
		catch { }

		// Also write to temp directory so CI/test harness can find it reliably
		try
		{
			var tempPath = System.IO.Path.Combine(Path.GetTempPath(), "articlesite-integration.log");
			await System.IO.File.AppendAllTextAsync(tempPath, entry);
		}
		catch { }
	}
	catch { }
	try
	{
		await next();
	}
	finally
	{
		logger.LogInformation("Response for {Method} {Path} => {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
		try
		{
			var entry = $"[{DateTimeOffset.UtcNow:O}] Response {context.Request.Method} {context.Request.Path} => {context.Response.StatusCode}\n";
			try
			{
				var logPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "articlesite-integration.log");
				await System.IO.File.AppendAllTextAsync(logPath, entry);
			}
			catch { }

			try
			{
				var tempPath = System.IO.Path.Combine(Path.GetTempPath(), "articlesite-integration.log");
				await System.IO.File.AppendAllTextAsync(tempPath, entry);
			}
			catch { }
		}
		catch { }
	}
});

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

app.MapPut("/api/articles/{id}", async (HttpContext httpContext, string id, ArticleDto dto, EditArticle.IEditArticleHandler handler, IConfiguration config) =>
{
	// Debug: Log configuration values
	var connStr = config["MongoDb:ConnectionString"] ?? "(not set)";
	var dbName = config["MongoDb:Database"] ?? "(not set)";
	httpContext.Response.Headers["X-Debug-Config-ConnectionString"] = connStr.Contains("mongodb") ? "***REDACTED***" : connStr;
	httpContext.Response.Headers["X-Debug-Config-Database"] = dbName;

	if (dto is null)
	{
		httpContext.Response.Headers["X-Debug-Result"] = "BadRequest:Article data cannot be null";
		httpContext.Response.Headers["X-Debug-Stage"] = "dto-null";
		return Results.BadRequest(new { error = "Article data cannot be null" });
	}

	// Normalize id: prefer route id when present, otherwise use dto.Id
	if (ObjectId.TryParse(id, out var routeId))
	{
		if (dto.Id == ObjectId.Empty || dto.Id != routeId)
		{
			dto = new ArticleDto(routeId, dto.Slug, dto.Title, dto.Introduction, dto.Content, dto.CoverImageUrl, dto.Author, dto.Category, dto.IsPublished, dto.PublishedOn, dto.CreatedOn, dto.ModifiedOn, dto.IsArchived, dto.CanEdit, dto.Version);
		}
	}
	else if (dto.Id == ObjectId.Empty)
	{
		httpContext.Response.Headers["X-Debug-Result"] = "BadRequest:Invalid article id";
		httpContext.Response.Headers["X-Debug-Stage"] = "invalid-id";
		return Results.BadRequest(new { error = "Invalid article id" });
	}

	httpContext.Response.Headers["X-Debug-Stage"] = "calling-handler";
	var result = await handler.HandleAsync(dto);
	httpContext.Response.Headers["X-Debug-Stage"] = $"handler-returned:{(result.Success ? "success" : "failure")}";
	httpContext.Response.Headers["X-Debug-ErrorCode"] = result.ErrorCode.ToString();

	if (result.Success)
	{
		httpContext.Response.Headers["X-Debug-Result"] = "Success";
		httpContext.Response.Headers["X-Debug-Stage"] = "returning-ok";
		try { Console.Error.WriteLine($"[PROGRAM-LOG] UpdateArticle: Success Id={result.Value?.Id} Version={result.Value?.Version}"); } catch { }
		return Results.Ok(result.Value);
	}

	if (result.ErrorCode == ResultErrorCode.Concurrency)
	{
		if (result.Details is Web.Infrastructure.ConcurrencyConflictInfo conflict)
		{
			var conflictDto = new Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto(result.Error, (int)result.ErrorCode, conflict.ServerVersion, conflict.ServerArticle, conflict.ChangedFields);
			httpContext.Response.Headers["X-Debug-Result"] = $"Conflict:ServerVersion={conflict.ServerVersion}";
			httpContext.Response.Headers["X-Debug-Stage"] = "returning-conflict-with-details";
			try { Console.Error.WriteLine($"[PROGRAM-LOG] UpdateArticle: Conflict Id={dto.Id} ServerVersion={conflict.ServerVersion} Error={result.Error}"); } catch { }
			return Results.Conflict(conflictDto);
		}

		httpContext.Response.Headers["X-Debug-Result"] = "Conflict:NoDetails";
		httpContext.Response.Headers["X-Debug-Stage"] = "returning-conflict-no-details";
		try { Console.Error.WriteLine($"[PROGRAM-LOG] UpdateArticle: Conflict(no details) Id={dto.Id} Error={result.Error}"); } catch { }
		return Results.Conflict(new Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto(result.Error, (int)result.ErrorCode, -1, null, null));
	}

	httpContext.Response.Headers["X-Debug-Result"] = $"BadRequest:{result.Error}";
	httpContext.Response.Headers["X-Debug-Stage"] = "returning-badrequest";
	try { Console.Error.WriteLine($"[PROGRAM-LOG] UpdateArticle: BadRequest Id={dto.Id} Error={result.Error}"); } catch { }
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

