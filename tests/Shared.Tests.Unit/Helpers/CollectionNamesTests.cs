//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     CollectionNamesTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using Shared.Abstractions;
using Shared.Helpers;

namespace Shared.Tests.Unit.Helpers;

/// <summary>
///   Unit tests for the <see cref="CollectionNames" /> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class CollectionNamesTests
{

	[Fact]
	public void GetCollectionName_WithArticle_ShouldReturnArticlesCollection()
	{
		// Arrange
		const string entityName = "Article";

		// Act
		Result<string> result = CollectionNames.GetCollectionName(entityName);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be("articles");
		result.Error.Should().BeNull();
	}

	[Fact]
	public void GetCollectionName_WithCategory_ShouldReturnCategoriesCollection()
	{
		// Arrange
		const string entityName = "Category";

		// Act
		Result<string> result = CollectionNames.GetCollectionName(entityName);

		// Assert
		result.Success.Should().BeTrue();
		result.Value.Should().Be("categories");
		result.Error.Should().BeNull();
	}

	[Theory]
	[InlineData("InvalidEntity")]
	[InlineData("article")]
	[InlineData("category")]
	[InlineData("Unknown")]
	[InlineData("")]
	public void GetCollectionName_WithInvalidEntityName_ShouldReturnFailure(string? invalidEntityName)
	{
		// Arrange & Act
		Result<string> result = CollectionNames.GetCollectionName(invalidEntityName);

		// Assert
		result.Success.Should().BeFalse();
		result.Failure.Should().BeTrue();
		result.Error.Should().Be("Invalid entity name provided.");
		result.Value.Should().BeNull();
	}

	[Fact]
	public void GetCollectionName_WithNull_ShouldReturnFailure()
	{
		// Arrange & Act
		Result<string> result = CollectionNames.GetCollectionName(null);

		// Assert
		result.Success.Should().BeFalse();
		result.Error.Should().Be("Invalid entity name provided.");
	}

}
