// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategoryComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;
using Web.Components.Features.Categories.CategoryCreate;

namespace Web.Tests.Unit.Components.Features.Categories.CategoryCreate;

/// <summary>
///   bUnit tests for the Create category component
/// </summary>
[ExcludeFromCodeCoverage]
public class CreateCategoryComponentTests : TestContext
{

	[Fact]
	public void InitialRender_ShouldShowFormAndHeading()
	{
		// Arrange - Register services BEFORE rendering
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		// Act
		IRenderedComponent<Create> cut = RenderComponent<Create>();

		// Assert
		cut.Markup.Should().Contain("Create New Category");
		cut.Find("form").Should().NotBeNull();
		cut.Find("input[id=categoryName]").Should().NotBeNull();
		cut.Find("button[type=submit]").TextContent.Should().Contain("Create Category");
		cut.Find("button[type=button]").TextContent.Should().Contain("Cancel");
	}

	[Fact]
	public void CancelButton_Click_ShouldNavigateToList()
	{
		// Arrange - Register services BEFORE rendering
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		IRenderedComponent<Create> cut = RenderComponent<Create>();
		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		// Act
		cut.Find("button[type=button]").Click();

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public void SubmittingForm_ShouldShowLoading()
	{
		// Arrange - Register services BEFORE rendering
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		IRenderedComponent<Create> cut = RenderComponent<Create>();

		// Act
		FieldInfo? isSubmittingField =
				cut.Instance.GetType().GetField("_isSubmitting", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(isSubmittingField);
		isSubmittingField.SetValue(cut.Instance, true);
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("Loading...");
	}

	[Fact]
	public async Task HandleSubmit_WithSuccess_ShouldNavigateToList()
	{
		// Arrange - Register services BEFORE rendering
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();

		handler.HandleAsync(Arg.Any<CategoryDto>()).Returns(Task.FromResult(
				Result.Ok(new CategoryDto { Id = ObjectId.GenerateNewId(), CategoryName = "Test" })));

		Services.AddSingleton(handler);

		IRenderedComponent<Create> cut = RenderComponent<Create>();
		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		FieldInfo? categoryField =
				cut.Instance.GetType().GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(categoryField);
		categoryField.SetValue(cut.Instance, new CategoryDto { CategoryName = "Test" });

		// Act
		MethodInfo? handleSubmitMethod =
				cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(handleSubmitMethod);
		var task = handleSubmitMethod.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public async Task HandleSubmit_WithFailure_ShouldShowError()
	{
		// Arrange - Register services BEFORE rendering
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();

		handler.HandleAsync(Arg.Any<CategoryDto>())
				.Returns(Task.FromResult(Result<CategoryDto>.Fail("Failed to create category.")));

		Services.AddSingleton(handler);

		IRenderedComponent<Create> cut = RenderComponent<Create>();

		FieldInfo? categoryField =
				cut.Instance.GetType().GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(categoryField);
		categoryField.SetValue(cut.Instance, new CategoryDto { CategoryName = "Test" });

		// Act
		MethodInfo? handleSubmitMethod =
				cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(handleSubmitMethod);
		var task = handleSubmitMethod.Invoke(cut.Instance, null) as Task;
		Assert.NotNull(task);
		await task;
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("Error creating category");
		cut.Markup.Should().Contain("Failed to create category.");
	}

}