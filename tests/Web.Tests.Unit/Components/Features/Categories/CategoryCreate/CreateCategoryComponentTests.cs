// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     CreateCategoryComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

namespace Web.Tests.Unit.Components.Features.Categories.CategoryCreate;

/// <summary>
///   bUnit tests for the Create category component
/// </summary>
// Modernized for bUnit v2 and helper-based authentication
[ExcludeFromCodeCoverage]
public class CreateCategoryComponentTests : BunitContext
{

	[Fact]
	public void InitialRender_ShouldShowFormAndHeading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		// Act
		var cut = Render<Create>();

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
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		var cut = Render<Create>();
		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		// Act
		cut.Find("button[type=button]").Click();

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public void SubmittingForm_ShouldShowLoading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();
		Services.AddSingleton(handler);

		var cut = Render<Create>();

		// Act
		var isSubmittingField = cut.Instance.GetType().GetField("_isSubmitting", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(isSubmittingField);
		isSubmittingField.SetValue(cut.Instance, true);
		cut.Render();

		// Assert
		cut.Markup.Should().Contain("Loading...");
	}

	[Fact]
	public async Task HandleSubmit_WithSuccess_ShouldNavigateToList()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();

		handler.HandleAsync(Arg.Any<CategoryDto>()).Returns(Task.FromResult(
			Result.Ok(new CategoryDto { Id = ObjectId.GenerateNewId(), CategoryName = "Test" })));

		Services.AddSingleton(handler);

		var cut = Render<Create>();
		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		var categoryField = cut.Instance.GetType().GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(categoryField);
		categoryField.SetValue(cut.Instance, new CategoryDto { CategoryName = "Test" });

		// Act
		var handleSubmitMethod = cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);

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
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });
		CreateCategory.ICreateCategoryHandler handler = Substitute.For<CreateCategory.ICreateCategoryHandler>();

		handler.HandleAsync(Arg.Any<CategoryDto>())
			.Returns(Task.FromResult(Result<CategoryDto>.Fail("Failed to create category.")));

		Services.AddSingleton(handler);

		var cut = Render<Create>();

		var categoryField = cut.Instance.GetType().GetField("_category", BindingFlags.NonPublic | BindingFlags.Instance);

		Assert.NotNull(categoryField);
		categoryField.SetValue(cut.Instance, new CategoryDto { CategoryName = "Test" });

		// Act
		var handleSubmitMethod = cut.Instance.GetType().GetMethod("HandleSubmit", BindingFlags.NonPublic | BindingFlags.Instance);

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
