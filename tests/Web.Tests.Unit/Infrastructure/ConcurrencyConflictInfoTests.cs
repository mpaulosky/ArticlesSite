namespace Web.Infrastructure;

[ExcludeFromCodeCoverage]
public class ConcurrencyConflictInfoTests
{
	[Fact]
	public void Constructor_SetsProperties_WhenProvided()
	{
		// Arrange
		var article = new ArticleDto { Version = 1, Title = "Title" };
		var originalChanged = new List<string> { "Title", "Content" };

		// Act
		var info = new ConcurrencyConflictInfo(42, article, originalChanged);

		// Mutate the original collection to assert a copy was made
		originalChanged.Add("NewField");

		// Assert
		info.ServerVersion.Should().Be(42);
		info.ServerArticle.Should().BeSameAs(article);
		info.ChangedFields.Should().ContainInOrder(new[] { "Title", "Content" });
		info.ChangedFields.Should().NotContain("NewField");
		// Ensure the ChangedFields collection is immutable from the consumer's perspective (IReadOnlyList)
		info.ChangedFields.Should().BeAssignableTo<IReadOnlyList<string>>();
	}

	[Fact]
	public void Constructor_DefaultsChangedFields_ToEmpty_WhenNullPassed()
	{
		// Arrange & Act
		var info = new ConcurrencyConflictInfo(7, null, null);

		// Assert
		info.ServerVersion.Should().Be(7);
		info.ServerArticle.Should().BeNull();
		info.ChangedFields.Should().NotBeNull();
		info.ChangedFields.Should().BeEmpty();
	}
}
