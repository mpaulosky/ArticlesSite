using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Polly;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Interfaces;
using Web.Infrastructure;
using Shared.Abstractions;
using Xunit;

namespace Web.Tests.Unit.Handlers;

public class EditArticleHandlerPolicyInjectionTests
{
    [Fact]
    public async Task Handler_Uses_Injected_Policy_OnConcurrencyRetry()
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

        // Make UpdateArticle fail first with concurrency then succeed
        repo.UpdateArticle(Arg.Any<Article>()).Returns(
            Result.Fail<Article>("Concurrency conflict", ResultErrorCode.Concurrency),
            Result.Ok<Article>(Arg.Any<Article>())
        );

        int policyRetryInvocations = 0;

        // Create a policy with an onRetry that increments our counter
        var policy = Policy<Result<Article>>
            .HandleResult(r => r.Failure && r.ErrorCode == ResultErrorCode.Concurrency)
            .WaitAndRetryAsync(new[] { TimeSpan.Zero }, onRetryAsync: (outcome, timespan, retryCount, context) =>
            {
                policyRetryInvocations++;
                return Task.CompletedTask;
            });

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 3, BaseDelayMilliseconds = 10, MaxDelayMilliseconds = 100, JitterMilliseconds = 5 });

        var handler = new EditArticle.Handler(repo, logger, validator, options, policy);

        var dto = new ArticleDto(
            articleId,
            "original",
            "Updated Title",
            "Intro",
            "Updated Content",
            "https://example.com/img.jpg",
            null,
            null,
            false,
            null,
            null,
            DateTimeOffset.UtcNow,
            false,
            false,
            0
        );

        // Act
        var result = await handler.HandleAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        policyRetryInvocations.Should().BeGreaterThan(0);
        await repo.Received(2).UpdateArticle(Arg.Any<Article>());
    }
}
