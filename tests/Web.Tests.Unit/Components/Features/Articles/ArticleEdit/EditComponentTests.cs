using Bunit;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Web.Components.Features.Articles.ArticleEdit;

using Shared.Interfaces;
using Shared.Entities;

using Xunit;
using NSubstitute;

namespace Web.Tests.Unit.Components.Features.Articles.ArticleEdit;

public class EditComponentTests : TestContext
{

	[Fact]
	public void RendersLoadingComponent_WhenIsLoading()
	{
		var repo = Substitute.For<IArticleRepository>();

		repo.GetArticleByIdAsync(Arg.Any<ObjectId>())
				.Returns(Task.FromResult(Result.Fail<Article?>("Article not found.")));

		Services.AddSingleton(typeof(IArticleRepository), repo);
		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));
		cut.WaitForState(() => cut.Markup.Contains("LoadingComponent"));
		cut.Markup.Should().Contain("LoadingComponent");
	}

	[Fact]
	public void RendersErrorAlert_WhenErrorMessageIsSet()
	{
		var repo = Substitute.For<IArticleRepository>();

		repo.GetArticleByIdAsync(Arg.Any<ObjectId>())
				.Returns(Task.FromResult(Result.Fail<Article?>("Article not found.")));

		Services.AddSingleton(typeof(IArticleRepository), repo);
		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));
		cut.WaitForState(() => cut.Markup.Contains("Article not found."));
		cut.Markup.Should().Contain("Article not found.");
	}

	[Fact]
	public void RendersEditForm_WhenEditModelIsPresent()
	{
		var repo = Substitute.For<IArticleRepository>();

		var article = new Article
		{
			Id = ObjectId.Parse("507f1f77bcf86cd799439011"),
			Title = "Test Title",
			Introduction = "Test Introduction",
			Content = "Test Content",
			CoverImageUrl = "https://example.com/image.jpg",
			Author = new AuthorInfo("user1", "Test Author"),
			Category = new Category { CategoryName = "Tech" },
			IsPublished = true,
			IsArchived = false
		};

		repo.GetArticleByIdAsync(Arg.Any<ObjectId>())
				.Returns(Task.FromResult(Result.Ok<Article?>(article)));

		Services.AddSingleton(typeof(IArticleRepository), repo);
		var cut = RenderComponent<Edit>(parameters => parameters.Add(p => p.Id, "507f1f77bcf86cd799439011"));
		cut.WaitForState(() => cut.Markup.Contains("modern-card"));
		cut.Markup.Should().Contain("modern-card");
		cut.Markup.Should().Contain("Test Title");
		cut.Markup.Should().Contain("Test Introduction");
		cut.Markup.Should().Contain("Test Author");
		cut.Markup.Should().Contain("Tech");
	}

}