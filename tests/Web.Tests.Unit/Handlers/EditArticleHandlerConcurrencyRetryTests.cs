using FluentAssertions;
using NSubstitute;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.Interfaces;
using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Models;
using Shared.Abstractions;

namespace Web.Tests.Unit.Handlers;

public class EditArticleHandlerConcurrencyRetryTests
{
	[Fact]
	public async Task HandleAsync_RetriesOnConcurrencyConflict_AndEventuallySucceeds()
	{
		// Arrange
		var repo = Substitute.For<IArticleRepository>();
		var logger = Substitute.For<ILogger<EditArticle.Handler>>();
		var validator = new ArticleDtoValidator();

		var articleId = ObjectId.GenerateNewId();
		var originalArticle = new Article()
		{
			Id = articleId,
			Title = "Original Title",
			Introduction = "Intro",
			Content = "Content",
			CoverImageUrl = "https://example.com/image.jpg",
			Slug = "original_title",
			IsPublished = false,
			IsArchived = false,
			Version = 0
		};

		var latestArticle = new Article()
		{
			Id = articleId,
			Title = "Original Title",
			Introduction = "Intro",
			Content = "Content",
			CoverImageUrl = "https://example.com/image.jpg",
			Slug = "original_title",
			IsPublished = false,
			IsArchived = false,
			Version = 1
		};

		// GetArticleByIdAsync should return original first, then latest on retry
		repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(originalArticle), Result.Ok<Article?>(latestArticle));

		// UpdateArticle should fail first with concurrency conflict (typed), then succeed
		repo.UpdateArticle(Arg.Any<Article>()).Returns(
			Result.Fail<Article>("Concurrency conflict: article was modified by another process", ResultErrorCode.Concurrency),
			Result.Ok<Article>(Arg.Any<Article>())
		);

		var handler = new EditArticle.Handler(repo, logger, validator);

		var dto = new ArticleDto(
			articleId,
			"original_title",
			"Updated Title",
			"Intro",
			"Updated Content",
			null, // coverImageUrl
			null, // author
			null, // category
			false, // isPublished
			null, // publishedOn
			null, // createdOn
			DateTimeOffset.UtcNow, // modifiedOn
			false, // isArchived
			false // canEdit
		);

		// Act
		var result = await handler.HandleAsync(dto);

		// Assert
		result.Success.Should().BeTrue();
		// UpdateArticle should have been attempted at least twice (initial + retry)
		repo.Received(2).UpdateArticle(Arg.Any<Article>());
	}
}
