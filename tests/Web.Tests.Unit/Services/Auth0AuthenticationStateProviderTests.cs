// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     Auth0AuthenticationStateProviderTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Http;

namespace Web.Tests.Unit.Services;

/// <summary>
/// Unit tests for Auth0AuthenticationStateProvider
/// </summary>
[ExcludeFromCodeCoverage]
public class Auth0AuthenticationStateProviderTests
{
	private readonly IHttpContextAccessor _mockHttpContextAccessor;
	private readonly ILogger<Auth0AuthenticationStateProvider> _mockLogger;
	private readonly Auth0AuthenticationStateProvider _provider;

	public Auth0AuthenticationStateProviderTests()
	{
		_mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
		_mockLogger = Substitute.For<ILogger<Auth0AuthenticationStateProvider>>();
		_provider = new Auth0AuthenticationStateProvider(_mockHttpContextAccessor, _mockLogger);
	}

	[Fact]
	public void Constructor_WithValidDependencies_ShouldCreateInstance()
	{
		// Arrange & Act
		var provider = new Auth0AuthenticationStateProvider(_mockHttpContextAccessor, _mockLogger);

		// Assert
		provider.Should().NotBeNull();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenHttpContextIsNull_ShouldReturnAnonymousUser()
	{
		// Arrange
		_mockHttpContextAccessor.HttpContext.Returns((HttpContext?)null);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserIsNotAuthenticated_ShouldReturnAnonymousUser()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var identity = new ClaimsIdentity(); // Not authenticated
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserIsAuthenticated_ShouldReturnAuthenticatedUser()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim(ClaimTypes.Email, "test@example.com")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.Should().NotBeNull();
		result.User.Should().NotBeNull();
		result.User.Identity.Should().NotBeNull();
		result.User.Identity!.IsAuthenticated.Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasCustomRoleClaim_ShouldAddRoleClaimsToIdentity()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin,Editor")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(2);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasAuth0RoleClaim_ShouldAddRoleClaimsToIdentity()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("roles", "Admin"),
			new Claim("roles", "Editor")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenUserHasBothRoleClaims_ShouldNotDuplicateRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin"),
			new Claim("roles", "Admin") // Duplicate
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role && c.Value == "Admin").Should().HaveCount(1);
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenCustomRoleClaimIsEmpty_ShouldNotAddRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().BeEmpty();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenRoleClaimHasWhitespace_ShouldTrimRoles()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", " Admin , Editor ")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.IsInRole(" Admin ").Should().BeFalse();
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_ShouldPreserveOriginalClaims()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim(ClaimTypes.Email, "test@example.com"),
			new Claim("custom_claim", "custom_value")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.FindFirst(ClaimTypes.Name)?.Value.Should().Be("tester");
		result.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
		result.User.FindFirst("custom_claim")?.Value.Should().Be("custom_value");
	}

	[Fact]
	public async Task GetAuthenticationStateAsync_WhenMultipleRolesInCustomClaim_ShouldSplitAndAddAll()
	{
		// Arrange
		var httpContext = new DefaultHttpContext();
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "tester"),
			new Claim("https://articlesite.com/roles", "Admin,Editor,Viewer")
		};
		var identity = new ClaimsIdentity(claims, "TestAuth");
		httpContext.User = new ClaimsPrincipal(identity);
		_mockHttpContextAccessor.HttpContext.Returns(httpContext);

		// Act
		var result = await _provider.GetAuthenticationStateAsync();

		// Assert
		result.User.IsInRole("Admin").Should().BeTrue();
		result.User.IsInRole("Editor").Should().BeTrue();
		result.User.IsInRole("Viewer").Should().BeTrue();
		result.User.Claims.Where(c => c.Type == ClaimTypes.Role).Should().HaveCount(3);
	}
}