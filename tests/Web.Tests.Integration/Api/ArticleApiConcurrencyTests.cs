using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;
using FluentAssertions;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.ArticleEdit;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Entities;
using Web.Components.Features.Articles.Interfaces;

namespace Web.Tests.Integration.Api;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleApiConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly MongoDbFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;

    public ArticleApiConcurrencyTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;

        // Configure the app to use the same MongoDB instance as the integration fixture
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["MongoDb:ConnectionString"] = _fixture.ConnectionString,
                    ["MongoDb:Database"] = _fixture.Database.DatabaseNamespace.DatabaseName
                };
                conf.AddInMemoryCollection(dict!);
            });
        });
    }

    [Fact]
    public async Task ConcurrentApiUpdates_OneSucceeds_OtherReturnsConflict()
    {
        // Arrange - clear and insert article
        await _fixture.ClearCollectionsAsync();

        var article = FakeArticle.GetNewArticle(useSeed: true);
        var collection = _fixture.Database.GetCollection<Article>("Articles");
        await collection.InsertOneAsync(article, cancellationToken: TestContext.Current.CancellationToken);

        var client = _factory.CreateClient();

        var editDto1 = new ArticleDto(article.Id, article.Slug, "Edit Title A", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);
        var editDto2 = new ArticleDto(article.Id, article.Slug, "Edit Title B", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);

        // Act - send both updates concurrently
        var task1 = client.PutAsJsonAsync($"/api/articles/{article.Id}", editDto1);
        var task2 = client.PutAsJsonAsync($"/api/articles/{article.Id}", editDto2);

        await Task.WhenAll(task1, task2);

        var resp1 = await task1;
        var resp2 = await task2;

        // Assert - at least one success, at least one 409 conflict
        (resp1.IsSuccessStatusCode || resp2.IsSuccessStatusCode).Should().BeTrue();
        (resp1.StatusCode == System.Net.HttpStatusCode.Conflict || resp2.StatusCode == System.Net.HttpStatusCode.Conflict).Should().BeTrue();

        // If conflict response contains ConcurrencyConflictResponseDto, validate structure
        if (resp1.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflict = await resp1.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>();
            conflict.Should().NotBeNull();
            conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
        }

        if (resp2.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflict = await resp2.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>();
            conflict.Should().NotBeNull();
            conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
        }

        // Verify article exists in DB
        var saved = await collection.Find(a => a.Id == article.Id).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        saved.Should().NotBeNull();
    }
}
