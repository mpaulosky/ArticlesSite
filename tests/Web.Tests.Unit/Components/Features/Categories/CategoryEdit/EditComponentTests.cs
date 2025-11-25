// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     EditComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;

using Web.Components.Features.Categories.CategoryEdit;

using Edit = Web.Components.Features.Categories.CategoryEdit.Edit;

namespace Web.Tests.Unit.Components.Features.Categories.CategoryEdit;

[ExcludeFromCodeCoverage]
public class EditComponentTests : BunitContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		// Create a delayed task to keep the component in loading state
		var tcs = new TaskCompletionSource<Result<CategoryDto>>();

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(tcs.Task);

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert - Check immediately while still loading
		cut.Markup.Should().Contain("Loading...");

		// Cleanup - complete the task
		tcs.SetResult(Result.Fail<CategoryDto>("Category not found."));
	}

	[Fact]
	public void RendersErrorAlert_WhenCategoryNotFound()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Fail<CategoryDto>("Category not found.")));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Category not found."));
		cut.Markup.Should().Contain("Category not found.");
		cut.Markup.Should().Contain("Unable to load category");
	}

		[Theory]
		[InlineData("Admin")]
		[InlineData("Author")]
		[InlineData("User")]
		[InlineData("Admin", "Author")]
		public void RendersEditForm_WhenCategoryIsLoaded(params string[] roles)
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", roles);

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));
		cut.Markup.Should().Contain("Edit Category");
		cut.Markup.Should().Contain("Technology");
		cut.Markup.Should().Contain("container-card");
	}

	[Fact]
	public void DisplaysCategoryName_InInputField()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Science",
			Slug = "science",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Science"));
		var input = cut.Find("#categoryName");
		input.GetAttribute("value").Should().Be("Science");
	}

	[Fact]
	public void DisplaysIsArchivedCheckbox_WithCorrectState()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Archived Category",
			Slug = "archived-category",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = true
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Archived Category"));
		var checkbox = cut.Find("#isArchived");
		checkbox.HasAttribute("checked").Should().BeTrue();
	}

	[Fact]
	public void RendersSubmitButton_WithCorrectText()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Save Changes"));
		cut.Markup.Should().Contain("Save Changes");
		cut.Markup.Should().Contain("Cancel");
	}

	[Fact]
	public void SubmitButton_ShouldBe_InitiallyEnabled()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Save Changes"));
		var submitButton = cut.Find("button[type='submit']");
		submitButton.HasAttribute("disabled").Should().BeFalse();
	}

	[Fact]
	public void PageHeading_ShouldDisplay_EditCategory()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));
		cut.Markup.Should().Contain("Edit Category");
	}

	[Fact]
	public void FormLabels_ShouldBe_DisplayedCorrectly()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Category Name"));
		cut.Markup.Should().Contain("Category Name");
		cut.Markup.Should().Contain("Archive this category");
	}

	[Fact]
	public void ErrorAlert_ShouldNotDisplay_Initially()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));
		cut.Markup.Should().NotContain("Error updating category");
	}

	[Fact]
	public async Task HandleSubmit_WithValidData_ShouldNavigateToList()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();
		EditCategory.IEditCategoryHandler? editHandler = Substitute.For<EditCategory.IEditCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};
		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		editHandler.HandleAsync(Arg.Any<CategoryDto>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);
		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler), editHandler);

		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var form = cut.Find("form");
		await form.SubmitAsync();

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public async Task HandleSubmit_WithFailure_ShouldDisplayError()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();
		EditCategory.IEditCategoryHandler? editHandler = Substitute.For<EditCategory.IEditCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		editHandler.HandleAsync(Arg.Any<CategoryDto>())
			.Returns(Task.FromResult(Result<CategoryDto>.Fail("Failed to update category.")));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);
		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler), editHandler);

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var form = cut.Find("form");
		await form.SubmitAsync();

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Error updating category"));
		cut.Markup.Should().Contain("Error updating category");
		cut.Markup.Should().Contain("Failed to update category.");
	}

	[Fact]
	public void CancelButton_Click_ShouldNavigateToList()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};
		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var cancelButton = cut.Find("button[type='button']");
		cancelButton.Click();

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public void CategoryNameInput_ShouldBind_ToModel()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};
		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var input = cut.Find("#categoryName");
		input.Change("Updated Technology");

		// Assert
		input.GetAttribute("value").Should().Be("Updated Technology");
	}

	[Fact]
	public void IsArchivedCheckbox_ShouldToggle_OnClick()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};
		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var checkbox = cut.Find("#isArchived");
		checkbox.Change(true);

		// Assert
		checkbox.HasAttribute("checked").Should().BeTrue();
	}

	[Fact]
	public async Task SubmitButton_ShouldBe_Disabled_WhileSubmitting()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();
		EditCategory.IEditCategoryHandler? editHandler = Substitute.For<EditCategory.IEditCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		// Simulate a delayed response to observe the submitting state
		var tcs = new TaskCompletionSource<Result<CategoryDto>>();
		getCategoryHandler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));
		editHandler.HandleAsync(Arg.Any<CategoryDto>())
			.Returns(tcs.Task);
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);
		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler), editHandler);

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Edit Category"));

		var form = cut.Find("form");
		var submitTask = form.SubmitAsync();

		// Check if button is disabled while submitting
		cut.WaitForState(() => cut.Markup.Contains("Saving..."), timeout: TimeSpan.FromSeconds(2));
		var submitButton = cut.Find("button[type='submit']");
		submitButton.HasAttribute("disabled").Should().BeTrue();

		// Complete the submission
		tcs.SetResult(Result.Ok(categoryDto));
		await submitTask;
	}

	[Fact]
	public void ComponentWithNoId_ShouldHandle_EmptyParameter()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? getCategoryHandler = Substitute.For<GetCategory.IGetCategoryHandler>();

		getCategoryHandler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Fail<CategoryDto>("Invalid category ID.")));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), getCategoryHandler);

		Services.AddSingleton(typeof(EditCategory.IEditCategoryHandler),
				Substitute.For<EditCategory.IEditCategoryHandler>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, ""));

		// Assert
		cut.WaitForState(() => cut.Markup.Contains("Unable to load category"));
		cut.Markup.Should().Contain("Unable to load category");
	}

}
