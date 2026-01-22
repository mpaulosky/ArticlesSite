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

	[Fact(Skip = "Draft - implement mapping checks to ensure Version is mapped between DTOs and Entities")]
	public void MappingExtensions_ShouldMapVersion()
	{
		// TODO: Inspect mapping extension files for mapping of Version <-> Version
		// Example assertions:
		// - ArticleMappingExtensions contains `Version = dto.Version` when mapping dto->entity
		// - ArticleMappingExtensions contains `Version = entity.Version` when mapping entity->dto
	}
}
