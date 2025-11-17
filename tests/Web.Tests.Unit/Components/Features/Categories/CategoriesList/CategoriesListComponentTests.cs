using Bunit.TestDoubles;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Web.Components.Features.Categories.CategoriesList;

using CategoriesListComponent = Web.Components.Features.Categories.CategoriesList.CategoriesList;


namespace Web.Tests.Unit.Components.Features.Categories.CategoriesList
{
	[ExcludeFromCodeCoverage]
	public class CategoriesListComponentTests : TestContext
	{
		private void SetupQuickGridJSInterop()
		{
			JSInterop.Mode = JSRuntimeMode.Loose;
		}

		[Fact]
		public void Should_FilterCategories_ByArchivedStatus_OnToggle()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			var handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var activeCategory = new CategoryDto
			{
				Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
				CategoryName = "Active Category",
				Slug = "active-category",
				CreatedOn = DateTimeOffset.UtcNow.AddDays(-10),
				ModifiedOn = null,
				IsArchived = false
			};

			var archivedCategory = new CategoryDto
			{
				Id = ObjectId.Parse("507f1f77bcf86cd799439012"),
				CategoryName = "Archived Category",
				Slug = "archived-category",
				CreatedOn = DateTimeOffset.UtcNow.AddDays(-20),
				ModifiedOn = DateTimeOffset.UtcNow.AddDays(-5),
				IsArchived = true
			};

			var allCategories = new List<CategoryDto> { activeCategory, archivedCategory };
			var activeOnly = new List<CategoryDto> { activeCategory };

			handler.HandleAsync(false).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(activeOnly)));
			handler.HandleAsync(true).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(allCategories)));
			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			var cut = RenderComponent<CategoriesListComponent>();
			cut.WaitForState(() => cut.Markup.Contains("Active Category"));
			cut.Markup.Should().Contain("Active Category");

			// Assert: Only active category shown by default
			var editButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Trim() == "Edit");
			editButton.Should().NotBeNull();
			// For active category, Edit should NOT be disabled
			editButton.HasAttribute("disabled").Should().BeFalse();

			// Toggle Include Archived
			var checkbox2 = cut.Find("input[type=checkbox]");
			checkbox2.Change(true);
			cut.WaitForState(() => cut.Markup.Contains("Archived Category"));
			cut.Markup.Should().Contain("Archived Category");
			cut.Markup.Should().Contain("Active Category");

			// Toggle back to hide archived
			checkbox2.Change(false);
			cut.WaitForState(() => !cut.Markup.Contains("Archived Category"));
			cut.Markup.Should().NotContain("Archived Category");
			cut.Markup.Should().Contain("Active Category");
		}

		[Fact]
		public void Should_DisplayEmptyMessage_WhenAllCategoriesArchived_AndNotIncluded()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			var handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
			{
				new()
				{
					Id = ObjectId.Parse("507f1f77bcf86cd799439013"),
					CategoryName = "Archived Only",
					Slug = "archived-only",
					CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
					ModifiedOn = null,
					IsArchived = true
				}
			};

			handler.HandleAsync(false).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(Enumerable.Empty<CategoryDto>())));
			handler.HandleAsync(true).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			var cut = RenderComponent<CategoriesListComponent>();

			// Assert: Should show empty message since archived are not included by default
			cut.WaitForState(() => cut.Markup.Contains("No categories available yet."));
			cut.Markup.Should().Contain("No categories available yet.");

			// Toggle Include Archived
			var checkbox2 = cut.Find("input[type=checkbox]");
			checkbox2.Change(true);
			cut.WaitForState(() => cut.Markup.Contains("Archived Only"), TimeSpan.FromSeconds(15));
			cut.Markup.Should().Contain("Archived Only");
		}
		// =======================================================
		// Copyright (c) 2025. All rights reserved.
		// File Name :     CategoriesListComponentTests.cs
		// Company :       mpaulosky
		// Author :        Matthew Paulosky
		// Solution Name : ArticlesSite
		// Project Name :  Web.Tests.Unit
		// =======================================================

		[Fact]
		public void RendersLoadingComponent_WhenIsLoading()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			var handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			// Create a delayed task to keep the component in loading state
			var tcs = new TaskCompletionSource<Result<IEnumerable<CategoryDto>>>();
			handler.HandleAsync(Arg.Any<bool>()).Returns(tcs.Task);
			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert - Check immediately while still loading
			cut.Markup.Should().Contain("Loading...");

			// Cleanup - complete the task
			tcs.SetResult(Result.Ok(Enumerable.Empty<CategoryDto>()));
		}

		[Fact]
		public void RendersErrorAlert_WhenLoadingFails()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync(Arg.Any<bool>())
					.Returns(Task.FromResult(Result<IEnumerable<CategoryDto>>.Fail("Failed to load categories.")));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("Failed to load categories."));
			cut.Markup.Should().Contain("Error loading articles");
			cut.Markup.Should().Contain("Failed to load categories.");
		}

		[Fact]
		public void RendersEmptyMessage_WhenNoCategoriesExist()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync(Arg.Any<bool>())
					.Returns(Task.FromResult(Result.Ok(Enumerable.Empty<CategoryDto>())));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("No categories available yet."));
			cut.Markup.Should().Contain("No categories available yet.");
			cut.Markup.Should().Contain("Check back soon for new content!");
		}

		[Fact]
		public void RendersCategoryList_WhenCategoriesExist()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "Technology",
						Slug = "technology",
						CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
						ModifiedOn = null,
						IsArchived = false
				},
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439012"),
						CategoryName = "Science",
						Slug = "science",
						CreatedOn = DateTimeOffset.UtcNow.AddDays(-20),
						ModifiedOn = DateTimeOffset.UtcNow.AddDays(-10),
						IsArchived = false
				}
		};

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
			handler.HandleAsync(Arg.Any<bool>()).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("Technology"));
			cut.Markup.Should().Contain("All Categories");
			cut.Markup.Should().Contain("Technology");
			cut.Markup.Should().Contain("Science");
			cut.Markup.Should().Contain("technology");
			cut.Markup.Should().Contain("science");
		}

		[Fact]
		public void DisplaysCategoryDetails_WithCorrectData()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "Technology",
						Slug = "technology",
						CreatedOn = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero),
						ModifiedOn = new DateTimeOffset(2024, 2, 20, 15, 30, 0, TimeSpan.Zero),
						IsArchived = false
				}
		};

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("Technology"));
		cut.Markup.Should().Contain("Category Name");
		cut.Markup.Should().Contain("Technology");
		cut.Markup.Should().Contain("Slug");
		cut.Markup.Should().Contain("technology");
		cut.Markup.Should().Contain("Created On");
		cut.Markup.Should().Contain("Modified On");
		cut.Markup.Should().Contain("Status");
			cut.Markup.Should().Contain("Active");
		}

		[Fact]
		public void DisplaysArchivedStatus_ForArchivedCategories()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "Technology",
						Slug = "technology",
						CreatedOn = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero),
						ModifiedOn = new DateTimeOffset(2024, 2, 20, 15, 30, 0, TimeSpan.Zero),
						IsArchived = true
				}
		};

			handler.HandleAsync(Arg.Any<bool>())
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			var cut = RenderComponent<CategoriesListComponent>();

			// Assert
			var checkbox = cut.Find("input[type=checkbox]");
			checkbox.Change(true);
			cut.WaitForState(() => cut.Markup.Contains("Archived"));
			cut.Markup.Should().Contain("badge bg-secondary");
			cut.Markup.Should().Contain("Archived");
		}

		[Fact]
		public void ViewButton_Click_ShouldNavigateToDetails()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "Technology",
						Slug = "technology",
						CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
						ModifiedOn = null,
						IsArchived = false
				}
		};

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
			handler.HandleAsync(Arg.Any<bool>()).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();
			NavigationManager nav = Services.GetRequiredService<NavigationManager>();

			cut.WaitForState(() => cut.Markup.Contains("Technology"));    // Act
			var viewButton = cut.Find("button:contains('View')");
			viewButton.Click();

			// Assert
			nav.Uri.Should().EndWith("/categories/details/507f1f77bcf86cd799439011");
		}

		[Fact]
		public void EditButton_Click_ShouldNavigateToEdit()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "Technology",
						Slug = "technology",
						CreatedOn = DateTimeOffset.UtcNow.AddDays(-30),
						ModifiedOn = null,
						IsArchived = false
				}
		};

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));
			handler.HandleAsync(Arg.Any<bool>()).Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();
			NavigationManager nav = Services.GetRequiredService<NavigationManager>();

			cut.WaitForState(() => cut.Markup.Contains("Technology"));

			// Act
			var editButton = cut.Find("button:contains('Edit')");
			editButton.Click();

			// Assert
			nav.Uri.Should().EndWith("/categories/edit/507f1f77bcf86cd799439011");
		}

		//[Fact]
		//public void EditButton_ShouldBe_Disabled_ForArchivedCategories()
		//{
		//	// Arrange
		//	var authContext = this.AddTestAuthorization();
		//	authContext.SetAuthorized("TEST USER");
		//	authContext.SetRoles("Admin");

		//	GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

		//	var categories = new List<CategoryDto>
		//{
		//		new()
		//		{
		//				Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
		//				CategoryName = "Archived Category",
		//				Slug = "archived-category",
		//				CreatedOn = DateTimeOffset.UtcNow.AddDays(-60),
		//				ModifiedOn = DateTimeOffset.UtcNow.AddDays(-30),
		//				IsArchived = true
		//		}
		//};

		//	handler.HandleAsync()
		//			.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

		//	Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);

		//	// Act
		//	var cut = RenderComponent<CategoriesListComponent>();

		//	// Assert
		//	var checkbox = cut.Find("input[type=checkbox]");
		//	checkbox.Change(true);
		//	cut.WaitForState(() => cut.Markup.Contains("Archived Category"), TimeSpan.FromSeconds(15));
		//	var editButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Trim() == "Edit");
		//	editButton.Should().NotBeNull();
		//	editButton.HasAttribute("disabled").Should().BeTrue();
		//}

		[Fact]
		public void CreateButton_ShouldDisplay_ForAdminUsers()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok(Enumerable.Empty<CategoryDto>())));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("All Categories"));

			cut.Markup.Should().Contain("Create")
					.And.Contain("New Category");
		}

		[Fact]
		public void CreateButton_ShouldNotDisplay_ForNonAdminUsers()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");

			// Not setting Admin role

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok(Enumerable.Empty<CategoryDto>())));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("All Categories"));

			cut.Markup.Should().NotContain("Create")
					.And.NotContain("New Category");
		}

		[Fact]
		public void CreateButton_Click_ShouldNavigateToCreate()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok(Enumerable.Empty<CategoryDto>())));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();
			NavigationManager nav = Services.GetRequiredService<NavigationManager>();

			cut.WaitForState(() => cut.Markup.Contains("All Categories"));

			// Act
			var createButton = cut.Find("button:contains('Create')");
			createButton.Click();   // Assert
			nav.Uri.Should().EndWith("/categories/create");
		}

		[Fact]
		public void PageHeading_ShouldDisplay_AllCategories()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok(Enumerable.Empty<CategoryDto>())));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("All Categories"));
			cut.Markup.Should().Contain("All Categories");
		}

		[Fact]
		public void DisplaysModifiedOn_AsNAWhenNull()
		{
			// Arrange
			var authContext = this.AddTestAuthorization();
			authContext.SetAuthorized("TEST USER");
			authContext.SetRoles("Admin");

			GetCategories.IGetCategoriesHandler? handler = Substitute.For<GetCategories.IGetCategoriesHandler>();

			var categories = new List<CategoryDto>
		{
				new()
				{
						Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
						CategoryName = "New Category",
						Slug = "new-category",
						CreatedOn = DateTimeOffset.UtcNow.AddDays(-5),
						ModifiedOn = null,
						IsArchived = false
				}
		};

			handler.HandleAsync()
					.Returns(Task.FromResult(Result.Ok<IEnumerable<CategoryDto>>(categories)));

			Services.AddSingleton(typeof(GetCategories.IGetCategoriesHandler), handler);
			SetupQuickGridJSInterop();

			// Act
			IRenderedComponent<CategoriesListComponent> cut = RenderComponent<CategoriesListComponent>();

			// Assert
			cut.WaitForState(() => cut.Markup.Contains("New Category"));
		cut.Markup.Should().Contain("Modified On");
			cut.Markup.Should().Contain("N/A");
		}

	}
}
