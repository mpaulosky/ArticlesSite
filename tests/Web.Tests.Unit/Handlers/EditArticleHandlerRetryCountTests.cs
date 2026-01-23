using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Interfaces;
using Web.Infrastructure;
using Shared.Abstractions;
using Xunit;

namespace Web.Tests.Unit.Handlers;

public class EditArticleHandlerRetryCountTests
{
    [Fact]
    public async Task When_AllAttemptsFail_HandlerInvokesUpdateArticle_InitialPlusMaxRetries()
    {
        // Arrange
        var repo = Substitute.For<IArticleRepository>();
        var logger = Substitute.For<ILogger<EditArticle.Handler>>();
        var validator = new ArticleDtoValidator();

        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();
        var original = new Article
        {
            Id = articleId,
            Title = "Original",
            Introduction = "Intro",
            Content = "Content",
            CoverImageUrl = "https://example.com/img.jpg",
            Slug = "original",
            IsPublished = false,
            IsArchived = false,
            Version = 0
        };

        repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(original));

        // UpdateArticle always returns concurrency
        repo.UpdateArticle(Arg.Any<Article>()).Returns(Result.Fail<Article>("Concurrency", ResultErrorCode.Concurrency));

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 3, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });
        var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null);

        var dto = new ArticleDto(articleId, "original", "Updated", "Intro", "Updated Content", null, null, null, false, null, null, DateTimeOffset.UtcNow, false, false, 0);

        // Act
        var result = await handler.HandleAsync(dto);

        // Assert - final result is failure and update was attempted initial + MaxRetries times
        result.Success.Should().BeFalse();
        var expectedCalls = 1 + options.Value.MaxRetries;
        await repo.Received(expectedCalls).UpdateArticle(Arg.Any<Article>());
    }

    [Fact]
    public async Task When_SucceedsAfterNFailures_HandlerInvokesUpdateArticle_InitialPlusN()
    {
        // Arrange
        var repo = Substitute.For<IArticleRepository>();
        var logger = Substitute.For<ILogger<EditArticle.Handler>>();
        var validator = new ArticleDtoValidator();

        var articleId = MongoDB.Bson.ObjectId.GenerateNewId();
        var original = new Article
        {
            Id = articleId,
            Title = "Original",
            Introduction = "Intro",
            Content = "Content",
            CoverImageUrl = "https://example.com/img.jpg",
            Slug = "original",
            IsPublished = false,
            IsArchived = false,
            Version = 0
        };

        repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(original));

        // Prepare sequence: fail N times then succeed
        int failCount = 2;
        var seq = new object[failCount + 1];
        for (int i = 0; i < failCount; i++) seq[i] = Result.Fail<Article>("Concurrency", ResultErrorCode.Concurrency);
        seq[failCount] = Result.Ok<Article>(original);

        repo.UpdateArticle(Arg.Any<Article>()).Returns(callInfo =>
        {
            var ret = seq[0];
            // shift left
            for (int i = 0; i < seq.Length - 1; i++) seq[i] = seq[i + 1];
            seq[^1] = Result.Fail<Article>("No more");
            return (Result<Article>)ret;
        });

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 5, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });
        var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null);

        var dto = new ArticleDto(articleId, "original", "Updated", "Intro", "Updated Content", null, null, null, false, null, null, DateTimeOffset.UtcNow, false, false, 0);

        // Act
        var result = await handler.HandleAsync(dto);

        // Assert - final result is success and UpdateArticle called initial + failCount times
        result.Success.Should().BeTrue();
        var expectedCalls = 1 + failCount;
        await repo.Received(expectedCalls).UpdateArticle(Arg.Any<Article>());
    }
}
