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
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var tcs = new TaskCompletionSource<Result<CategoryDto>>();
		handler.HandleAsync(Arg.Any<string>()).Returns(tcs.Task);

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.Markup.Should().Contain("Loading...");

		tcs.SetResult(Result.Fail<CategoryDto>("Category not found."));
	}

	[Fact]
	public void RendersErrorAlert_WhenCategoryNotFound()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		handler.HandleAsync(Arg.Any<string>())
			.Returns(Task.FromResult(Result.Fail<CategoryDto>("Category not found.")));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Category not found."));
		cut.Markup.Should().Contain("Unable to load category");
		cut.Markup.Should().Contain("Category not found.");
	}

	[Theory]
	[InlineData("Admin")]
	[InlineData("Author")]
	[InlineData("User")]
	[InlineData("Admin", "Author")]
	public void RendersDetailsView_WhenCategoryIsLoaded(params string[] roles)
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", roles);

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
		cut.Markup.Should().Contain("Technology");
		cut.Markup.Should().Contain("container-card");
	}

	[Fact]
	public void DisplaysCategoryName_InHeading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Science",
			Slug = "science",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Science"));
		cut.Markup.Should().Contain("Science");
	}

	[Fact]
	public void DisplaysCategorySlug_Correctly()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology-slug",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("technology-slug"));
		cut.Markup.Should().Contain("Category Slug:");
		cut.Markup.Should().Contain("technology-slug");
	}

	[Fact]
	public void DisplaysCategoryId_Correctly()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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

		cut.WaitForState(() => cut.Markup.Contains("507f1f77bcf86cd799439011"));
		cut.Markup.Should().Contain("Category ID:");
		cut.Markup.Should().Contain("507f1f77bcf86cd799439011");
	}

	[Fact]
	public void DisplaysCreatedOn_InCorrectFormat()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var createdDate = new DateTimeOffset(2024, 1, 15, 12, 30, 0, TimeSpan.Zero);

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = createdDate,
			ModifiedOn = null,
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Created On:"));
		cut.Markup.Should().Contain("Created On:");
	}

	[Fact]
	public void DisplaysModifiedOn_AsNeverWhenNull()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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

		cut.WaitForState(() => cut.Markup.Contains("Never"));
		cut.Markup.Should().Contain("Modified On:");
		cut.Markup.Should().Contain("Never");
	}

	[Fact]
	public void DisplaysModifiedOn_WhenNotNull()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

		GetCategory.IGetCategoryHandler? handler = Substitute.For<GetCategory.IGetCategoryHandler>();

		var categoryDto = new CategoryDto
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			CategoryName = "Technology",
			Slug = "technology",
			CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
			ModifiedOn = new DateTimeOffset(2024, 2, 20, 15, 30, 0, TimeSpan.Zero),
			IsArchived = false
		};

		handler.HandleAsync(Arg.Any<string>())
				.Returns(Task.FromResult(Result.Ok(categoryDto)));

		Services.AddSingleton(typeof(GetCategory.IGetCategoryHandler), handler);

		// Act
		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));

		cut.WaitForState(() => cut.Markup.Contains("Modified On:"));
		cut.Markup.Should().Contain("Modified On:");
		cut.Markup.Should().NotContain("Never");
	}

	[Fact]
	public void DisplaysActiveStatus_ForNonArchivedCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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

		cut.WaitForState(() => cut.Markup.Contains("Technology"));
		var editButton = cut.Find("button:contains('Edit')");
		editButton.HasAttribute("disabled").Should().BeFalse();
	}

	[Fact]
	public void EditButton_ShouldBe_Disabled_ForArchivedCategories()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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

		cut.WaitForState(() => cut.Markup.Contains("Archived Category"));
		var editButton = cut.Find("button:contains('Edit')");
		editButton.HasAttribute("disabled").Should().BeTrue();
	}

	[Fact]
	public void EditButton_Click_ShouldNavigateToEdit()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
		var editButton = cut.Find("button:contains('Edit')");
		editButton.Click();

		// Assert
		nav.Uri.Should().EndWith("/categories/edit/507f1f77bcf86cd799439011");
	}

	[Fact]
	public void BackToListButton_Click_ShouldNavigateToList()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
	public void RendersButtons_WithCorrectText()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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

		cut.WaitForState(() => cut.Markup.Contains("Technology"));
		cut.Markup.Should().Contain("Edit");
		cut.Markup.Should().Contain("Back to List");
	}

	[Fact]
	public void ComponentWithNoId_ShouldHandle_EmptyParameter()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", new[] { "Admin" });

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
