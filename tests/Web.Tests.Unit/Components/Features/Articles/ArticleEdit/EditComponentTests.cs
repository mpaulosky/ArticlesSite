using Web.Components.Features.Articles.ArticleEdit;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleEdit;

// Modernized for bUnit v2 and helper-based authentication
[ExcludeFromCodeCoverage]
public class EditComponentTests : BunitContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		var tcsCats = new TaskCompletionSource<Result<IEnumerable<CategoryDto>>>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(_ => tcsCats.Task);

		var tcsArticle = new TaskCompletionSource<Result<ArticleDto>>();
		getArticle.HandleAsync(id).Returns(_ => tcsArticle.Task);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		cut.Markup.Should().Contain("Loading");
	}

	[Fact]
	public void RendersErrorAlert_WhenArticleLoadFails()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok(Enumerable.Empty<CategoryDto>()));

		getArticle.HandleAsync(id).Returns(Result.Fail<ArticleDto>("Article not found."));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		cut.Markup.Should().Contain("Unable to load article");
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersEditForm_WhenEditModelIsPresent()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var catId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto>
		{
			new() { Id = catId, CategoryName = "Tech", IsArchived = false }
		};
		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok<IEnumerable<CategoryDto>>(categories));

		var article = new ArticleDto(
				ObjectId.Parse("507f1f77bcf86cd799439011"),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"Test Content",
				"https://example.com/image.jpg",
				new AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		cut.Markup.Should().Contain("Title");
		cut.Markup.Should().Contain("Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

	[Fact]
	public void RendersPageHeading_WhenPageLoads()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var catId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto>
		{
			new() { Id = catId, CategoryName = "Tech", IsArchived = false }
		};
		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok<IEnumerable<CategoryDto>>(categories));

		var article = new ArticleDto(
				ObjectId.Parse("507f1f77bcf86cd799439011"),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"Test Content",
				"https://example.com/image.jpg",
				new AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		cut.Markup.Should().Contain("Edit Article");
	}

}
