// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthenticationExtensions.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================

namespace Web.Services;

/// <summary>
/// Extension methods for configuring authentication and authorization services.
/// </summary>
public static class AuthenticationServiceExtensions
{

	/// <summary>
	/// Adds and configures authentication, authorization, and CORS services.
	/// </summary>
	public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
	{
		// Configure authentication: use cookies as the default authenticate/sign-in scheme
		// and Auth0 as the default challenge scheme for external login flows.
		// Note: Do not call AddCookie explicitly here because the Auth0 integration
		// already registers the cookie scheme. Registering it twice throws a "Scheme already exists" error.
		services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = Auth0Constants.AuthenticationScheme;
				})
				.AddAuth0WebAppAuthentication(options =>
				{
					options.Domain = configuration["Auth0:Domain"] ??
													throw new InvalidOperationException("Configuration value 'Auth0:Domain' is required.");

					options.ClientId = configuration["Auth0:ClientId"] ??
														throw new InvalidOperationException("Configuration value 'Auth0:ClientId' is required.");

					options.ClientSecret = configuration["Auth0:ClientSecret"] ??
																throw new InvalidOperationException(
																		"Configuration value 'Auth0:ClientSecret' is required.");

					// Add role claim to the user profile
					options.Scope = "openid profile email";
				});

		// Configure authentication state services
		services.AddScoped<AuthenticationStateProvider, Auth0AuthenticationStateProvider>();
		services.AddCascadingAuthenticationState();

		// Add authorization services
		services.AddAuthorizationBuilder()
				.AddPolicy("RequireAdminRole", policy =>
						policy.RequireRole("Admin"))
				.AddPolicy("RequireUserRole", policy =>
						policy.RequireRole("User", "Author", "Admin"));

		services.AddCors(options =>
		{
			options.AddDefaultPolicy(policy => policy
					.WithOrigins("https://localhost:7180")
					.AllowAnyHeader()
					.AllowAnyMethod());
		});

		return services;
	}

}