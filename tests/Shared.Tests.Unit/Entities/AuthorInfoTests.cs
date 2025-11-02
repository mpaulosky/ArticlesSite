//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     AuthorInfoTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

namespace Shared.Tests.Unit.Entities;

/// <summary>
///   Unit tests for the <see cref="AuthorInfo" /> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorInfoTests
{

	[Fact]
	public void Constructor_WithParameters_ShouldSetProperties()
	{
		// Arrange
		const string userId = "auth0|123456";
		const string name = "John Doe";

		// Act
		AuthorInfo authorInfo = new(userId, name);

		// Assert
		authorInfo.UserId.Should().Be(userId);
		authorInfo.Name.Should().Be(name);
	}

	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		AuthorInfo authorInfo = new();

		// Assert
		authorInfo.UserId.Should().Be(string.Empty);
		authorInfo.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		AuthorInfo empty = AuthorInfo.Empty;

		// Assert
		empty.UserId.Should().Be(string.Empty);
		empty.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void Record_Equality_ShouldCompareBothProperties()
	{
		// Arrange
		AuthorInfo author1 = new("auth0|123", "John Doe");
		AuthorInfo author2 = new("auth0|123", "John Doe");
		AuthorInfo author3 = new("auth0|456", "Jane Doe");

		// Act & Assert
		author1.Should().Be(author2);
		author1.Should().NotBe(author3);
	}

	[Fact]
	public void With_Expression_ShouldCreateNewInstance()
	{
		// Arrange
		AuthorInfo original = new("auth0|123", "John Doe");

		// Act
		AuthorInfo modified = original with { Name = "Jane Doe" };

		// Assert
		modified.UserId.Should().Be(original.UserId);
		modified.Name.Should().Be("Jane Doe");
		original.Name.Should().Be("John Doe"); // Original unchanged
	}

}