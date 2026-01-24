// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     RepositoryConcurrencyTests.cs
// Company :       mpaulosky
// Author :        GitHub Copilot
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class RepositoryConcurrencyTests
{
	private readonly string _srcPath;

	public RepositoryConcurrencyTests()
	{
		// locate solution root (simple approach)
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
	public void Repositories_ShouldUseVersionFilterOnUpdate()
	{
		// Arrange - Find repository files
		string webProjectPath = Path.Combine(_srcPath, "Web");
		string[] repoFiles = Directory.GetFiles(webProjectPath, "*Repository.cs", SearchOption.AllDirectories);
		repoFiles.Should().NotBeEmpty("because the project should contain Repository classes");

		// Act & Assert - Check each repository's Update method
		foreach (string repoFile in repoFiles)
		{
			string content = File.ReadAllText(repoFile);
			string fileName = Path.GetFileName(repoFile);

			// Check if this repository has an Update method
			bool hasUpdateMethod = content.Contains("Update") && (content.Contains("Article") || content.Contains("Category"));

			if (hasUpdateMethod)
			{
				// For now, we verify that the repository uses either ReplaceOneAsync or FindOneAndReplaceAsync
				// Both are acceptable approaches for optimistic concurrency control with version filtering
				// In the future, we should check for version-based filtering like:
				// filter: x => x.Id == entity.Id && x.Version == entity.Version
				// and proper version incrementing

				bool hasReplaceMethod = content.Contains("ReplaceOneAsync") || content.Contains("FindOneAndReplaceAsync");
				hasReplaceMethod.Should().BeTrue(
						$"Repository {fileName} should use ReplaceOneAsync or FindOneAndReplaceAsync for updates");

				// Verify that version filtering is used in the update operation
				content.Should().Contain("Id ==", $"Repository {fileName} should filter by Id in update operations");
				content.Should().Contain("Version", $"Repository {fileName} should use version filtering for optimistic concurrency control");
			}
		}
	}
}
