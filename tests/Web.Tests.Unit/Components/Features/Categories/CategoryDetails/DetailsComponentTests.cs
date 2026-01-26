// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     DetailsComponentTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

namespace Web.Components.Features.Categories.CategoryDetails;

/// <summary>
/// bUnit tests for the Category Details component
/// </summary>
// Modernized for bUnit v2 and helper-based authentication
[ExcludeFromCodeCoverage]
public class DetailsComponentTests : BunitContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		var tcs = new TaskCompletionSource<Result<CategoryDto>>();
		handler.HandleAsync(Arg.Any<string>()).Returns(tcs.Task);
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));
		// No need to complete tcs, component should show loading
		cut.Markup.Should().Contain("Loading");
	}

	[Fact]
	public async Task ShowsCategoryDetails_WhenCategoryExists()
	{
		// Arrange
		var category = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Category 1",
			Slug = "category-1",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-1),
			ModifiedOn = null,
			IsArchived = false
		};
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Ok(category)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, category.Id.ToString()));

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert
		cut.Markup.Should().Contain(category.CategoryName);
	}

	[Fact]
	public async Task ShowsNotFound_WhenCategoryIsNull()
	{
		// Arrange
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Fail<CategoryDto>("Category not found.")));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439012"));

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert
		cut.Markup.Should().Contain("not found");
	}

	[Fact]
	public async Task ShowsEditButton_WhenUserIsAdmin()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);
		var category = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439013"),
			CategoryName = "Category 3",
			Slug = "category-3",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-2),
			ModifiedOn = null,
			IsArchived = false
		};
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Ok(category)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Create a fake AuthenticationState with an admin user
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var user = new ClaimsPrincipal(identity);
		var authState = new AuthenticationState(user);

		var cut = Render<Details>(
			parameters => parameters
				.Add(p => p.Id, category.Id.ToString())
				.AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(authState))
		);

		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert
		cut.Markup.Should().Contain("Edit");
	}

	[Fact]
	public async Task DoesNotShowEditButton_WhenUserIsNotAdmin()
	{
		// Arrange
		var category = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439014"),
			CategoryName = "Category 4",
			Slug = "category-4",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-3),
			ModifiedOn = null,
			IsArchived = false
		};
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Ok(category)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, category.Id.ToString()));

		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert
		cut.Markup.Should().NotContain("Edit");
	}

	[Fact]
	public async Task RendersDetailsView_WhenCategoryIsLoaded()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "User", "Admin", "Author" ]);
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology-slug",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, categoryDto.Id.ToString()));

		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		cut.Markup.Should().Contain("Category Slug:");
		cut.Markup.Should().Contain("technology-slug");
	}

	[Fact]
	public async Task DisplaysModifiedOn_WhenNotNull()
	{
		// Arrange
		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = new DateTimeOffset(2024, 2, 20, 15, 30, 0, TimeSpan.Zero),
			IsArchived = false
		};
		var handler = Substitute.For<GetCategory.IGetCategoryHandler>();
		handler.HandleAsync(Arg.Any<string>()).Returns(Task.FromResult(Result.Ok(categoryDto)));
		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, categoryDto.Id.ToString()));
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		cut.Markup.Should().Contain("Modified On:");
		cut.Markup.Should().NotContain("Never");
	}

	[Fact]
	public void DisplaysActiveStatus_ForNonArchivedCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Active"));
		cut.Markup.Should().Contain("Status:");
		cut.Markup.Should().Contain("Active");
	}

	[Fact]
	public void DisplaysArchivedStatus_ForArchivedCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Archived Category",
			Slug = "archived-category",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-60),
			ModifiedOn = DateTimeOffset.UtcNow.AddDays(-30),
			IsArchived = true
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Archived"));
		cut.Markup.Should().Contain("Status:");
		cut.Markup.Should().Contain("Archived");
	}

	[Fact]
	public void EditButton_ShouldBe_Enabled_ForActiveCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Create a fake AuthenticationState with an admin user
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var user = new ClaimsPrincipal(identity);
		var authState = new AuthenticationState(user);

		// Act
		var cut = Render<Details>(
			parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
				.AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(authState))
		);

		cut.WaitForState(() => cut.Markup.Contains("Technology"));
		var editButton = cut.Find("button:contains('Edit')");
		editButton.HasAttribute("disabled").Should().BeFalse();
	}

	[Fact]
	public void EditButton_ShouldBe_Disabled_ForArchivedCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Archived Category",
			Slug = "archived-category",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-60),
			ModifiedOn = DateTimeOffset.UtcNow.AddDays(-30),
			IsArchived = true
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Create a fake AuthenticationState with an admin user
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var user = new ClaimsPrincipal(identity);
		var authState = new AuthenticationState(user);

		// Act
		var cut = Render<Details>(
			parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
				.AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(authState))
		);

		cut.WaitForState(() => cut.Markup.Contains("Archived Category"));
		var editButton = cut.Find("button:contains('Edit')");
		editButton.HasAttribute("disabled").Should().BeTrue();
	}

	[Fact]
	public async Task EditButton_Click_ShouldNavigateToEdit()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Create a fake AuthenticationState with an admin user
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var user = new ClaimsPrincipal(identity);
		var authState = new AuthenticationState(user);

		var cut = Render<Details>(
			parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
				.AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(authState))
		);

		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Act
		var editButton = cut.Find("button:contains('Edit')");
		await editButton.ClickAsync();

		// Assert
		nav.Uri.Should().EndWith("/categories/edit/507f1f77bcf86cd799439011");
	}

	[Fact]
	public void BackToListButton_Click_ShouldNavigateToList()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		NavigationManager nav = Services.GetRequiredService<NavigationManager>();

		cut.WaitForState(() => cut.Markup.Contains("Technology"));

		// Act
		var backButton = cut.Find("button:contains('Back to List')");
		backButton.Click();

		// Assert
		nav.Uri.Should().EndWith("/categories");
	}

	[Fact]
	public void PageHeading_ShouldDisplay_CategoryDetails()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Category Details"));
		cut.Markup.Should().Contain("Category Details");
	}

	[Fact]
	public async Task RendersButtons_WithCorrectText()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Create a fake AuthenticationState with an admin user
		var claims = new[] { new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "TestAuthType");
		var user = new ClaimsPrincipal(identity);
		var authState = new AuthenticationState(user);

		// Act
		var cut = Render<Details>(
			parameters => parameters
				.Add(p => p.Id, "507f1f77bcf86cd799439011")
				.AddCascadingValue<Task<AuthenticationState>>(Task.FromResult(authState))
		);

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		cut.Markup.Should().Contain("Edit");
		cut.Markup.Should().Contain("Back to List");
	}

	[Fact]
	public void ComponentWithNoId_ShouldHandle_EmptyParameter()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		handler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Fail<CategoryDto>("Invalid category ID.")));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, ""));

		cut.WaitForState(() => cut.Markup.Contains("Unable to load category"));
		cut.Markup.Should().Contain("Unable to load category");
	}

}
