using System.Text.Json;

namespace Web.Components.Features.Articles.Models;

[ExcludeFromCodeCoverage]
public class ConcurrencyConflictResponseDtoTests
{
	[Fact]
	public void DefaultConstructor_SetsExpectedDefaults()
	{
		var dto = new ConcurrencyConflictResponseDto();

		dto.Error.Should().BeNull();
		dto.Code.Should().Be(0);
		dto.ServerVersion.Should().Be(0);
		dto.ServerArticle.Should().BeNull();
		dto.ChangedFields.Should().NotBeNull().And.BeEmpty();
		dto.Version.Should().Be(dto.ServerVersion);
	}

	[Fact]
	public void ParameterConstructor_PopulatesProperties()
	{
		var article = new ArticleDto();
		var fields = new List<string> { "Title", "Content" };

		var dto = new ConcurrencyConflictResponseDto("Conflict occurred", 42, 7, article, fields);

		dto.Error.Should().Be("Conflict occurred");
		dto.Code.Should().Be(42);
		dto.ServerVersion.Should().Be(7);
		dto.ServerArticle.Should().BeSameAs(article);
		dto.ChangedFields.Should().HaveCount(2).And.ContainInOrder("Title", "Content");
		dto.Version.Should().Be(7);
	}

	[Fact]
	public void ParameterConstructor_WithNullChangedFields_ProducesEmptyList()
	{
		var dto = new ConcurrencyConflictResponseDto("err", 1, 2, null, null);

		dto.ChangedFields.Should().NotBeNull().And.BeEmpty();
	}

	[Fact]
	public void ChangedFields_AreCopied_NotReferenced()
	{
		var source = new List<string> { "A", "B" };
		var dto = new ConcurrencyConflictResponseDto(null, 0, 0, null, source);

		source.Add("C");

		dto.ChangedFields.Should().Contain("A", "B").And.NotContain("C");
	}

	[Fact]
	public void JsonSerialization_ProducesExpectedPropertyNamesAndValues()
	{
		var article = new ArticleDto { Title = "Hello" };
		var dto = new ConcurrencyConflictResponseDto("err", 13, 99, article, [ "Title" ]);

		var json = JsonSerializer.Serialize(dto);

		json.Should().Contain("\"error\":").And.Contain("\"code\":").And.Contain("\"serverVersion\":").And.Contain("\"serverArticle\":").And.Contain("\"changedFields\":");
		json.Should().Contain("err");
		json.Should().Contain("99");
		json.Should().Contain("Hello");
	}

	[Fact]
	public void JsonDeserialization_PopulatesProperties()
	{
		var json = "{\"error\":\"e\",\"code\":5,\"serverVersion\":10,\"serverArticle\":null,\"changedFields\":[\"Title\"]}";
		var dto = JsonSerializer.Deserialize<ConcurrencyConflictResponseDto>(json);

		dto.Should().NotBeNull();
		dto.Error.Should().Be("e");
		dto.Code.Should().Be(5);
		dto.ServerVersion.Should().Be(10);
		dto.ChangedFields.Should().ContainSingle("Title");
		// Version aliases server version
		dto.Version.Should().Be(10);
	}
}
