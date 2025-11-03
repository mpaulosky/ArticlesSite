// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Web.Components.Features.Articles.ArticleCreate;
using Web.Components.Features.Categories.CategoriesList;

using Create = Web.Components.Features.Articles.ArticleCreate.Create;

// Ensure this is included for Result<T>

namespace Web.Tests.Unit.Components.Features.Articles.ArticleCreate;

[ExcludeFromCodeCoverage]
public class CreateComponentTests : TestContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Register all services BEFORE rendering the component
		GetCategories.IGetCategoriesHandler? getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();

		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));

		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		// Now safe to render the component
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("LoadingComponent"));
		cut.Markup.Should().Contain("LoadingComponent");
	}

	[Fact]
	public void RendersErrorAlert_WhenErrorMessageIsSet()
	{
		// Register all services BEFORE rendering the component
		GetCategories.IGetCategoriesHandler? getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();

		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));

		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		// Now safe to render the component
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("Failed to create article."));
		cut.Markup.Should().Contain("Failed to create article.");
	}

	[Fact]
	public void RendersCreateForm_WhenNotLoadingOrError()
	{
		// Register all services BEFORE rendering the component
		GetCategories.IGetCategoriesHandler? getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();

		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));

		Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), getCategoriesHandler);

		Services.AddSingleton(typeof(CreateArticle.ICreateArticleHandler),
				Substitute.For<CreateArticle.ICreateArticleHandler>());

		// Now safe to render the component
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.WaitForState(() => cut.Markup.Contains("modern-card"));
		cut.Markup.Should().Contain("modern-card");
		cut.Markup.Should().Contain("Create Article");
	}

}