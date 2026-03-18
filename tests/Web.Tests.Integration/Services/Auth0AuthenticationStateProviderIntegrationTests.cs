// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     Auth0AuthenticationStateProviderIntegrationTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Integration
// =======================================================

using System.Linq;
using System.Security.Claims;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Tests.Integration.Services;

[ExcludeFromCodeCoverage]
public class Auth0AuthenticationStateProviderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public Auth0AuthenticationStateProviderIntegrationTests(WebApplicationFactory<Program> factory)
	{
		// Ensure Auth0 config is available early for Program startup (read from environment variables)
		Environment.SetEnvironmentVariable("Auth0__Domain", "test");
		Environment.SetEnvironmentVariable("Auth0__ClientId", "test");
		Environment.SetEnvironmentVariable("Auth0__ClientSecret", "test");

		_factory = factory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureAppConfiguration((context, config) =>
			{
				config.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Auth0:Domain"] = "test",
					["Auth0:ClientId"] = "test",
					["Auth0:ClientSecret"] = "test",
				});
			});

			builder.Configure(app =>
			{
				// Middleware to set the test principal from headers
				app.Use(async (context, next) =>
				{
					var rolesHeader = context.Request.Headers["X-Test-Roles"].FirstOrDefault();
					var auth0Header = context.Request.Headers["X-Test-Auth0-Roles"].FirstOrDefault();

					var claims = new List<Claim> { new Claim(ClaimTypes.Name, "integration-test") };

					if (!string.IsNullOrEmpty(rolesHeader))
					{
						claims.Add(new Claim("https://articlesite.com/roles", rolesHeader));
					}

					if (!string.IsNullOrEmpty(auth0Header))
					{
						foreach (var r in auth0Header.Split(',', StringSplitOptions.RemoveEmptyEntries))
						{
							claims.Add(new Claim("roles", r.Trim()));
						}
					}

					context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
					await next();
				});

				app.UseRouting();
				app.UseEndpoints(endpoints =>
				{
					endpoints.MapGet("/test/authroles", async context =>
					{
						var provider = context.RequestServices.GetRequiredService<AuthenticationStateProvider>();
						var state = await provider.GetAuthenticationStateAsync();
						var roles = state.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
						await context.Response.WriteAsJsonAsync(roles);
					});
				});
			});
		});
	}

	[Fact]
	public async Task Provider_MergesRolesFromCustomAndAuth0Claims()
	{
		using var client = _factory.CreateClient();
		var request = new HttpRequestMessage(HttpMethod.Get, "/test/authroles");
		request.Headers.Add("X-Test-Roles", "Admin, Editor");
		request.Headers.Add("X-Test-Auth0-Roles", "Admin, Viewer");

		var resp = await client.SendAsync(request, TestContext.Current.CancellationToken);
		resp.EnsureSuccessStatusCode();

		var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(json);

		roles.Should().NotBeNull();
		roles.Should().Contain(new[] { "Admin", "Editor", "Viewer" });
	}

	[Fact]
	public async Task Provider_ReturnsAnonymous_WhenNoUser()
	{
		using var client = _factory.CreateClient();
		var resp = await client.GetAsync("/test/authroles", TestContext.Current.CancellationToken);
		resp.EnsureSuccessStatusCode();

		var json = await resp.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
		var roles = System.Text.Json.JsonSerializer.Deserialize<string[]>(json);

		roles.Should().NotBeNull();
		roles.Should().BeEmpty();
	}
}
