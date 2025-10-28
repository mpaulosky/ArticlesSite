// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     PageIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

namespace Web.Tests.Integration;

/// <summary>
///   Integration tests for page rendering and routing
/// </summary>
[ExcludeFromCodeCoverage]
public class PageIntegrationTests : IClassFixture<TestWebHostBuilder>
{

	private readonly TestWebHostBuilder _factory;
	private readonly HttpClient _client;

	public PageIntegrationTests(TestWebHostBuilder factory)
	{
		_factory = factory;
		_client = _factory.CreateClient();
	}

	[Fact]
	public async Task HomePage_Returns_Success()
	{
		// Act
		var response = await _client.GetAsync("/");

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task AboutPage_Returns_Success()
	{
		// Act
		var response = await _client.GetAsync("/about");

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task ContactPage_Returns_Success()
	{
		// Act
		var response = await _client.GetAsync("/contact");

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task NotFoundPage_Returns_NotFound()
	{
		// Act
		var response = await _client.GetAsync("/this-page-does-not-exist");

		// Assert
		response.Should().NotBeNull();
		response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("/about")]
	[InlineData("/contact")]
	public async Task PublicPages_AccessibleWithoutAuthentication(string url)
	{
		// Act
		var response = await _client.GetAsync(url);

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode("public pages should be accessible without authentication");
	}

	[Fact]
	public async Task AdminPage_WithoutAuthentication_RedirectsToLogin()
	{
		// Arrange
		var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		// Act
		var response = await client.GetAsync("/admin");

		// Assert
		response.Should().NotBeNull();

		// Should either redirect or return unauthorized
		response.StatusCode.Should().BeOneOf(
			System.Net.HttpStatusCode.Redirect,
			System.Net.HttpStatusCode.Found,
			System.Net.HttpStatusCode.Unauthorized);
	}

}
