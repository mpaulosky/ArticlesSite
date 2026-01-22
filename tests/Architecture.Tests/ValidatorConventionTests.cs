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

	[Fact(Skip = "Draft - implement validator message robustness checks (use substring or error codes)")]
	public void ValidatorMessageRobustness()
	{
		// TODO: Inspect integration tests that assert exact error messages and suggest using substring or error codes
		// Example: If a test asserts exact message "Category name is required", make it assert Contains("required") or use the validator error key
	}
}
