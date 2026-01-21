//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     AuthorInfoTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Web.Tests.Unit
//=======================================================

namespace Web.Tests.Unit.Components.Features.AuthorInfo;

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
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo authorInfo = new(userId, name);

		// Assert
		authorInfo.UserId.Should().Be(userId);
		authorInfo.Name.Should().Be(name);
	}

	[Fact]
	public void Constructor_Parameterless_ShouldSetDefaultValues()
	{
		// Arrange & Act
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo authorInfo = new();

		// Assert
		authorInfo.UserId.Should().Be(string.Empty);
		authorInfo.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void Empty_ShouldReturnEmptyInstance()
	{
		// Arrange & Act
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo empty = Web.Components.Features.AuthorInfo.Entities
				.AuthorInfo.Empty;

		// Assert
		empty.UserId.Should().Be(string.Empty);
		empty.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void Record_Equality_ShouldCompareBothProperties()
	{
		// Arrange
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo author1 = new("auth0|123", "John Doe");
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo author2 = new("auth0|123", "John Doe");
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo author3 = new("auth0|456", "Jane Doe");

		// Act & Assert
		author1.Should().Be(author2);
		author1.Should().NotBe(author3);
	}

	[Fact]
	public void With_Expression_ShouldCreateNewInstance()
	{
		// Arrange
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo original = new("auth0|123", "John Doe");

		// Act
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo modified = original with { Name = "Jane Doe" };

		// Assert
		modified.UserId.Should().Be(original.UserId);
		modified.Name.Should().Be("Jane Doe");
		original.Name.Should().Be("John Doe"); // Original unchanged
	}

}
