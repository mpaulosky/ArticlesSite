using Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);

	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
/// <summary>
/// HTTPS redirection is disabled in development mode to simplify local testing and avoid certificate issues.
/// </summary>
if (!app.Environment.IsDevelopment())
{
	app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
		.AddInteractiveServerRenderMode();

app.Run();