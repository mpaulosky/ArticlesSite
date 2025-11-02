using Web.Components.Features.Articles.ArticleCreate;
using Web.Components.Features.Categories.CategoriesList;

// Ensure this is included for Result<T>

namespace Web.Tests.Unit.Components.Features.Articles.ArticleCreate;

[ExcludeFromCodeCoverage]
public class CreateComponentTests : TestContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));
		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		var cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("LoadingComponent"));
		cut.Markup.Should().Contain("LoadingComponent");
	}

	[Fact]
	public void RendersErrorAlert_WhenErrorMessageIsSet()
	{
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));
		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		var cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("Failed to create article."));
		cut.Markup.Should().Contain("Failed to create article.");
	}

	[Fact]
	public void RendersCreateForm_WhenNotLoadingOrError()
	{
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));
		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		var cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("modern-card"));
		cut.Markup.Should().Contain("modern-card");
		cut.Markup.Should().Contain("Create Article");
	}

}