// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ProfileTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using System.Security.Claims;

using Bunit;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Web.Components.User;

/// <summary>
/// Unit tests for Profile component logic and rendering.
/// Tests cover claims extraction, role parsing, and component rendering.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProfileTests : BunitContext
{
	[Fact]
	public void Profile_HasAuthorizeAttribute()
	{
		// Act
		var attributes = typeof(Profile).GetCustomAttributes(typeof(AuthorizeAttribute), false);

		// Assert
		attributes.Should().NotBeEmpty("Profile page should require authorization");
		attributes.Should().HaveCount(1);
	}

	#region GetAllRoleClaims Tests

	[Fact]
	public void GetAllRoleClaims_ReturnsEmptyList_WhenNoClaims()
	{
		// Arrange
		var user = new ClaimsPrincipal(new ClaimsIdentity());

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().BeEmpty();
	}

	[Fact]
	public void GetAllRoleClaims_ReturnsSingleRole()
	{
		// Arrange
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(1);
		result.First().Should().Be("Admin");
	}

	[Fact]
	public void GetAllRoleClaims_ReturnsMultipleRoles()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim(ClaimTypes.Role, "Editor"),
			new Claim(ClaimTypes.Role, "Viewer")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
		result.Should().Contain("Viewer");
	}

	[Fact]
	public void GetAllRoleClaims_HandlesAlternativeRoleClaimTypes()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim("role", "Editor"),
			new Claim("roles", "Viewer")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
		result.Should().Contain("Viewer");
	}

	[Fact]
	public void GetAllRoleClaims_FiltersEmptyRoles()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim(ClaimTypes.Role, ""),
			new Claim(ClaimTypes.Role, "   "),
			new Claim(ClaimTypes.Role, "Editor")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(2);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
		result.Should().NotContain("");
		result.Should().NotContain("   ");
	}

	[Fact]
	public void GetAllRoleClaims_ReturnsDistinctRoles()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim(ClaimTypes.Role, "Editor"),
			new Claim(ClaimTypes.Role, "Admin")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(2);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
	}

	[Fact]
	public void GetAllRoleClaims_IsCaseInsensitive()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim("ROLE", "Editor"),
			new Claim("RoLe", "Viewer")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
		result.Should().Contain("Viewer");
	}

	[Fact]
	public void GetAllRoleClaims_PreservesRoleOrder_WithinClaimType()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Zebra"),
			new Claim(ClaimTypes.Role, "Apple"),
			new Claim(ClaimTypes.Role, "Mango")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(3);
		// Should preserve order from claims (not alphabetical)
		result[0].Should().Be("Zebra");
		result[1].Should().Be("Apple");
		result[2].Should().Be("Mango");
	}

	[Fact]
	public void GetAllRoleClaims_ComplexScenario_WithMixedRoles()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Role, "Admin"),
			new Claim(ClaimTypes.Role, ""),
			new Claim("role", "Editor"),
			new Claim("roles", "Viewer"),
			new Claim(ClaimTypes.Role, "Admin"), // duplicate
			new Claim("ROLE", "SuperUser"),
			new Claim(ClaimTypes.Name, "Should be ignored")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetAllRoleClaims(user);

		// Assert
		result.Should().NotBeNull();
		result.Should().HaveCount(4);
		result.Should().Contain("Admin");
		result.Should().Contain("Editor");
		result.Should().Contain("Viewer");
		result.Should().Contain("SuperUser");
	}

	#endregion

	#region GetClaimValue Tests

	[Fact]
	public void GetClaimValue_ReturnsValue_WhenClaimExists()
	{
		// Arrange
		var claims = new[] { new Claim("custom-type", "value1") };
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetClaimValue(user, "custom-type");

		// Assert
		result.Should().Be("value1");
	}

	[Fact]
	public void GetClaimValue_ReturnsFirstNonEmptyValue()
	{
		// Arrange - skips first empty claim type, returns value from second claim type
		var claims = new[]
		{
			new Claim("email", ""),
			new Claim("name", "John Doe")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act - email claim is empty, so it should skip and return name
		var result = Profile.GetClaimValue(user, "email", "name");

		// Assert
		result.Should().Be("John Doe");
	}

	[Fact]
	public void GetClaimValue_SkipsEmptyValues()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Email, ""),
			new Claim("alt-email", "user@example.com")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetClaimValue(user, ClaimTypes.Email, "alt-email");

		// Assert
		result.Should().Be("user@example.com");
	}

	[Fact]
	public void GetClaimValue_ReturnsNA_WhenNoClaimsFound()
	{
		// Arrange
		var claims = new[] { new Claim(ClaimTypes.Name, "TestUser") };
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetClaimValue(user, "non-existent-type");

		// Assert
		result.Should().Be("N/A");
	}

	[Fact]
	public void GetClaimValue_ReturnsNA_WhenClaimsListEmpty()
	{
		// Arrange
		var user = new ClaimsPrincipal(new ClaimsIdentity());

		// Act
		var result = Profile.GetClaimValue(user, ClaimTypes.Email);

		// Assert
		result.Should().Be("N/A");
	}

	[Fact]
	public void GetClaimValue_HandlesSingleClaimType()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "TestUser"),
			new Claim(ClaimTypes.Email, "test@example.com")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetClaimValue(user, ClaimTypes.Email);

		// Assert
		result.Should().Be("test@example.com");
	}

	[Fact]
	public void GetClaimValue_HandlesMultipleClaimTypes_And_ReturnsFirstMatch()
	{
		// Arrange
		var claims = new[]
		{
			new Claim(ClaimTypes.Name, "TestUser"),
			new Claim("email", "wrong@example.com"),
			new Claim(ClaimTypes.Email, "john@example.com")
		};
		var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

		// Act
		var result = Profile.GetClaimValue(user, ClaimTypes.Email, "email");

		// Assert
		result.Should().Be("john@example.com");
	}

	#endregion
}

