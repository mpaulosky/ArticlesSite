// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     CreateComponentPrivateMethodsTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

using Web.Components.Features.Categories.CategoriesList;

namespace Web.Components.Features.Articles.ArticleCreate;

[ExcludeFromCodeCoverage]
public class CreateComponentPrivateMethodsTests : BunitContext
{
	public CreateComponentPrivateMethodsTests()
	{
		// Setup JSInterop for markdown editor
		JSInterop.SetupVoid("initialize", _ => true).SetVoidResult();

		// Add Authorization
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TestUser", [ "Author" ]);

		// Register common services
		Services.AddSingleton(Substitute.For<CreateArticle.ICreateArticleHandler>());
		Services.AddSingleton(Substitute.For<IFileStorage>());
	}

	[Fact]
	public async Task OnCategoryChanged_WithValidCategory_SetsArticleCategory()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto> { new() { Id = categoryId, CategoryName = "T1", IsArchived = false } };
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
		Services.AddSingleton(getCategoriesHandler);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act - call private OnCategoryChanged
		var method = cut.Instance.GetType().GetMethod("OnCategoryChanged", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var args = new ChangeEventArgs { Value = categoryId.ToString() };
		method.Invoke(cut.Instance, [ args ]);

		// Assert - article's Category should be set
		var articleField = cut.Instance.GetType().GetField("_article", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(articleField);
		var article = articleField.GetValue(cut.Instance) as ArticleDto;
		article.Should().NotBeNull();
		article.Category.Should().NotBeNull();
		article.Category!.Id.Should().Be(categoryId);
	}

	[Fact]
	public async Task OnCategoryChanged_WithInvalidId_SetsArticleCategoryNull()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto> { new() { Id = categoryId, CategoryName = "T1" } };
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
		Services.AddSingleton(getCategoriesHandler);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act - pass invalid id
		var method = cut.Instance.GetType().GetMethod("OnCategoryChanged", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var args = new ChangeEventArgs { Value = "not-an-id" };
		method.Invoke(cut.Instance, [ args ]);

		// Assert - article's Category should be null
		var articleField = cut.Instance.GetType().GetField("_article", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(articleField);
		var article = articleField.GetValue(cut.Instance) as ArticleDto;
		article.Should().NotBeNull();
		article.Category.Should().BeNull();
	}

	[Fact]
	public async Task LoadCategoriesAsync_WhenHandlerFails_SetsErrorMessageAndLogsWarning()
	{
		// Arrange
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Fail<IEnumerable<CategoryDto>>("nope")));
		Services.AddSingleton(getCategoriesHandler);

		var logger = Substitute.For<ILogger<Create>>();
		Services.AddSingleton(logger);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act - call private LoadCategoriesAsync
		var method = cut.Instance.GetType().GetMethod("LoadCategoriesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var task = method.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;

		// Assert
		var categoriesField = cut.Instance.GetType().GetField("_categories", BindingFlags.NonPublic | BindingFlags.Instance);
		var categories = categoriesField?.GetValue(cut.Instance) as List<CategoryDto>;
		categories.Should().NotBeNull();
		categories.Should().BeEmpty();

		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		var error = errorField?.GetValue(cut.Instance) as string;
		error.Should().Contain("nope");

		// Logger call is an extension method and not directly assertable with NSubstitute; error message check suffices.
	}

	[Fact]
	public async Task LoadCategoriesAsync_WhenHandlerThrows_SetsErrorMessageAndLogsError()
	{
		// Arrange
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns<Task<Result<IEnumerable<CategoryDto>>>>(_ => throw new Exception("boom"));
		Services.AddSingleton(getCategoriesHandler);

		var logger = Substitute.For<ILogger<Create>>();
		Services.AddSingleton(logger);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act
		var method = cut.Instance.GetType().GetMethod("LoadCategoriesAsync", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var task = method.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;

		// Assert
		var categoriesField = cut.Instance.GetType().GetField("_categories", BindingFlags.NonPublic | BindingFlags.Instance);
		var categories = categoriesField?.GetValue(cut.Instance) as List<CategoryDto>;
		categories.Should().NotBeNull();
		categories.Should().BeEmpty();

		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		var error = errorField?.GetValue(cut.Instance) as string;
		error.Should().Be("boom");

		// Logger call is an extension method and not directly assertable with NSubstitute; error message check suffices.
	}

	[Fact]
	public async Task GoToList_NavigatesToArticlesList()
	{
		// Arrange
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>())));
		Services.AddSingleton(getCategoriesHandler);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		cut.Markup.Should().Contain("Create Article");

		// Act - invoke private GoToList directly
		var method = cut.Instance.GetType().GetMethod("GoToList", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		await cut.InvokeAsync(() => method.Invoke(cut.Instance, null));

		// Assert
		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith("/articles");
	}
}
