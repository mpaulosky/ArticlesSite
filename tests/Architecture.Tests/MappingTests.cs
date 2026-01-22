// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     MappingTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.IO;

using FluentAssertions;

using Xunit;

namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class MappingTests
{
	private readonly string _srcPath;

	public MappingTests()
	{
		string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
		string dir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
		string? foundRoot = null;

		while (!string.IsNullOrEmpty(dir))
		{
			string slnx = Path.Combine(dir, "ArticlesSite.slnx");
			if (File.Exists(slnx)) { foundRoot = dir; break; }
			DirectoryInfo? parent = Directory.GetParent(dir);
			if (parent == null) break;
			dir = parent.FullName;
		}

		if (foundRoot == null) throw new DirectoryNotFoundException("Solution root not found");
		_srcPath = Path.Combine(foundRoot, "src");
	}

	[Fact]
	public void MappingExtensions_ShouldMapVersion()
	{
		// Arrange - Find mapping extension files
		string webProjectPath = Path.Combine(_srcPath, "Web");
		string[] mappingFiles = Directory.GetFiles(webProjectPath, "*MappingExtensions.cs", SearchOption.AllDirectories);
		mappingFiles.Should().NotBeEmpty("because the project should contain mapping extension classes");

		// Act & Assert - Check each mapping file for Version mapping
		foreach (string mappingFile in mappingFiles)
		{
			string content = File.ReadAllText(mappingFile);
			string fileName = Path.GetFileName(mappingFile);

			// Skip AuthorInfo mappings as they may not need Version
			if (fileName.Contains("AuthorInfo"))
			{
				continue;
			}

			// Check for ToDto method that maps Version from entity to DTO
			if (content.Contains("ToDto"))
			{
				// Should map entity.Version to DTO
				bool mapsVersionToDto = content.Contains("Version = ") ||
																content.Contains("article.Version") ||
																content.Contains("category.Version") ||
																content.Contains(".Version");

				mapsVersionToDto.Should().BeTrue(
						$"Mapping {fileName} ToDto method should map Version from entity to DTO");
			}

			// Check for ToEntity method that maps Version from DTO to entity
			if (content.Contains("ToEntity"))
			{
				// Should map dto.Version to entity
				bool mapsVersionToEntity = content.Contains("Version = ") ||
																	 content.Contains("dto.Version") ||
																	 content.Contains(".Version");

				mapsVersionToEntity.Should().BeTrue(
						$"Mapping {fileName} ToEntity method should map Version from DTO to entity");
			}
		}
	}
}
