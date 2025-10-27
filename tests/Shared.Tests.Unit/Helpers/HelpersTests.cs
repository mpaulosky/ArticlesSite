//=======================================================
//Copyright (c) 2025. All rights reserved.
//File Name :     HelpersTests.cs
//Company :       mpaulosky
//Author :        Matthew Paulosky
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using Shared.Helpers;

namespace Shared.Tests.Unit.Helpers;

/// <summary>
///   Unit tests for the <see cref="Helpers" /> class.
/// </summary>
public class HelpersTests
{

	[Fact]
	public void GetStaticDate_ShouldReturnConsistentDate()
	{
		// Arrange
		DateTimeOffset expectedDate = new (2025, 1, 1, 8, 0, 0, TimeSpan.Zero);

		// Act
		DateTimeOffset result1 = Shared.Helpers.Helpers.GetStaticDate();
		DateTimeOffset result2 = Shared.Helpers.Helpers.GetStaticDate();

		// Assert
		result1.Should().Be(expectedDate);
		result2.Should().Be(expectedDate);
		result1.Should().Be(result2);
	}

	[Theory]
	[InlineData("Hello World", "hello_world")]
	[InlineData("Test Article", "test_article")]
	[InlineData("C# Programming", "c_programming")]
	[InlineData("ASP.NET Core", "asp_net_core")]
	[InlineData("Multiple   Spaces", "multiple_spaces")]
	[InlineData("Special@Characters#Here!", "special_characters_here_")]
	[InlineData("123 Numbers", "123_numbers")]
	[InlineData("UPPERCASE", "uppercase")]
	[InlineData("  Leading and Trailing  ", "leading_and_trailing")]
	public void GetSlug_ShouldConvertToUrlFriendlySlug(string input, string expected)
	{
		// Arrange & Act
		string result = input.GetSlug();

		// Assert
		result.Should().Be(expected);
	}

	[Fact]
	public void GetSlug_ShouldBeUrlEncoded()
	{
		// Arrange
		const string input = "Test Article";

		// Act
		string result = input.GetSlug();

		// Assert
		result.Should().NotContain(" ");
		result.ToCharArray().Should().OnlyContain(c => !char.IsUpper(c));
	}

	[Fact]
	public void GetRandomCategoryName_ShouldReturnValidCategory()
	{
		// Arrange
		List<string> validCategories = new()
		{
				MyCategories.First,
				MyCategories.Second,
				MyCategories.Third,
				MyCategories.Fourth,
				MyCategories.Fifth,
				MyCategories.Sixth,
				MyCategories.Seventh,
				MyCategories.Eighth,
				MyCategories.Ninth
		};

		// Act
		string result = Shared.Helpers.Helpers.GetRandomCategoryName();

		// Assert
		result.Should().NotBeNullOrEmpty();
		validCategories.Should().Contain(result);
	}

	[Fact]
	public void GetRandomCategoryName_CalledMultipleTimes_ShouldReturnValidCategories()
	{
		// Arrange
		List<string> validCategories = new()
		{
				MyCategories.First,
				MyCategories.Second,
				MyCategories.Third,
				MyCategories.Fourth,
				MyCategories.Fifth,
				MyCategories.Sixth,
				MyCategories.Seventh,
				MyCategories.Eighth,
				MyCategories.Ninth
		};

		// Act - Call multiple times to test randomness
		List<string> results = new ();

		for (int i = 0; i < 20; i++)
		{
			results.Add(Shared.Helpers.Helpers.GetRandomCategoryName());
		}

		// Assert - All results should be valid
		foreach (string result in results)
		{
			validCategories.Should().Contain(result);
		}
	}

}