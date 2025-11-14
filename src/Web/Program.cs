WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;

// --- Service Registration ---

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
builder.Services.AddScoped<Web.Services.IFileStorage, Web.Services.FileStorage>();
builder.Services.AddScoped<Web.Services.IThemeService, Web.Services.ThemeService>();

// --- Build App ---
WebApplication app = builder.Build();

// --- Pipeline Configuration ---
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", true);
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/not-found");
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// --- Endpoint Mapping ---

app.MapStaticAssets();

app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapGet("/Account/Login", async (HttpContext httpContext, string returnUrl = "/") =>
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
	// Optional: decide whether to rethrow based on environment
}

// --- Run App ---
app.Run();