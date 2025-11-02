// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ProfileTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Tests.Unit.Components.Pages;

/// <summary>
/// Unit tests for Profile component
/// </summary>
[ExcludeFromCodeCoverage]
public class ProfileTests
{

	[Fact]
	public void GetClaimValue_WithFirstClaimType_ShouldReturnValue()
	{
		// Arrange
		var claims = new[] { new Claim("name", "John Doe"), new Claim("email", "john@example.com") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		result.Should().Be("John Doe");
	}

	[Fact]
	public void GetClaimValue_WithSecondClaimType_ShouldReturnValue()
	{
		// Arrange
		var claims = new[] { new Claim("nickname", "Johnny"), new Claim("email", "john@example.com") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		result.Should().Be("Johnny");
	}

	[Fact]
	public void GetClaimValue_WhenNoMatchingClaim_ShouldReturnNA()
	{
		// Arrange
		var claims = new[] { new Claim("email", "john@example.com") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		result.Should().Be("N/A");
	}

	[Fact]
	public void GetClaimValue_WithEmptyClaimValue_ShouldContinueToNextClaimType()
	{
		// Arrange
		var claims = new[] { new Claim("name", ""), new Claim("nickname", "Johnny") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		result.Should().Be("Johnny");
	}

	[Fact]
	public void GetClaimValue_WithNullClaimValue_ShouldContinueToNextClaimType()
	{
		// Arrange
		var claims = new[] { new Claim("nickname", "Johnny") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		result.Should().Be("Johnny");
	}

	[Fact]
	public void GetClaimValue_WithSingleClaimType_ShouldReturnValue()
	{
		// Arrange
		var claims = new[] { new Claim(ClaimTypes.Email, "john@example.com") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, ClaimTypes.Email);

		// Assert
		result.Should().Be("john@example.com");
	}

	[Fact]
	public void GetClaimValue_WithMultipleClaimTypes_ShouldReturnFirstMatch()
	{
		// Arrange
		var claims = new[]
		{
				new Claim("given_name", "John"), new Claim(ClaimTypes.Name, "John Doe"), new Claim("nickname", "Johnny")
		};

		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname", "given_name", ClaimTypes.Name);

		// Assert
		result.Should().Be("Johnny");
	}

	[Fact]
	public void GetClaimValue_WithWhitespaceClaimValue_ShouldReturnWhitespace()
	{
		// Arrange
		var claims = new[] { new Claim("name", "   "), new Claim("nickname", "Johnny") };
		var identity = new ClaimsIdentity(claims, "TestAuth");
		var user = new ClaimsPrincipal(identity);

		// Act
		var result = InvokeGetClaimValue(user, "name", "nickname");

		// Assert
		// The current implementation returns whitespace as valid since it only checks !string.IsNullOrEmpty
		result.Should().Be("   ");
	}

	// Helper method to test the private static GetClaimValue method via reflection
	private static string InvokeGetClaimValue(ClaimsPrincipal user, params string[] claimTypes)
	{
		var profileType = typeof(Web.Components.User.Profile);

		var method = profileType.GetMethod("GetClaimValue",
				BindingFlags.NonPublic | BindingFlags.Static);

		method.Should().NotBeNull("GetClaimValue method should exist");

		var result = method!.Invoke(null, new object[] { user, claimTypes });

		return (string)result!;
	}

}
