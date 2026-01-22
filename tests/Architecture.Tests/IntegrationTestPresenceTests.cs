// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     IntegrationTestPresenceTests.cs
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
public class IntegrationTestPresenceTests
{
	private readonly string _testsPath;

	public IntegrationTestPresenceTests()
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
		_testsPath = Path.Combine(foundRoot, "tests");
	}

	[Fact]
	public void IntegrationConcurrencyTestExistence()
	{
		// Arrange - Find integration test directory
		string integrationTestPath = Path.Combine(_testsPath, "Web.Tests.Integration");
		if (!Directory.Exists(integrationTestPath))
		{
			throw new DirectoryNotFoundException($"Integration test project not found at {integrationTestPath}");
		}

		// Act & Assert - Check for ArticleConcurrencyTests.cs
		string articleConcurrencyTestFile = Path.Combine(integrationTestPath, "Handlers", "Articles", "ArticleConcurrencyTests.cs");
		File.Exists(articleConcurrencyTestFile).Should().BeTrue(
				"ArticleConcurrencyTests.cs should exist to test article concurrency scenarios");

		// Check content includes concurrency-related tests
		string articleContent = File.ReadAllText(articleConcurrencyTestFile);
		articleContent.Should().Contain("Version", "ArticleConcurrencyTests should include Version-related tests");
		articleContent.Should().Contain("Concurrent", "ArticleConcurrencyTests should test concurrent scenarios");
		articleContent.Should().Contain("Simultaneous", "ArticleConcurrencyTests should test simultaneous edits");

		// Act & Assert - Check for CategoryConcurrencyTests.cs
		string categoryConcurrencyTestFile = Path.Combine(integrationTestPath, "Handlers", "Categories", "CategoryConcurrencyTests.cs");
		File.Exists(categoryConcurrencyTestFile).Should().BeTrue(
				"CategoryConcurrencyTests.cs should exist to test category concurrency scenarios");

		// Check content includes concurrency-related tests
		string categoryContent = File.ReadAllText(categoryConcurrencyTestFile);
		categoryContent.Should().Contain("Version", "CategoryConcurrencyTests should include Version-related tests");
		categoryContent.Should().Contain("Concurrent", "CategoryConcurrencyTests should test concurrent scenarios");
		categoryContent.Should().Contain("Simultaneous", "CategoryConcurrencyTests should test simultaneous edits");
	}
}
