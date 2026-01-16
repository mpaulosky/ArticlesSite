using Blazored.LocalStorage;

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

// ReSharper disable once AsyncVoidMethod
app.MapGet("/Account/Login", async void (HttpContext httpContext, string returnUrl = "/") =>
{
	var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
			.WithRedirectUri(returnUrl)
			.Build();

	await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
});

app.MapGet("/Account/Logout", async httpContext =>
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