using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.ArticleDetails;
using Web.Components.Features.Articles.Models;
using Web.Infrastructure;
using Shared.Abstractions;
using Xunit;

namespace Web.Tests.Bunit.Components.Articles;

public class EditArticleConflictTests : TestContext
{
    [Fact]
    public async Task When_EditResultsInConcurrency_Then_ConflictPanelIsShown_And_ForceOverwriteSucceeds()
    {
        // Arrange
        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();

        var initial = new ArticleDto(
            articleId,
            "initial_slug",
            "Initial Title",
            "Intro",
            "Content",
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
        // First call returns initial, subsequent calls return server
        getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

        var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, new[] { "Title", "Content" });

        var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
        // First save attempt fails with concurrency, second attempt (force overwrite) succeeds
        editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(
            Result.Fail<ArticleDto>("Concurrency conflict: article was modified by another process", ResultErrorCode.Concurrency, conflictInfo),
            Result.Ok<ArticleDto>(server)
        );

        var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
        categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>(Enumerable.Empty<CategoryDto>()));

        // Register services in test DI
        Services.AddSingleton(getHandler);
        Services.AddSingleton(editHandler);
        Services.AddSingleton(categoriesHandler);

        // Provide a fake NavigationManager to observe navigation
        var navMan = Services.GetRequiredService<FakeNavigationManager>();

        // Act - render component
        var cut = RenderComponent<Web.Components.Features.Articles.ArticleEdit.Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));

        // Ensure component finished initial load
        await cut.WaitForAssertionAsync(() => cut.Find("form"));

        // Submit the form (click Save)
        var saveButton = cut.Find("button[type=submit]");
        saveButton.Click();

        // Wait for the conflict UI to appear
        await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

        // Assert conflict UI present
        cut.Markup.Should().Contain("Concurrency Conflict");
        var reloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reload Latest"));
        var forceButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Force Overwrite"));
        reloadButton.Should().NotBeNull();
        forceButton.Should().NotBeNull();

        // Act - click Force Overwrite
        forceButton!.Click();

        // Wait for navigation to details page after successful overwrite
        await Task.Delay(50); // small delay for navigation to occur in component

        // Assert navigation occurred to details page
        navMan.Uri.Should().Contain($"/articles/details/{articleId}");

        // Verify Edit handler was called twice (initial failure + retry/force)
        await editHandler.Received(2).HandleAsync(Arg.Any<ArticleDto>());
    }

    [Fact]
    public async Task When_EditResultsInConcurrency_Then_ReloadLatestLoadsServerArticle()
    {
        // Arrange
        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();

        var initial = new ArticleDto(
            articleId,
            "initial_slug",
            "Initial Title",
            "Intro",
            "Content",
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
        getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

        var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, new[] { "Title", "Content" });
        var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
        editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

        var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
        categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>(Enumerable.Empty<CategoryDto>()));

        Services.AddSingleton(getHandler);
        Services.AddSingleton(editHandler);
        Services.AddSingleton(categoriesHandler);

        var cut = RenderComponent<Web.Components.Features.Articles.ArticleEdit.Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));

        await cut.WaitForAssertionAsync(() => cut.Find("form"));

        // Submit the form to trigger conflict
        var saveButton = cut.Find("button[type=submit]");
        saveButton.Click();

        // Wait for conflict UI
        await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

        // Click Reload Latest
        var reloadButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reload Latest"));
        reloadButton.Should().NotBeNull();
        reloadButton!.Click();

        // After reload, the edit model title should reflect server title
        await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Server Title"));
        cut.Markup.Should().Contain("Server Title");

        // Ensure edit handler was called once (initial attempt)
        await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
    }

    [Fact]
    public async Task When_SaveSucceeds_NavigatesToDetails()
    {
        // Arrange
        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();

        var article = new ArticleDto(
            articleId,
            "slug",
            "Title",
            "Intro",
            "Content",
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

        var getHandler = Substitute.For<GetArticle.IGetArticleHandler>();
        getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(article));

        var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
        editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Result.Ok<ArticleDto>(article));

        var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
        categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>(Enumerable.Empty<CategoryDto>()));

        Services.AddSingleton(getHandler);
        Services.AddSingleton(editHandler);
        Services.AddSingleton(categoriesHandler);

        var navMan = Services.GetRequiredService<FakeNavigationManager>();

        // Act
        var cut = RenderComponent<Web.Components.Features.Articles.ArticleEdit.Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));
        await cut.WaitForAssertionAsync(() => cut.Find("form"));

        var saveButton = cut.Find("button[type=submit]");
        saveButton.Click();

        // Wait a short time for navigation
        await Task.Delay(50);

        // Assert
        navMan.Uri.Should().Contain($"/articles/details/{articleId}");
        await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
    }

    [Fact]
    public async Task When_ConflictAndUserCancels_NavigatesToDetails_WithoutRetry()
    {
        // Arrange
        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();

        var initial = new ArticleDto(
            articleId,
            "initial_slug",
            "Initial Title",
            "Intro",
            "Content",
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
        getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

        var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, new[] { "Title", "Content" });
        var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
        editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

        var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
        categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>(Enumerable.Empty<CategoryDto>()));

        Services.AddSingleton(getHandler);
        Services.AddSingleton(editHandler);
        Services.AddSingleton(categoriesHandler);

        var navMan = Services.GetRequiredService<FakeNavigationManager>();

        var cut = RenderComponent<Web.Components.Features.Articles.ArticleEdit.Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));
        await cut.WaitForAssertionAsync(() => cut.Find("form"));

        // Trigger save to produce conflict
        var saveButton = cut.Find("button[type=submit]");
        saveButton.Click();

        await cut.WaitForAssertionAsync(() => cut.Markup.Contains("Concurrency Conflict"));

        // Find and click Cancel in the conflict panel
        var cancelButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Cancel"));
        cancelButton.Should().NotBeNull();
        cancelButton!.Click();

        // Wait briefly for navigation
        await Task.Delay(50);

        // Assert navigation to details and only one edit attempt
        navMan.Uri.Should().Contain($"/articles/details/{articleId}");
        await editHandler.Received(1).HandleAsync(Arg.Any<ArticleDto>());
    }
}
