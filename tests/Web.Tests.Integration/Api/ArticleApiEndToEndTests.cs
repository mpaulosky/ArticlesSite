using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Xunit;
using Web.Components.Features.Articles.Models;
using Web.Components.Features.Articles.Entities;
using System;

namespace Web.Tests.Integration.Api;

[Collection("MongoDb Collection")]
[ExcludeFromCodeCoverage]
public class ArticleApiEndToEndTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly MongoDbFixture _fixture;
    private readonly WebApplicationFactory<Program> _factory;

    public ArticleApiEndToEndTests(MongoDbFixture fixture, WebApplicationFactory<Program> factory)
    {
        _fixture = fixture;
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Inject configuration so the app connects to the test MongoDB instance
            builder.ConfigureAppConfiguration((context, cfg) =>
            {
                var dict = new Dictionary<string, string?>
                {
                    ["MongoDb:ConnectionString"] = _fixture.ConnectionString,
                    ["MongoDb:Database"] = _fixture.Database.DatabaseNamespace.DatabaseName
                };
                cfg.AddInMemoryCollection(dict!);
            });
        });
    }

    [Fact]
    public async Task EndToEnd_ConcurrentUpdate_OneSucceeds_OtherGets409()
    {
        // Arrange
        await _fixture.ClearCollectionsAsync();

        var article = FakeArticle.GetNewArticle(useSeed: true);
        var collection = _fixture.Database.GetCollection<Article>("Articles");
        await collection.InsertOneAsync(article);

        var client = _factory.CreateClient();

        var dto1 = new ArticleDto(article.Id, article.Slug, "E2E Title 1", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);
        var dto2 = new ArticleDto(article.Id, article.Slug, "E2E Title 2", article.Introduction, article.Content, article.CoverImageUrl, article.Author, article.Category, article.IsPublished, article.PublishedOn, article.CreatedOn, DateTimeOffset.UtcNow, article.IsArchived, true, article.Version);

        // Act
        var t1 = client.PutAsJsonAsync($"/api/articles/{article.Id}", dto1);
        var t2 = client.PutAsJsonAsync($"/api/articles/{article.Id}", dto2);
        await Task.WhenAll(t1, t2);

        var r1 = await t1;
        var r2 = await t2;

        // Assert at least one success and at least one 409
        (r1.IsSuccessStatusCode || r2.IsSuccessStatusCode).Should().BeTrue();
        (r1.StatusCode == System.Net.HttpStatusCode.Conflict || r2.StatusCode == System.Net.HttpStatusCode.Conflict).Should().BeTrue();

        // If we got Conflict, ensure the response contains structured payload
        if (r1.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflict = await r1.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>();
            conflict.Should().NotBeNull();
            conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
        }

        if (r2.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var conflict = await r2.Content.ReadFromJsonAsync<Web.Components.Features.Articles.Models.ConcurrencyConflictResponseDto>();
            conflict.Should().NotBeNull();
            conflict!.ServerVersion.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
