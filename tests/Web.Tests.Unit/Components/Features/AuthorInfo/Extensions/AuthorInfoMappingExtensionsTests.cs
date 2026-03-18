// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     AuthorInfoMappingExtensionsTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

namespace Web.Components.Features.AuthorInfo.Extensions;

/// <summary>
/// Unit tests for <see cref="AuthorInfoMappingExtensions"/> helper methods.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuthorInfoMappingExtensionsTests
{

	[Fact]
	public void Copy_WithValidAuthorInfo_CreatesNewInstance()
	{
		// Arrange
		var original = new global::Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user123", "John Doe");

		// Act
		var copy = original.Copy();

		// Assert
		copy.Should().NotBeNull();
		copy.UserId.Should().Be("user123");
		copy.Name.Should().Be("John Doe");
		copy.Should().NotBeSameAs(original);
	}

	[Fact]
	public void Copy_WithValidAuthorInfo_CreatesIndependentCopy()
	{
		// Arrange
		var original = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user456", "Jane Smith");

		// Act
		var copy = original.Copy();

		// Assert - Records are immutable, so test that copy has the same values
		copy.UserId.Should().Be(original.UserId);
		copy.Name.Should().Be(original.Name);
	}

	[Fact]
	public void Copy_WithNullAuthorInfo_ReturnsEmptyAuthorInfo()
	{
		// Arrange
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo? original = null;

		// Act
		var copy = original.Copy();

		// Assert
		copy.Should().NotBeNull();
		copy.UserId.Should().Be(string.Empty);
		copy.Name.Should().Be(string.Empty);
		copy.Should().Be(Web.Components.Features.AuthorInfo.Entities.AuthorInfo.Empty);
	}

	[Fact]
	public void Copy_WithEmptyAuthorInfo_ReturnsEmptyAuthorInfo()
	{
		// Arrange
		var original = Web.Components.Features.AuthorInfo.Entities.AuthorInfo.Empty;

		// Act
		var copy = original.Copy();

		// Assert
		copy.Should().NotBeNull();
		copy.UserId.Should().Be(string.Empty);
		copy.Name.Should().Be(string.Empty);
	}

	[Fact]
	public void Copy_PreservesUserIdAndName()
	{
		// Arrange
		var userId = "complex-user-id-12345";
		var name = "Complex Author Name With Spaces";
		var original = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo(userId, name);

		// Act
		var copy = original.Copy();

		// Assert
		copy.UserId.Should().Be(userId);
		copy.Name.Should().Be(name);
	}

	[Fact]
	public void IsEmpty_WithNullAuthorInfo_ReturnsTrue()
	{
		// Arrange
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo? author = null;

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_WithEmptyAuthorInfo_ReturnsTrue()
	{
		// Arrange
		var author = Web.Components.Features.AuthorInfo.Entities.AuthorInfo.Empty;

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_WithEmptyUserIdAndName_ReturnsTrue()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo(string.Empty, string.Empty);

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_WithWhitespaceUserIdAndName_ReturnsTrue()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("   ", "   ");

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_WithValidUserId_ReturnsFalse()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user123", string.Empty);

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeFalse();
	}

	[Fact]
	public void IsEmpty_WithValidName_ReturnsFalse()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo(string.Empty, "John Doe");

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeFalse();
	}

	[Fact]
	public void IsEmpty_WithValidAuthorInfo_ReturnsFalse()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user123", "John Doe");

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeFalse();
	}

	[Fact]
	public void IsEmpty_WithOnlyWhitespaceUserId_ReturnsTrue()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("\t\n ", "John Doe");

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_WithOnlyWhitespaceName_ReturnsTrue()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user123", "\t\n ");

		// Act
		var isEmpty = author.IsEmpty();

		// Assert
		isEmpty.Should().BeTrue();
	}

	[Fact]
	public void Copy_And_IsEmpty_Combined_WorksCorrectly()
	{
		// Arrange
		Web.Components.Features.AuthorInfo.Entities.AuthorInfo? nullAuthor = null;
		var validAuthor = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user789", "Alice Johnson");

		// Act
		var copiedNull = nullAuthor.Copy();
		var isNullEmpty = copiedNull.IsEmpty();
		var copiedValid = validAuthor.Copy();
		var isValidEmpty = copiedValid.IsEmpty();

		// Assert
		isNullEmpty.Should().BeTrue();
		isValidEmpty.Should().BeFalse();
		copiedValid.UserId.Should().Be("user789");
		copiedValid.Name.Should().Be("Alice Johnson");
	}

	[Fact]
	public void IsEmpty_MultipleCallsConsistent_ReturnsConsistentResults()
	{
		// Arrange
		var author = new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user123", "John Doe");

		// Act
		var isEmpty1 = author.IsEmpty();
		var isEmpty2 = author.IsEmpty();
		var isEmpty3 = author.IsEmpty();

		// Assert
		isEmpty1.Should().Be(isEmpty2);
		isEmpty2.Should().Be(isEmpty3);
		isEmpty1.Should().BeFalse();
	}

}
