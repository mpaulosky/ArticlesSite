// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     DetailsComponentAdditionalTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Components;

namespace Web.Components.Features.Articles.ArticleDetails;

[ExcludeFromCodeCoverage]
public class DetailsComponentAdditionalTests : BunitContext
{
	public DetailsComponentAdditionalTests()
	{
		// Register default services used by many components
		Services.AddSingleton(Substitute.For<IFileStorage>());
	}

	[Fact]
	public void RendersDraftAndArchivedFlags_WhenArticleIsDraftAndArchived()
	{
		// Arrange
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var article = new ArticleDto(
				ObjectId.GenerateNewId(),
				"slug",
				"Title",
				"Intro",
				"# Heading",
				"https://example.com/img.jpg",
				new Web.Components.Features.AuthorInfo.Entities.AuthorInfo("u1", "A"),
				new Category { CategoryName = "Cat" },
				false, // IsPublished
				null,
				DateTimeOffset.UtcNow,
				DateTimeOffset.UtcNow,
				true, // IsArchived
				false
		);

		handler.HandleAsync(article.Id).Returns(Result.Ok(article));

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, article.Id.ToString()));

		// Assert
		cut.Markup.Should().Contain("Draft");
		cut.Markup.Should().Contain("Archived");
	}

	[Fact]
	public async Task EditButtonVisible_ForAdmin_AndNavigatesToEdit()
	{
		// Arrange
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		handler.HandleAsync(id).Returns(Result.Ok(article));

		// Provide a cascading AuthenticationState with the Admin role
		var claims = new[] { new Claim(ClaimTypes.Name, "AdminUser"), new Claim(ClaimTypes.Role, "Admin") };
		var identity = new ClaimsIdentity(claims, "Test");
		var principal = new ClaimsPrincipal(identity);

		var cut = Render<Details>(parameters => parameters
			.AddCascadingValue(Task.FromResult(new AuthenticationState(principal)))
			.Add(p => p.Id, id.ToString())
		);

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert Edit button exists
		cut.Markup.Should().Contain("Edit");

		// Act - click edit
		await cut.Find(".btn-primary").ClickAsync();

		// Assert navigation to edit page
		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/edit/{id}");
	}

	[Fact]
	public async Task EditButtonVisible_ForAuthor_WhenUserIdMatches()
	{
		// Arrange - create a custom auth provider that supplies NameIdentifier claim
		var authorId = "auth-123";
		var claims = new[] { new Claim(ClaimTypes.NameIdentifier, authorId), new Claim(ClaimTypes.Name, "AuthorUser") };
		var identity = new ClaimsIdentity(claims, "Test");
		var principal = new ClaimsPrincipal(identity);
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "Content", "", new Web.Components.Features.AuthorInfo.Entities.AuthorInfo(authorId, "AuthorUser"), null, true, DateTimeOffset.UtcNow, null, null, false, false);
		handler.HandleAsync(id).Returns(Result.Ok(article));

		var cut = Render<Details>(parameters => parameters
			.AddCascadingValue(Task.FromResult(new AuthenticationState(principal)))
			.Add(p => p.Id, id.ToString())
		);

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;

		// Assert Edit button exists
		cut.Markup.Should().Contain("Edit");
		// Click edit
		await cut.Find(".btn-primary").ClickAsync();
		var nav = Services.GetRequiredService<NavigationManager>();
		nav.Uri.Should().EndWith($"/articles/edit/{id}");
	}

	[Fact]
	public async Task RendersMarkdownContent_AsHtml()
	{
		// Arrange
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.GenerateNewId();
		var article = new ArticleDto(id, "slug", "Title", "Intro", "# Heading", "", null, null, true, DateTimeOffset.UtcNow, null, null, false, false);
		handler.HandleAsync(id).Returns(Result.Ok(article));

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, id.ToString()));

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		cut.Markup.Should().Contain("<h1>Heading</h1>");
	}

	[Fact]
	public async Task HandlerThrows_ShowsErrorMessage()
	{
		// Arrange
		var handler = Substitute.For<GetArticle.IGetArticleHandler>();
		Services.AddSingleton(handler);

		var id = ObjectId.GenerateNewId();
		handler.HandleAsync(id).Returns<Task<Result<ArticleDto>>>(_ => throw new Exception("boom"));

		var cut = Render<Details>(parameters => parameters.Add(p => p.Id, id.ToString()));

		// Ensure initialization completed deterministically
		var onInit = cut.Instance.GetType().GetMethod("OnInitializedAsync", BindingFlags.Instance | BindingFlags.NonPublic);
		if (onInit?.Invoke(cut.Instance, null) is Task onInitTask) await onInitTask;
		cut.Markup.Should().Contain("boom");
	}

	// Simple auth provider for custom claims
	private class TestAuthenticationStateProvider : AuthenticationStateProvider
	{
		private readonly AuthenticationState _state;
		public TestAuthenticationStateProvider(ClaimsPrincipal principal)
		{
			_state = new AuthenticationState(principal);
		}

		public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(_state);
	}
}
