// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     HealthCheckIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Tests.Integration;

/// <summary>
///   Integration tests for health check endpoints
/// </summary>
[ExcludeFromCodeCoverage]
public class HealthCheckIntegrationTests : IClassFixture<TestWebHostBuilder>
{

	private readonly TestWebHostBuilder _factory;
	private readonly HttpClient _client;

	public HealthCheckIntegrationTests(TestWebHostBuilder factory)
	{
		_factory = factory;
		_client = _factory.CreateClient();
	}

	[Fact]
	public async Task HealthCheck_Alive_ReturnsHealthy()
	{
		// Act
		var response = await _client.GetAsync("/health");

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task HealthCheck_Ready_ReturnsHealthy()
	{
		// Act
		var response = await _client.GetAsync("/alive");

		// Assert
		response.Should().NotBeNull();
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		content.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task HealthCheckService_CheckHealth_ReturnsHealthReport()
	{
		// Arrange
		using var scope = _factory.Services.CreateScope();

		// Act
		var healthReport = await HealthCheckTestHelper.RunHealthChecksAsync(scope.ServiceProvider);

		// Assert
		healthReport.Should().NotBeNull();
		healthReport.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
		healthReport.Entries.Should().NotBeEmpty();
	}

}
