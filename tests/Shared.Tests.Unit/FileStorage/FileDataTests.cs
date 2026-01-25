//=======================================================
//Copyright (c) 2026. All rights reserved.
//File Name :     FileDataTests.cs
//Company :       mpaulosky
//Author :        GitHub Copilot
//Solution Name : ArticlesSite
//Project Name :  Shared.Tests.Unit
//=======================================================

using Shared.FileStorage;

namespace Shared.Tests.Unit.FileStorage;

[ExcludeFromCodeCoverage]
public class FileDataTests
{
	[Fact]
	public void Missing_ShouldReturnNullStreamAndEmptyMetadata()
	{
		// Act
		FileData missing = FileData.Missing;

		// Assert
		missing.File.Should().BeSameAs(Stream.Null);
		missing.Metadata.FileName.Should().BeEmpty();
		missing.Metadata.ContentType.Should().BeEmpty();
		missing.Metadata.CreateDate.Should().Be(DateTimeOffset.MinValue);
	}

	[Fact]
	public void ValidateFileName_ShouldThrow_WhenFileNameIsEmpty()
	{
		// Arrange
		var meta = new FileMetaData(string.Empty, "application/octet-stream", DateTimeOffset.UtcNow);

		// Act
		Action act = () => meta.ValidateFileName();

		// Assert
		act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be(nameof(FileMetaData.FileName));
	}

	[Fact]
	public void ValidateFileName_ShouldThrow_WhenFileNameHasInvalidCharacters()
	{
		// Arrange
		var meta = new FileMetaData("bad/name.txt", "application/octet-stream", DateTimeOffset.UtcNow);

		// Act
		Action act = () => meta.ValidateFileName();

		// Assert
		act.Should().Throw<ArgumentException>().Which.ParamName.Should().Be(nameof(FileMetaData.FileName));
	}

	[Fact]
	public void ValidateFileName_ShouldNotThrow_ForValidFileName()
	{
		// Arrange
		var meta = new FileMetaData("file-1_2.3.txt", "application/octet-stream", DateTimeOffset.UtcNow);

		// Act
		Action act = () => meta.ValidateFileName();

		// Assert
		act.Should().NotThrow();
	}
}
