using Details = Web.Components.Features.Articles.ArticleDetails.Details;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleDetails;

[ExcludeFromCodeCoverage]
public class DetailsComponentTests : BunitContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange DI: use the actual handler the component depends on
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		// Simulate a pending load so the component stays in the loading state
		var tcs = new TaskCompletionSource<Result<ArticleDto>>();
		handler.HandleAsync(id).Returns(_ => tcs.Task);

		var cut = Render<Details>(parameters => parameters
			.Add(p => p.Id, id.ToString())
		);

		// Assert: Loading UI should be visible
		cut.Markup.Should().Contain("Loading...");
	}

	[Fact]
	public void RendersErrorAlert_WhenArticleIsNull()
	{
		// Arrange DI
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");
		handler.HandleAsync(id).Returns(Result.Fail<ArticleDto>("Article not found."));

		var cut = Render<Details>(parameters => parameters
			.Add(p => p.Id, id.ToString())
		);

		// Assert
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersCard_WhenArticleIsPresent()
	{
		// Arrange DI
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var article = new ArticleDto(
				ObjectId.GenerateNewId(),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"<p>Test Content</p>",
				"https://example.com/image.jpg",
				new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author"),
				new Category { CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		handler.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Details>(parameters => parameters
			.Add(p => p.Id, article.Id.ToString())
		);

		// Assert
		cut.Markup.Should().Contain("container-card");
		cut.Markup.Should().Contain("Test Title");
		cut.Markup.Should().Contain("Test Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

}
