// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ConcurrencyTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using FluentAssertions;

using Xunit;

namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class ConcurrencyTests
{
	private readonly string _srcPath;
	private readonly string _testsPath;

	public ConcurrencyTests()
	{
		// Minimal solution root discovery lifted from ArchitectureTests
		string assemblyLocation = Assembly.GetExecutingAssembly().Location;
		string dir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
		string? foundRoot = null;

		while (!string.IsNullOrEmpty(dir))
		{
			string slnx = Path.Combine(dir, "ArticlesSite.slnx");
			if (File.Exists(slnx))
			{
				foundRoot = dir;
				break;
			}

			DirectoryInfo? parent = Directory.GetParent(dir);
			if (parent == null) break;
			dir = parent.FullName;
		}

		if (foundRoot == null) throw new DirectoryNotFoundException("Could not find solution root (ArticlesSite.slnx)");

		_srcPath = Path.Combine(foundRoot, "src");
		_testsPath = Path.Combine(foundRoot, "tests");
	}

	[Fact]
	public void EntitiesAndDTOs_ShouldContainVersionProperty()
	{
		// Arrange - Find all DTO and Entity types
		string webProjectPath = Path.Combine(_srcPath, "Web");
		if (!Directory.Exists(webProjectPath))
		{
			throw new DirectoryNotFoundException($"Web project not found at {webProjectPath}");
		}

		// Find all *Dto.cs files
		string[] dtoFiles = Directory.GetFiles(webProjectPath, "*Dto.cs", SearchOption.AllDirectories);
		dtoFiles.Should().NotBeEmpty("because the project should contain DTO classes");

		// Find all entity files under Features/**/Entities/
		string[] entityFiles = Directory.GetFiles(webProjectPath, "*.cs", SearchOption.AllDirectories)
				.Where(f => f.Contains(Path.Combine("Features")) && f.Contains(Path.Combine("Entities")))
				.Where(f => !f.Contains("AuthorInfo")) // AuthorInfo might not need Version
				.ToArray();
		entityFiles.Should().NotBeEmpty("because the project should contain Entity classes");

		// Act & Assert - Check DTOs contain Version property
		foreach (string dtoFile in dtoFiles)
		{
			string content = File.ReadAllText(dtoFile);
			string fileName = Path.GetFileName(dtoFile);

			// Check for Version property declaration
			content.Should().Contain("Version", $"DTO {fileName} should have a Version property for optimistic concurrency");

			// Check it's a property (public int/long Version)
			bool hasVersionProperty = content.Contains("int Version") || content.Contains("long Version");
			hasVersionProperty.Should().BeTrue($"DTO {fileName} should declare Version as int or long property");
		}

		// Act & Assert - Check Entities contain Version property
		foreach (string entityFile in entityFiles)
		{
			string content = File.ReadAllText(entityFile);
			string fileName = Path.GetFileName(entityFile);

			// Check for Version property declaration
			content.Should().Contain("Version", $"Entity {fileName} should have a Version property for optimistic concurrency");

			// Check it's a property (public int/long Version)
			bool hasVersionProperty = content.Contains("int Version") || content.Contains("long Version");
			hasVersionProperty.Should().BeTrue($"Entity {fileName} should declare Version as int or long property");
		}
	}
}
