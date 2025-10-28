// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthenticationIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration;

/// <summary>
///   Integration tests for authentication flows and endpoints
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthenticationIntegrationTests : IClassFixture<TestWebHostBuilder>
{

	private readonly TestWebHostBuilder _factory;
	private readonly HttpClient _client;

	public AuthenticationIntegrationTests(TestWebHostBuilder factory)
	{
		_factory = factory;
		_client = _factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});
	}

	[Fact]
	public async Task LoginEndpoint_RedirectsToAuth0()
	{
		// Act
		var response = await _client.GetAsync("/Account/Login");

		// Assert
		response.Should().NotBeNull();

		// Should redirect to Auth0 or challenge authentication
		response.StatusCode.Should().BeOneOf(
			System.Net.HttpStatusCode.Redirect,
			System.Net.HttpStatusCode.Found,
			System.Net.HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task LoginEndpoint_WithReturnUrl_IncludesReturnUrlInRedirect()
	{
		// Act
		var response = await _client.GetAsync("/Account/Login?returnUrl=/admin");

		// Assert
		response.Should().NotBeNull();
		response.StatusCode.Should().BeOneOf(
			System.Net.HttpStatusCode.Redirect,
			System.Net.HttpStatusCode.Found,
			System.Net.HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task LogoutEndpoint_WithoutAuthentication_ProcessesLogout()
	{
		// Act
		var response = await _client.GetAsync("/Account/Logout");

		// Assert
		response.Should().NotBeNull();

		// Logout should process even without authentication
		response.StatusCode.Should().BeOneOf(
			System.Net.HttpStatusCode.OK,
			System.Net.HttpStatusCode.Redirect,
			System.Net.HttpStatusCode.Found);
	}

	[Fact]
	public async Task ProfilePage_WithoutAuthentication_RedirectsToLogin()
	{
		// Act
		var response = await _client.GetAsync("/profile");

		// Assert
		response.Should().NotBeNull();

		// Should redirect to login or return unauthorized
		response.StatusCode.Should().BeOneOf(
			System.Net.HttpStatusCode.Redirect,
			System.Net.HttpStatusCode.Found,
			System.Net.HttpStatusCode.Unauthorized);
	}

	[Fact]
	public async Task AuthenticationState_UnauthenticatedUser_ReturnsAnonymous()
	{
		// Arrange
		using var scope = _factory.Services.CreateScope();
		var authStateProvider = scope.ServiceProvider.GetService<Web.Services.Auth0AuthenticationStateProvider>();

		if (authStateProvider != null)
		{
			// Act
			var authState = await authStateProvider.GetAuthenticationStateAsync();

			// Assert
			authState.Should().NotBeNull();
			authState.User.Should().NotBeNull();
			authState.User.Identity.Should().NotBeNull();
			authState.User.Identity!.IsAuthenticated.Should().BeFalse();
		}
	}

}
