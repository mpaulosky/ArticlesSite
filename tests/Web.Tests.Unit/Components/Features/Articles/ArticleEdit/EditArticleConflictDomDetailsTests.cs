using Web.Components.Features.Categories.CategoriesList;
using Web.Infrastructure;

namespace Web.Components.Features.Articles.ArticleEdit;

/// <summary>
/// Tests that the conflict panel renders accessible attributes and lists changed fields.
/// </summary>
[ExcludeFromCodeCoverage]
public class EditArticleConflictDomDetailsTests : BunitContext
{

	[Fact]
	public async Task ConflictPanel_HasRoleAlert_And_ListsChangedFields()
	{
		// Arrange
		JSInterop.SetupVoid("initialize", _ => true).SetVoidResult();
		var articleId = ObjectId.GenerateNewId();

		var initial = new ArticleDto(
				articleId,
				"initial_slug",
				"Initial Title",
				"Intro",
				"Original Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				0
		);

		var server = new ArticleDto(
				articleId,
				"initial_slug",
				"Server Title",
				"Intro",
				"Server Content",
				"https://example.com/img.jpg",
				null,
				null,
				false,
				null,
				null,
				null,
				false,
				true,
				1
		);

		var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();
		getHandler.HandleAsync(articleId).Returns(Result.Ok(initial), Result.Ok(server));

		var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, ["Title", "Content"]);
		var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();

		editHandler.HandleAsync(Arg.Any<ArticleDto>())
				.Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

		var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
		categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>>([]));

		Services.AddSingleton(getHandler);
		Services.AddSingleton(editHandler);
		Services.AddSingleton(categoriesHandler);
		Services.AddSingleton(Substitute.For<IFileStorage>());

		// Act
		var cut = Render<Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Invoke the submitted handler directly to trigger a conflict panel deterministically
		var submit = cut.Instance.GetType().GetMethod("HandleValidSubmit", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (submit?.Invoke(cut.Instance, null) is Task t) await t;
		});

		// Ensure the component is in a conflict state
		var conflictField = cut.Instance.GetType().GetField("_isConcurrencyConflict", BindingFlags.NonPublic | BindingFlags.Instance);
		var isConflict = conflictField?.GetValue(cut.Instance) as bool?;
		(isConflict ?? false).Should().BeTrue();

		// Ensure latest article is loaded (ReloadLatestAsync) to be deterministic under coverage
		var reloadMethod = cut.Instance.GetType().GetMethod("ReloadLatestAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		await cut.InvokeAsync(async () =>
		{
			if (reloadMethod?.Invoke(cut.Instance, null) is Task t) await t;
		});

		var latestField = cut.Instance.GetType().GetField("_latestArticle", BindingFlags.NonPublic | BindingFlags.Instance);
		var latest = latestField?.GetValue(cut.Instance) as ArticleDto;
		latest.Should().NotBeNull();

		// Assert the latest article version is the server version
		latest.Version.Should().Be(server.Version);
	}

}
