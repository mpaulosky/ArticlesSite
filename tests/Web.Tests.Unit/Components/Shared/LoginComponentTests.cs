// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     LoginComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Authorization;

namespace Web.Components.Shared;

/// <summary>
/// Unit tests for LoginComponent using Bunit
/// </summary>
[ExcludeFromCodeCoverage]
public class LoginComponentTests : BunitContext
{

	public LoginComponentTests()
	{
		Services.AddAuthorization(options => { });
		Services.AddSingleton<IAuthorizationService, AlwaysAllowAuthorizationService>();
	}

	private class AlwaysAllowAuthorizationService : IAuthorizationService
	{
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements)
				=> Task.FromResult(user.Identity?.IsAuthenticated == true ? AuthorizationResult.Success() : AuthorizationResult.Failed());
		public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
				=> Task.FromResult(user.Identity?.IsAuthenticated == true ? AuthorizationResult.Success() : AuthorizationResult.Failed());
	}

	[Fact]
	public void RendersLoginLink_WhenNotAuthorized()
	{
		Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: false));
		var cut = Render<CascadingAuthenticationState>(parameters =>
				parameters.AddChildContent<LoginComponent>());
		cut.Markup.Contains("Log in");
		Assert.DoesNotContain("Log out", cut.Markup);
	}

	[Fact]
	public void RendersLogoutLink_WhenAuthorized()
	{
		Services.AddSingleton<AuthenticationStateProvider>(new TestAuthStateProvider(isAuthenticated: true));
		var cut = Render<CascadingAuthenticationState>(parameters =>
				parameters.AddChildContent<LoginComponent>());
		cut.Markup.Contains("Log out");
		Assert.DoesNotContain("Log in", cut.Markup);
	}

	private class TestAuthStateProvider : AuthenticationStateProvider
	{
		private readonly bool _isAuthenticated;
		public TestAuthStateProvider(bool isAuthenticated) => _isAuthenticated = isAuthenticated;
		public override Task<AuthenticationState> GetAuthenticationStateAsync()
		{
			var identity = _isAuthenticated ? new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim("name", "TestUser") }, "TestAuth") : new System.Security.Claims.ClaimsIdentity();
			return Task.FromResult(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal(identity)));
		}
	}
}
