// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FileStorageTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Web.Tests.Unit
// =======================================================

using Microsoft.AspNetCore.Hosting;

namespace Web.Services;

/// <summary>
///   Unit tests for FileStorage service
/// </summary>
[ExcludeFromCodeCoverage]
public class FileStorageTests
{
	[Fact]
	public async Task AddFile_SavesFileAndReturnsUniqueName_WhenValidFile()
	{
		// Arrange
		var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(webRoot);
		var env = Substitute.For<IWebHostEnvironment>();
		env.WebRootPath.Returns(webRoot);
		var logger = Substitute.For<ILogger<FileStorage>>();
		var svc = new FileStorage(env, logger);

		var contentBytes = System.Text.Encoding.UTF8.GetBytes("hello");
		using var ms = new MemoryStream(contentBytes);
		var metadata = new FileMetaData("test.txt", "text/plain", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act
		var returned = await svc.AddFile(fileData);

		// Assert
		returned.Should().EndWith(".txt");
		var savedPath = Path.Combine(webRoot, "uploads", returned);
		File.Exists(savedPath).Should().BeTrue();
		var savedBytes = await File.ReadAllBytesAsync(savedPath);
		savedBytes.Should().Equal(contentBytes);

		// Cleanup
		Directory.Delete(webRoot, true);
	}

	[Fact]
	public async Task AddFile_CreatesUploadsDirectory_WhenMissing()
	{
		// Arrange
		var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		var env = Substitute.For<IWebHostEnvironment>();
		env.WebRootPath.Returns(webRoot);
		var logger = Substitute.For<ILogger<FileStorage>>();
		var svc = new FileStorage(env, logger);

		using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data"));
		var metadata = new FileMetaData("file.bin", "application/octet-stream", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act
		var returned = await svc.AddFile(fileData);

		// Assert
		var uploadsDir = Path.Combine(webRoot, "uploads");
		Directory.Exists(uploadsDir).Should().BeTrue();
		File.Exists(Path.Combine(uploadsDir, returned)).Should().BeTrue();

		// Cleanup
		Directory.Delete(webRoot, true);
	}

	[Fact]
	public async Task AddFile_CreatesZeroLengthFile_WhenContentEmpty()
	{
		// Arrange
		var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(webRoot);
		var env = Substitute.For<IWebHostEnvironment>();
		env.WebRootPath.Returns(webRoot);
		var logger = Substitute.For<ILogger<FileStorage>>();
		var svc = new FileStorage(env, logger);

		using var ms = new MemoryStream(); // empty
		var metadata = new FileMetaData("empty.txt", "text/plain", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act
		var returned = await svc.AddFile(fileData);

		// Assert
		var savedPath = Path.Combine(webRoot, "uploads", returned);
		File.Exists(savedPath).Should().BeTrue();
		new FileInfo(savedPath).Length.Should().Be(0);

		// Cleanup
		Directory.Delete(webRoot, true);
	}

	[Fact]
	public async Task AddFile_ReturnsNameWithoutExtension_WhenMetaDataNameHasNoExtension()
	{
		// Arrange
		var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(webRoot);
		var env = Substitute.For<IWebHostEnvironment>();
		env.WebRootPath.Returns(webRoot);
		var logger = Substitute.For<ILogger<FileStorage>>();
		var svc = new FileStorage(env, logger);

		using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("data"));
		var metadata = new FileMetaData("file", "application/octet-stream", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act
		var returned = await svc.AddFile(fileData);

		// Assert
		returned.Should().NotContain(".");
		File.Exists(Path.Combine(webRoot, "uploads", returned)).Should().BeTrue();

		// Cleanup
		Directory.Delete(webRoot, true);
	}

	[Fact]
	public async Task AddFile_ThrowsAndLogs_WhenCopyFails()
	{
		// Arrange
		var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(webRoot);
		var env = Substitute.For<IWebHostEnvironment>();
		env.WebRootPath.Returns(webRoot);
		var logger = Substitute.For<ILogger<FileStorage>>();
		var svc = new FileStorage(env, logger);

		using var ms = new ThrowingStream();
		var metadata = new FileMetaData("bad.txt", "text/plain", DateTime.UtcNow);
		var fileData = new FileData(ms, metadata);

		// Act & Assert
		await Assert.ThrowsAsync<InvalidOperationException>(() => svc.AddFile(fileData));

		// Verify logger was called with an error log
		logger.Received().Log(
			Arg.Is<LogLevel>(l => l == LogLevel.Error),
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception>(),
			Arg.Any<Func<object, Exception?, string>>());

		// Cleanup
		Directory.Delete(webRoot, true);
	}

	private class ThrowingStream : MemoryStream
	{
		public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			throw new InvalidOperationException("boom");
		}
	}
}
