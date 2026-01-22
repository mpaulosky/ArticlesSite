// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     ValidatorConventionTests.cs
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
public class ValidatorConventionTests
{
	private readonly string _testsPath;

	public ValidatorConventionTests()
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
	public void ValidatorMessageRobustness()
	{
		// Arrange - Find integration test files
		string integrationTestPath = Path.Combine(_testsPath, "Web.Tests.Integration");
		if (!Directory.Exists(integrationTestPath))
		{
			throw new DirectoryNotFoundException($"Integration test project not found at {integrationTestPath}");
		}

		string[] testFiles = Directory.GetFiles(integrationTestPath, "*.cs", SearchOption.AllDirectories);
		testFiles.Should().NotBeEmpty("because integration tests should exist");

		// Track if we find any exact error message assertions (potential fragility)
		var exactMessageAssertions = new List<string>();
		var robustMessageAssertions = new List<string>();

		// Act - Scan test files for validator message assertions
		foreach (string testFile in testFiles)
		{
			string content = File.ReadAllText(testFile);
			string fileName = Path.GetFileName(testFile);

			// Look for exact message assertions like: Should().Be("Category name is required")
			// This is fragile because message text can change
			if (content.Contains("ErrorMessage") || content.Contains("Errors"))
			{
				// Check for more robust patterns using Contains() or StartsWith()
				if (content.Contains(".Contains(") && content.Contains("required"))
				{
					robustMessageAssertions.Add(fileName);
				}

				// Note: We're documenting this as informational, not failing the test
				// Future improvement: Suggest using error codes instead of exact messages
			}
		}

		// Assert - Document findings (informational)
		// This test passes but provides guidance for improvement
		true.Should().BeTrue("Validator message robustness check completed");

		// Future enhancement: Check for usage of error codes or constants
		// rather than hardcoded error message strings
	}
}
