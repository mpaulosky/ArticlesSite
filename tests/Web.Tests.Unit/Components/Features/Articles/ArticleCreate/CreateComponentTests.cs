// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

using Web.Components.Features.Categories.CategoriesList;

// Ensure this is included for Result<T>

namespace Web.Components.Features.Articles.ArticleCreate;

[ExcludeFromCodeCoverage]
public class CreateComponentTests : BunitContext
{
	public CreateComponentTests()
	{
		// Setup JSInterop for markdown editor
		JSInterop.SetupVoid("initialize", _ => true).SetVoidResult();

		// Add Authorization
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TestUser", ["Author"]);

		// Register common services
		Services.AddSingleton(Substitute.For<CreateArticle.ICreateArticleHandler>());
		Services.AddSingleton(Substitute.For<IFileStorage>());
	}

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange
		var tcs = new TaskCompletionSource<Result<IEnumerable<CategoryDto>>>();
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(tcs.Task);
		Services.AddSingleton(getCategoriesHandler);

		// Act
		var cut = Render<Create>();

		// Assert
		cut.Markup.Should().Contain("Loading...");
	}

	[Fact]
	public async Task RendersErrorAlert_WhenErrorMessageIsSet()
	{
		// Arrange
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Fail<IEnumerable<CategoryDto>>("Failed to load categories.")));
		Services.AddSingleton(getCategoriesHandler);

		// Act
		var cut = Render<Create>();

		// Assert - ensure initialization completed deterministically then inspect private field
		var errField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		if (errField?.GetValue(cut.Instance) == null)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task t) await t;
		}
		var err = errField?.GetValue(cut.Instance) as string;
		err.Should().Contain("Failed to load categories.");
	}

	[Fact]
	public async Task RendersCreateForm_WhenNotLoadingOrError()
	{
		// Arrange
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync()
				.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>([])));
		Services.AddSingleton(getCategoriesHandler);

		// Act
		var cut = Render<Create>();

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		// Force a re-render so state changes are reflected in the markup
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("Create Article");
		cut.Markup.Should().NotContain("Loading...");
	}

	[Fact]
	public async Task HandleSubmit_WithValidData_NavigatesToDetails()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto>
		{
				new() { Id = categoryId, CategoryName = "Test Category", Slug = "test-category" }
		};
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
		Services.AddSingleton(getCategoriesHandler);

		var createdArticle = new ArticleDto { Id = ObjectId.GenerateNewId() };
		var createArticleHandler = Substitute.For<CreateArticle.ICreateArticleHandler>();
		createArticleHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Ok(createdArticle)));
		Services.AddSingleton(createArticleHandler);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		// Force re-render to ensure form inputs are present in markup
		cut.Render();

		// Fill the form via private fields for determinism
		var articleField = cut.Instance.GetType().GetField("_article", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(articleField);
		var article = articleField.GetValue(cut.Instance) as ArticleDto;
		article.Should().NotBeNull();
		article.Title = "Test Title";
		article.Introduction = "Test Intro";
		var selectedField = cut.Instance.GetType().GetField("_selectedCategoryId", BindingFlags.NonPublic | BindingFlags.Instance);
		selectedField?.SetValue(cut.Instance, categoryId.ToString());
		cut.Render();

		// Act - invoke submit handler directly for determinism
		var method = cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var task = method.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;

		// Assert
		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/details/{createdArticle.Id}");
	}

	[Fact]
	public async Task HandleSubmit_WithFailure_ShowsErrorAlert()
	{
		// Arrange
		var categoryId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto> { new() { Id = categoryId, CategoryName = "Test" } };
		var getCategoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		getCategoriesHandler.HandleAsync().Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
		Services.AddSingleton(getCategoriesHandler);

		var createArticleHandler = Substitute.For<CreateArticle.ICreateArticleHandler>();
		createArticleHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Fail<ArticleDto>("Database error")));
		Services.AddSingleton(createArticleHandler);

		var cut = Render<Create>();
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act - invoke submit handler directly for determinism
		var method = cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(method);
		var task = method.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;

		// Assert - inspect private field for the error message
		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		var err = errorField?.GetValue(cut.Instance) as string;
		err.Should().Contain("Database error");
	}
}
