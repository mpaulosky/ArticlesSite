using System.Linq;
using System.Threading.Tasks;
using Bunit;
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

/// <summary>
/// Tests that the conflict panel renders accessible attributes and lists changed fields.
/// </summary>
public class EditArticleConflictDomDetailsTests : TestContext
{
    [Fact]
    public async Task ConflictPanel_HasRoleAlert_And_ListsChangedFields()
    {
        // Arrange
        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();

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
        getHandler.HandleAsync(articleId).Returns(Result.Ok<ArticleDto?>(initial), Result.Ok<ArticleDto?>(server));

        var conflictInfo = new ConcurrencyConflictInfo(server.Version, server, new[] { "Title", "Content" });
        var editHandler = Substitute.For<EditArticle.IEditArticleHandler>();
        editHandler.HandleAsync(Arg.Any<ArticleDto>()).Returns(Result.Fail<ArticleDto>("Concurrency conflict", ResultErrorCode.Concurrency, conflictInfo));

        var categoriesHandler = Substitute.For<GetCategories.IGetCategoriesHandler>();
        categoriesHandler.HandleAsync().Returns(Result.Ok<IEnumerable<CategoryDto>?>(Enumerable.Empty<CategoryDto>()));

        Services.AddSingleton(getHandler);
        Services.AddSingleton(editHandler);
        Services.AddSingleton(categoriesHandler);

        // Act
        var cut = RenderComponent<Web.Components.Features.Articles.ArticleEdit.Edit>(parameters => parameters.Add(p => p.Id, articleId.ToString()));
        await cut.WaitForAssertionAsync(() => cut.Find("form"));

        var saveButton = cut.Find("button[type=submit]");
        saveButton.Click();

        // Wait for conflict panel
        await cut.WaitForAssertionAsync(() => cut.Find("[role=alert]"));

        // Assert presence of role and aria-live
        var panel = cut.Find("[role=alert]");
        panel.GetAttribute("aria-live").Should().Be("polite");

        // Assert changed fields list contains expected fields
        var listItems = cut.FindAll("[role=alert] ul li").Select(li => li.TextContent.Trim()).ToList();
        listItems.Should().Contain(new[] { "Title", "Content" });
    }
}
