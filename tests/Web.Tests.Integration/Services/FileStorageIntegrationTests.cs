// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FileStorageIntegrationTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Integration
// =======================================================

using Microsoft.AspNetCore.Hosting;

using Web.Services;

namespace Web.Tests.Integration.Services;

/// <summary>
///   Integration tests for FileStorage service with dependency injection
/// </summary>
[ExcludeFromCodeCoverage]
public class FileStorageIntegrationTests : IDisposable
{
	private readonly string _webRoot;

	public FileStorageIntegrationTests()
	{
		_webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(_webRoot);
	}

	[Fact]
	public async Task AddFile_SavesFile_WhenResolvedFromScopedDI()
	{
		// Arrange - build a minimal DI container like the app would
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddSingleton<IWebHostEnvironment>(new TestEnv { WebRootPath = _webRoot });
		services.AddScoped<IFileStorage, FileStorage>();

		using var provider = services.BuildServiceProvider();
		using var scope = provider.CreateScope();
		var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

		var bytes = System.Text.Encoding.UTF8.GetBytes("integration");
		using var ms = new MemoryStream(bytes);
		var metadata = new FileMetaData("integ.txt", "text/plain", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act
		var returned = await fileStorage.AddFile(fileData);

		// Assert
		returned.Should().EndWith(".txt");
		File.Exists(Path.Combine(_webRoot, "uploads", returned)).Should().BeTrue();
	}

	public void Dispose()
	{
		try
		{
			if (Directory.Exists(_webRoot)) Directory.Delete(_webRoot, true);
		}
		catch
		{
			// best-effort cleanup
		}
	}

	private class TestEnv : IWebHostEnvironment
	{
		public string EnvironmentName { get; set; } = "Development";
		public string ApplicationName { get; set; } = "WebApp_TestHost";
		public string WebRootPath { get; set; } = string.Empty;
		public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
		public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
		public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
	}
}
