using Bunit;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Shared.Entities;
using Shared.Models;

using Web.Components.Features.Articles.ArticleEdit;
using Web.Services;

using Xunit;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleEdit;

[ExcludeFromCodeCoverage]
public class EditComponentTests : TestContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		// Arrange handlers used by the component
		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<Web.Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		// Setup JSInterop for the MarkdownEditor component
		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		// Keep loading by returning pending tasks
		var tcsCats = new TaskCompletionSource<Result<IEnumerable<CategoryDto>>>();
		getCategories.HandleAsync(Arg.Any<bool>()).Returns(_ => tcsCats.Task);

		var tcsArticle = new TaskCompletionSource<Result<ArticleDto>>();
		getArticle.HandleAsync(id).Returns(_ => tcsArticle.Task);

		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		// Assert: loading markup present
		cut.Markup.Should().Contain("Loading");
	}

	[Fact]
	public void RendersErrorAlert_WhenArticleLoadFails()
	{
		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<Web.Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		// Setup JSInterop for the MarkdownEditor component
		JSInterop.Mode = JSRuntimeMode.Loose;

		var id = ObjectId.Parse("507f1f77bcf86cd799439011");

		// Categories load ok (empty list is fine)
		getCategories.HandleAsync(Arg.Any<bool>())
			.Returns(Result.Ok<IEnumerable<CategoryDto>>(Enumerable.Empty<CategoryDto>()));

		// Article load fails
		getArticle.HandleAsync(id).Returns(Result.Fail<ArticleDto>("Article not found."));

		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, id.ToString()));

		// Assert: Error alert should be shown when article load fails
		cut.Markup.Should().Contain("Unable to load article");
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersEditForm_WhenEditModelIsPresent()
	{
		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<Web.Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		// Setup JSInterop for the MarkdownEditor component
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
				new AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		// Assert basic form content
		cut.Markup.Should().Contain("Title");
		cut.Markup.Should().Contain("Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

	[Fact]
	public void RendersPageHeading_WhenPageLoads()
	{
		var getCategories = Substitute.For<Web.Components.Features.Categories.CategoriesList.GetCategories.IGetCategoriesHandler>();
		var getArticle = Substitute.For<Web.Components.Features.Articles.ArticleDetails.GetArticle.IGetArticleHandler>();
		var editArticle = Substitute.For<Web.Components.Features.Articles.ArticleEdit.EditArticle.IEditArticleHandler>();
		var fileStorage = Substitute.For<IFileStorage>();

		Services.AddSingleton(getCategories);
		Services.AddSingleton(getArticle);
		Services.AddSingleton(editArticle);
		Services.AddSingleton(fileStorage);

		// Setup JSInterop for the MarkdownEditor component
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
				new AuthorInfo("user1", "Test Author"),
				new Category { Id = catId, CategoryName = "Tech" },
				true,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				false,
				true
		);

		getArticle.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		// Assert PageHeadingComponent is rendered
		cut.Markup.Should().Contain("Edit Article");
	}

}