using Bunit;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Web.Components.Features.Articles.ArticleDetails;

using Shared.Interfaces;
using Shared.Entities;

using Xunit;

using Details = Web.Components.Features.Articles.ArticleDetails.Details;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleDetails;

public class DetailsComponentTests : TestContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		Services.AddSingleton(typeof(IArticleRepository),
				Substitute.For<IArticleRepository>());

		// Arrange
		var cut = RenderComponent<Details>(parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
		);

		cut.Instance.GetType().GetProperty("_isLoading")!.SetValue(cut.Instance, true);
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("LoadingComponent");
	}

	[Fact]
	public void RendersErrorAlert_WhenArticleIsNull()
	{
		Services.AddSingleton(typeof(IArticleRepository), Substitute.For<IArticleRepository>());

		// Arrange
		var cut = RenderComponent<Details>(parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
		);

		cut.Instance.GetType().GetProperty("_isLoading")!.SetValue(cut.Instance, false);
		cut.Instance.GetType().GetProperty("_article")!.SetValue(cut.Instance, null);
		cut.Instance.GetType().GetProperty("_errorMessage")!.SetValue(cut.Instance, "Article not found.");
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersModernCard_WhenArticleIsPresent()
	{
		Services.AddSingleton(typeof(IArticleRepository), Substitute.For<IArticleRepository>());

		// Arrange
		var article = new ArticleDto(
				ObjectId.GenerateNewId(),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"<p>Test Content</p>",
				"https://example.com/image.jpg",
				new AuthorInfo("user1", "Test Author"),
				new Category { CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		var cut = RenderComponent<Details>(parameters => parameters
				.Add(p => p.Id, article.Id.ToString())
		);

		cut.Instance.GetType().GetProperty("_isLoading")!.SetValue(cut.Instance, false);
		cut.Instance.GetType().GetProperty("_article")!.SetValue(cut.Instance, article);
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("modern-card");
		cut.Markup.Should().Contain("Test Title");
		cut.Markup.Should().Contain("Test Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

}
