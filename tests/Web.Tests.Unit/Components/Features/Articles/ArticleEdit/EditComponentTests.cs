using Microsoft.AspNetCore.Components;

namespace Web.Components.Features.Articles.ArticleEdit;

// Modernized for bUnit v2 and helper-based authentication
[ExcludeFromCodeCoverage]
public class EditComponentTests : BunitContext
{
	public EditComponentTests()
	{
		JSInterop.Mode = JSRuntimeMode.Loose;
	}

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		var tcsCats = new TaskCompletionSource<Result<IEnumerable<CategoryDto>>>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(_ => tcsCats.Task);

		var tcsArticle = new TaskCompletionSource<Result<ArticleDto>>();
		getArticle.HandleAsync(id).Returns(_ => tcsArticle.Task);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		cut.Markup.Should().Contain("Loading");
	}

	[Fact]
	public void RendersErrorAlert_WhenArticleLoadFails()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok(Enumerable.Empty<CategoryDto>()));

		getArticle.HandleAsync(id).Returns(Result.Fail<ArticleDto>("Article not found."));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		cut.Markup.Should().Contain("Unable to load article");
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersEditForm_WhenEditModelIsPresent()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var catId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto>
		{
			new() { Id = catId, CategoryName = "Tech", IsArchived = false }
		};
		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok<IEnumerable<CategoryDto>>(categories));

		var article = new ArticleDto(
				ObjectId.Parse("507f1f77bcf86cd799439011"),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"Test Content",
				"https://example.com/image.jpg",
				new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		cut.Markup.Should().Contain("Title");
		cut.Markup.Should().Contain("Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

	[Fact]
	public void RendersPageHeading_WhenPageLoads()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		JSInterop.Mode = JSRuntimeMode.Loose;

		var catId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto>
		{
			new() { Id = catId, CategoryName = "Tech", IsArchived = false }
		};
		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok<IEnumerable<CategoryDto>>(categories));

		var article = new ArticleDto(
				ObjectId.Parse("507f1f77bcf86cd799439011"),
				"test-slug",
				"Test Title",
				"Test Introduction",
				"Test Content",
				"https://example.com/image.jpg",
				new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		cut.Markup.Should().Contain("Edit Article");
	}

	[Fact]
	public async Task HandleValidSubmit_WithSuccess_NavigatesToDetails()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var article = new ArticleDto(ObjectId.GenerateNewId(), "slug", "Title", "Intro", "Content", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		editArticle.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Ok(article)));
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));
		// Ensure initialization completed deterministically
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		// Invoke submit handler directly for determinism
		var handle = cut.Instance.GetType().GetMethod("HandleValidSubmit", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (handle?.Invoke(cut.Instance, null) is Task t) await t;
		});

		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/details/{article.Id}");
	}

	[Fact]
	public async Task HandleValidSubmit_WithFailure_ShowsError()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var article = new ArticleDto(ObjectId.GenerateNewId(), "slug", "Title", "Intro", "Content", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		editArticle.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Fail<ArticleDto>("Database error")));
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		var handle = cut.Instance.GetType().GetMethod("HandleValidSubmit", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (handle?.Invoke(cut.Instance, null) is Task t) await t;
		});

		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		var errorValue = errorField?.GetValue(cut.Instance) as string;
		errorValue.Should().Contain("Database error");
	}

	[Fact]
	public async Task HandleValidSubmit_WithConcurrency_ShowsConflictUI()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title1", "Intro1", "Content1", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var latest = new ArticleDto(id, "slug", "Title2", "Intro2", "Content2", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false) { Version = 2 };

		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(id).Returns(Result.Ok(article), Result.Ok(latest));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		editArticle.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Fail<ArticleDto>("Concurrency conflict")));
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));
		// Ensure initialization completed deterministically
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		var handle = cut.Instance.GetType().GetMethod("HandleValidSubmit", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (handle?.Invoke(cut.Instance, null) is Task t) await t;
		});

		// Inspect private fields instead of relying on markup to avoid timing fragility
		var isConflictField = cut.Instance.GetType().GetField("_isConcurrencyConflict", BindingFlags.NonPublic | BindingFlags.Instance);
		var latestField = cut.Instance.GetType().GetField("_latestArticle", BindingFlags.NonPublic | BindingFlags.Instance);
		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);

		(isConflictField?.GetValue(cut.Instance) as bool?).Should().BeTrue();
		(latestField?.GetValue(cut.Instance) as ArticleDto).Should().NotBeNull();
		(errorField?.GetValue(cut.Instance) as string).Should().Contain("concurrency");
	}

	[Fact]
	public async Task OnCategoryChanged_SetsCategoryOrClears()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var catId = ObjectId.GenerateNewId();
		var categories = new List<CategoryDto> { new() { Id = catId, CategoryName = "C1", IsArchived = false } };
		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(categories));
		Services.AddSingleton(getCategories);

		var article = new ArticleDto(ObjectId.GenerateNewId(), "slug", "Title", "Intro", "Content", "", null, null, false, null, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));
		// Ensure initialization completed deterministically
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		// Ensure category option is present before changing
		var catsField = cut.Instance.GetType().GetField("_categories", BindingFlags.NonPublic | BindingFlags.Instance);
		var cats = catsField?.GetValue(cut.Instance) as List<CategoryDto>;
		cats.Should().NotBeNull();
		cats.Any(c => c.Id == catId).Should().BeTrue();

		// Instead of simulating DOM change, call the private handler directly for determinism
		var method = cut.Instance.GetType().GetMethod("OnCategoryChanged", BindingFlags.NonPublic | BindingFlags.Instance);
		method?.Invoke(cut.Instance, [ new ChangeEventArgs { Value = catId.ToString() } ]);

		var model = modelField?.GetValue(cut.Instance) as ArticleDto;
		model.Should().NotBeNull();
		model.Category.Should().NotBeNull();
		model.Category!.Id.Should().Be(catId);

		// Clear selection
		method?.Invoke(cut.Instance, [ new ChangeEventArgs { Value = string.Empty } ]);
		var model2 = modelField?.GetValue(cut.Instance) as ArticleDto;
		model2.Should().NotBeNull();
		model2.Category.Should().BeNull();

		await cut.Find("select").ChangeAsync(string.Empty);
		var model3 = modelField?.GetValue(cut.Instance) as ArticleDto;
		model3.Should().NotBeNull();
		model3.Category.Should().BeNull();
	}

	[Fact]
	public async Task CancelToDetails_NavigatesBack()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", ["Admin"]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var article = new ArticleDto(ObjectId.GenerateNewId(), "slug", "Title", "Intro", "Content", "", null, null, false, null, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);
		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		await cut.FindAll(".btn-secondary").Last().ClickAsync();
		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/details/{article.Id}");
	}

	[Fact]
	public async Task UserCanEdit_WhenAuthorMatches_ReturnsTrue()
	{
		// Arrange - do not register admin role; instead provide cascading AuthenticationState with sub claim matching author
		var catHandler = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		catHandler.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(catHandler);

		var authorId = "author-123";
		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", new Web.Components.Features.AuthorInfo.Entities.AuthorInfo(authorId, "Auth"), null, false, null, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		// Provide an authentication state with 'sub' claim equal to authorId
		var claims = new[] { new Claim("sub", authorId), new Claim(ClaimTypes.Name, "x") };
		var identity = new ClaimsIdentity(claims, "FakeAuth");
		var principal = new ClaimsPrincipal(identity);

		var cut = Render(builder =>
		{
			builder.OpenComponent<CascadingValue<Task<AuthenticationState>>>(0);
			builder.AddAttribute(1, "Value", Task.FromResult(new AuthenticationState(principal)));
			builder.AddAttribute(2, "ChildContent", (RenderFragment)(b =>
			{
				b.OpenComponent<Edit>(3);
				b.AddAttribute(4, "Id", id.ToString());
				b.CloseComponent();
			}));
			builder.CloseComponent();
		});

		// Assert
		// Wait for UI to render the article then inspect private model
		if ((cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}
		var editComp = cut.FindComponent<Edit>();
		var modelField = editComp.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		var model = modelField?.GetValue(editComp.Instance) as ArticleDto;
		model.Should().NotBeNull();
		model.CanEdit.Should().BeTrue();
	}

	[Fact]
	public async Task UserCanEdit_WhenNotAuthenticated_ReturnsFalse()
	{
		// Arrange - no authentication provider registered
		var catHandler = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		catHandler.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(catHandler);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("authorX", "Auth"), null, false, null, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}
		var model = modelField?.GetValue(cut.Instance) as ArticleDto;
		model.Should().NotBeNull();
		model.CanEdit.Should().BeFalse();
	}

	[Fact]
	public async Task ForceOverwrite_WithSuccess_NavigatesToDetails()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title1", "Intro1", "Content1", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var latest = new ArticleDto(id, "slug", "Title2", "Intro2", "Content2", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false) { Version = 2 };

		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(id).Returns(Result.Ok(article), Result.Ok(latest));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		editArticle.HandleAsync(Arg.Any<ArticleDto>()).Returns(Task.FromResult(Result.Fail<ArticleDto>("Concurrency conflict")), Task.FromResult(Result.Ok(article)));
		Services.AddSingleton(editArticle);
		Services.AddSingleton(Substitute.For<IFileStorage>());

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));
		var modelField = cut.Instance.GetType().GetField("_editModel", BindingFlags.NonPublic | BindingFlags.Instance);
		if ((modelField?.GetValue(cut.Instance) as ArticleDto)?.Id == ObjectId.Empty)
		{
			var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
			if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		}

		// Act - invoke submit handler directly to trigger concurrency handling deterministically
		var method = cut.Instance.GetType().GetMethod("HandleValidSubmit", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.NotNull(method);
		if (method.Invoke(cut.Instance, null) is Task task) await task;
		var isConflictField = cut.Instance.GetType().GetField("_isConcurrencyConflict", BindingFlags.NonPublic | BindingFlags.Instance);
		(isConflictField?.GetValue(cut.Instance) as bool?).Should().BeTrue();

		// Call ForceOverwriteAsync directly to avoid click timing fragility
		var forceMethod = cut.Instance.GetType().GetMethod("ForceOverwriteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (forceMethod?.Invoke(cut.Instance, null) is Task t) await t;
		});

		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/details/{id}");
	}

	[Fact]
	public async Task ForceOverwrite_NoLatest_ShowsError()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var editArticle = Substitute.For<EditArticle.IEditArticleHandler>();
		Services.AddSingleton(editArticle);
		Services.AddSingleton(Substitute.For<IFileStorage>());

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Directly invoke ForceOverwriteAsync when no _latestArticle
		var method = cut.Instance.GetType().GetMethod("ForceOverwriteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
		await cut.InvokeAsync(async () =>
		{
			if (method?.Invoke(cut.Instance, null) is Task t) await t;
		});

		// Inspect private field for error message
		var errorField = cut.Instance.GetType().GetField("_errorMessage", BindingFlags.NonPublic | BindingFlags.Instance);
		var err = errorField?.GetValue(cut.Instance) as string;
		err.Should().NotBeNull();
		err.Should().Be("No latest article available to base overwrite on.");
	}

	[Fact]
	public async Task LoadCategories_WhenFails_ShowsError()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Task.FromResult(Result.Fail<IEnumerable<CategoryDto>>("no cats")));
		Services.AddSingleton(getCategories);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(id).Returns(Result.Ok(article));
		Services.AddSingleton(getArticle);

		var mockLogger = Substitute.For<ILogger<Edit>>();
		Services.AddSingleton(mockLogger);

		Services.AddSingleton(Substitute.For<EditArticle.IEditArticleHandler>());
		Services.AddSingleton(Substitute.For<IFileStorage>());

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Should log a warning about categories failure
		var logCalls = mockLogger.ReceivedCalls();
		logCalls.Any(c => c.GetMethodInfo().Name == "Log" && c.GetArguments().Any(a => a?.ToString()?.Contains("Failed to load categories") == true)).Should().BeTrue();
	}

	[Fact]
	public async Task OnInitialized_WhenGetArticleThrows_ShowsError()
	{
		Helpers.TestAuthHelper.RegisterTestAuthentication(Services, "TEST USER", [ "Admin" ]);

		var getCategories = Substitute.For<Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(Result.Ok<IEnumerable<CategoryDto>>(new List<CategoryDto>()));
		Services.AddSingleton(getCategories);

		var getArticle = Substitute.For<GetArticle.IGetArticleHandler>();
		getArticle.HandleAsync(Arg.Any<ObjectId>()).Returns<Task<Result<ArticleDto>>>(_ => throw new Exception("boom"));
		Services.AddSingleton(getArticle);

		Services.AddSingleton(Substitute.For<EditArticle.IEditArticleHandler>());
		Services.AddSingleton(Substitute.For<IFileStorage>());

		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, ObjectId.GenerateNewId().ToString()));
		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		cut.Markup.Should().Contain("boom");
	}
}
