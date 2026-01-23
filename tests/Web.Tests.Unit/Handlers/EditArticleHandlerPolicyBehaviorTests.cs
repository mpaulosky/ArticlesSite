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

public class EditArticleHandlerPolicyBehaviorTests
{
    [Fact]
    public async Task FallbackToDefaultPolicy_WhenNoPolicyInjected_RetriesAndSucceeds_MetricsRecorded()
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

        // GetArticle returns original on initial load
        repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(original));

        // UpdateArticle fails once with concurrency then succeeds
        repo.UpdateArticle(Arg.Any<Article>()).Returns(
            Result.Fail<Article>("Concurrency conflict", ResultErrorCode.Concurrency),
            Result.Ok<Article>(Arg.Any<Article>())
        );

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 3, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });
        var metrics = new Web.Tests.Unit.Infrastructure.InMemoryMetricsPublisher();

        // Act - pass null policy so handler uses fallback CreatePolicy
        var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null, metrics: metrics);

        var dto = new ArticleDto(articleId, "original", "Updated Title", "Intro", "Updated Content", "https://example.com/img.jpg", null, null, false, null, null, DateTimeOffset.UtcNow, false, false, 0);

        var result = await handler.HandleAsync(dto);

        // Assert
        result.Success.Should().BeTrue();
        await repo.Received(2).UpdateArticle(Arg.Any<Article>());

        // Metrics: at least one retry and one success recorded
        metrics.GetCount("retry").Should().BeGreaterThanOrEqualTo(1);
        metrics.GetCount("success").Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task TerminalFailureDuringOnRetry_ReloadFails_PropagatesFailure_MetricsRecorded()
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

        // Initial load returns original, subsequent reload returns failure
        repo.GetArticleByIdAsync(articleId).Returns(Result.Ok<Article?>(original), Result.Fail<Article?>("Reload failure"));

        // UpdateArticle always returns concurrency to force a retry and trigger reload
        repo.UpdateArticle(Arg.Any<Article>()).Returns(Result.Fail<Article>("Concurrency", ResultErrorCode.Concurrency));

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 2, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });
        var metrics = new Web.Tests.Unit.Infrastructure.InMemoryMetricsPublisher();

        var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null, metrics: metrics);

        var dto = new ArticleDto(articleId, "original", "Updated Title", "Intro", "Updated Content", "https://example.com/img.jpg", null, null, false, null, null, DateTimeOffset.UtcNow, false, false, 0);

        // Act
        var result = await handler.HandleAsync(dto);

        // Assert - handler should return the reload failure propagated
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Reload failure");

        // Metrics: conflict incremented and at least one retry recorded
        metrics.GetCount("conflict").Should().BeGreaterThanOrEqualTo(1);
        metrics.GetCount("retry").Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task PolicyHonors_MaxRetries_AttemptCountMatches_MetricsRecorded()
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

        // Always return concurrency to exercise retries
        repo.UpdateArticle(Arg.Any<Article>()).Returns(Result.Fail<Article>("Concurrency", ResultErrorCode.Concurrency));

        var options = Options.Create(new ConcurrencyOptions { MaxRetries = 4, BaseDelayMilliseconds = 0, MaxDelayMilliseconds = 0, JitterMilliseconds = 0 });
        var metrics = new Web.Tests.Unit.Infrastructure.InMemoryMetricsPublisher();

        var handler = new EditArticle.Handler(repo, logger, validator, options, concurrencyPolicy: null, metrics: metrics);

        var dto = new ArticleDto(articleId, "original", "Updated Title", "Intro", "Updated Content", "https://example.com/img.jpg", null, null, false, null, null, DateTimeOffset.UtcNow, false, false, 0);

        // Act
        var result = await handler.HandleAsync(dto);

        // Assert - handler should fail after exhausting retries
        result.Success.Should().BeFalse();

        // UpdateArticle should have been called MaxRetries times
        await repo.Received(options.Value.MaxRetries).UpdateArticle(Arg.Any<Article>());

        // Metrics: retry count should be equal to MaxRetries
        metrics.GetCount("retry").Should().Be(options.Value.MaxRetries);
        metrics.GetCount("conflict").Should().BeGreaterThanOrEqualTo(1);
    }
}
