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

public class CreateComponentTests : BunitContext
{
	public CreateComponentTests()
	{
		// Setup JSInterop for markdown editor
		JSInterop.SetupVoid("initialize", _ => true).SetVoidResult();

		// Add Authorization
		Web.Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TestUser", ["Author"]);

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

		// Assert
		await cut.WaitForAssertionAsync(() => cut.Markup.Should().Contain("Failed to load categories."));
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

		// Assert
		await cut.WaitForAssertionAsync(() =>
		{
			cut.Markup.Should().Contain("Create Article");
			cut.Markup.Should().NotContain("Loading...");
		});
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
		await cut.WaitForAssertionAsync(() => cut.Markup.Should().Contain("Create Article"));

		// Fill the form
		cut.Find("#titleEdit").Change("Test Title");
		cut.Find("#introductionEdit").Change("Test Intro");
		cut.Find("select").Change(categoryId.ToString());

		// Act
		await cut.Find("form").SubmitAsync();

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
		await cut.WaitForAssertionAsync(() => cut.Markup.Should().Contain("Create Article"));

		// Act
		await cut.Find("form").SubmitAsync();

		// Assert
		await cut.WaitForAssertionAsync(() => cut.Markup.Should().Contain("Database error"));
	}
}
