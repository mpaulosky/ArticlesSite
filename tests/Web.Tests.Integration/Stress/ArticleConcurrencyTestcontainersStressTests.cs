using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Models;
using Xunit;

namespace Web.Tests.Integration.Stress;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleConcurrencyTestcontainersStressTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly MongoDbFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;

    public ArticleConcurrencyTestcontainersStressTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((ctx, config) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["MongoDb:ConnectionString"] = _fixture.ConnectionString,
                    ["MongoDb:Database"] = _fixture.Database.DatabaseNamespace.DatabaseName
                };
                config.AddInMemoryCollection(dict!);
            });
        });
    }

    [Fact(Timeout = 120_000)]
    public async Task Stress_ManyConcurrentApiWrites_UsingSharedTestFixture()
    {
        // Arrange - seed the database
        await _fixture.ClearCollectionsAsync();

        var article = FakeArticle.GetNewArticle(useSeed: true);
        article.Version = 0;
        var collection = _fixture.Database.GetCollection<Article>("Articles");
        await collection.InsertOneAsync(article);

        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        const int clients = 100; // heavier stress
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        var tasks = new List<Task<System.Net.Http.HttpResponseMessage>>(clients);

        for (int i = 0; i < clients; i++)
        {
            var dto = new ArticleDto(
                article.Id,
                article.Slug,
                $"Stress Edit {i}",
                article.Introduction,
                article.Content,
                article.CoverImageUrl,
                article.Author,
                article.Category,
                article.IsPublished,
                article.PublishedOn,
                article.CreatedOn,
                DateTimeOffset.UtcNow,
                article.IsArchived,
                true,
                article.Version
            );

            tasks.Add(client.PutAsJsonAsync($"/api/articles/{article.Id}", dto, cts.Token));
        }

        // Act
        await Task.WhenAll(tasks);

        var responses = tasks.Select(t => t.Result).ToList();
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var conflictCount = responses.Count(r => r.StatusCode == System.Net.HttpStatusCode.Conflict);

        // Assert
        successCount.Should().BeGreaterThanOrEqualTo(1, "at least one of the concurrent updates should succeed");
        conflictCount.Should().BeGreaterThanOrEqualTo(1, "concurrent contention should produce at least one 409 conflict");

        // Verify final DB state
        var saved = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(cts.Token);
        saved.Should().NotBeNull();
        saved!.Version.Should().BeGreaterThanOrEqualTo(0);

        // If any conflict responses contained structured details, ensure serverVersion is present
        foreach (var resp in responses.Where(r => r.StatusCode == System.Net.HttpStatusCode.Conflict))
        {
            var conflict = await resp.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>();
            conflict.Should().NotBeNull();
            conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
