// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     TestProjectTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

namespace Architecture.Tests;

/// <summary>
///   Tests to validate that all required test projects exist and are properly configured
/// </summary>
[ExcludeFromCodeCoverage]
public class TestProjectTests
{

	private readonly string _solutionRoot;

	private readonly string _testsFolder;

	public TestProjectTests()
	{

		// Dynamically find the solution root by walking up the directory tree
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

			if (parent == null)
			{
				break;
			}

			dir = parent.FullName;
		}

		if (foundRoot == null)
		{
			throw new DirectoryNotFoundException("Could not find solution root (ArticlesSite.slnx) in parent directories.");
		}

		_solutionRoot = foundRoot;
		_testsFolder = Path.Combine(foundRoot, "tests");

		if (!Directory.Exists(_testsFolder))
		{
			throw new DirectoryNotFoundException($"testsFolder not found: {_testsFolder}");
		}
	}

	[Fact]
	public void SharedTestsUnit_ShouldExist()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Shared.Tests.Unit project should exist");
	}

	[Fact]
	public void WebTestsUnit_ShouldExist()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Web.Tests.Unit project should exist");
	}

	[Fact]
	public void ArchitectureTestsUnit_ShouldExist()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Architecture.Tests project should exist");
	}

	[Fact]
	public void WebTestsIntegration_ShouldExist()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Web.Tests.Integration project should exist");
	}

	[Fact]
	public void AllTestProjects_ShouldBeMarkedAsTestProjects()
	{
		// Arrange
		string[] testProjectPaths = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj"),
				Path.Combine(_solutionRoot, "src", "ArticleSite.E2E", "ArticleSite.E2E.csproj")
		};

		// Act & Assert
		foreach (string projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				string content = File.ReadAllText(projectPath);

				content.Should().Contain("<IsTestProject>true</IsTestProject>",
						$"{Path.GetFileName(projectPath)} should be marked as a test project");
			}
		}
	}

	[Fact]
	public void AllTestProjects_ShouldReferenceXUnit()
	{
		// Arrange
		string[] testProjectPaths = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj")
		};

		// Act & Assert
		foreach (string projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				string content = File.ReadAllText(projectPath);

				content.Should().Contain("xunit",
						$"{Path.GetFileName(projectPath)} should reference xUnit");
			}
		}
	}

	[Fact]
	public void AllTestProjects_ShouldReferenceFluentAssertions()
	{
		// Arrange
		string[] testProjectPaths = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj")
		};

		// Act & Assert
		foreach (string projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				string content = File.ReadAllText(projectPath);

				content.Should().Contain("FluentAssertions",
						$"{Path.GetFileName(projectPath)} should reference FluentAssertions");
			}
		}
	}

	[Fact]
	public void UnitTestProjects_ShouldReferenceNSubstitute()
	{
		// Arrange
		string[] unitTestProjectPaths = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj")
		};

		// Act & Assert
		foreach (string projectPath in unitTestProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				string content = File.ReadAllText(projectPath);

				content.Should().Contain("NSubstitute",
						$"{Path.GetFileName(projectPath)} should reference NSubstitute for mocking");
			}
		}
	}

	[Fact]
	public void ArchitectureTests_ShouldReferenceNetArchTest()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj");

		// Act
		string content = File.ReadAllText(projectPath);

		// Assert
		content.Should().Contain("NetArchTest.Rules",
				"Architecture.Tests should reference NetArchTest.Rules");
	}

	[Fact]
	public void IntegrationTests_ShouldReferenceTestcontainers()
	{
		// Arrange
		string projectPath = Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj");

		// Act
		string content = File.ReadAllText(projectPath);

		// Assert
		content.Should().Contain("Testcontainers",
				"Web.Tests.Integration should reference Testcontainers");
	}

	[Fact]
	public void E2ETests_ShouldReferenceMicrosoftPlaywright()
	{
		// Arrange
		string projectPath = Path.Combine(_solutionRoot, "src", "ArticleSite.E2E", "ArticleSite.E2E.csproj");

		// Act & Assert
		if (File.Exists(projectPath))
		{
			string content = File.ReadAllText(projectPath);

			content.Should().Contain("Microsoft.Playwright",
					"ArticleSite.E2E should reference Microsoft.Playwright");
		}
	}

	[Fact]
	public void AllTestProjects_ShouldHaveGlobalUsingsFile()
	{
		// Arrange
		string[] testProjectFolders = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit"), Path.Combine(_testsFolder, "Web.Tests.Unit"),
				Path.Combine(_testsFolder, "Architecture.Tests"), Path.Combine(_testsFolder, "Web.Tests.Integration")
		};

		// Act & Assert
		foreach (string folder in testProjectFolders)
		{
			if (Directory.Exists(folder))
			{
				string globalUsingsPath = Path.Combine(folder, "GlobalUsings.cs");

				File.Exists(globalUsingsPath).Should().BeTrue(
						$"{Path.GetFileName(folder)} should have a GlobalUsings.cs file");
			}
		}
	}

	[Fact]
	public void TestProjects_ShouldNotBePackable()
	{
		// Arrange
		string[] testProjectPaths = new[]
		{
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
				Path.Combine(_testsFolder, "Architecture.Tests", "Architecture.Tests.csproj"),
				Path.Combine(_testsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj"),
				Path.Combine(_solutionRoot, "src", "ArticleSite.E2E", "ArticleSite.E2E.csproj")
		};

		// Act & Assert
		foreach (string projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				string content = File.ReadAllText(projectPath);

				content.Should().Contain("<IsPackable>false</IsPackable>",
						$"{Path.GetFileName(projectPath)} should not be packable");
			}
		}
	}

	[Fact]
	public void UnitTestProjects_ShouldReferenceAppropriateSourceProjects()
	{
		// Arrange & Act
		string sharedTestsContent = File.ReadAllText(
				Path.Combine(_testsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"));

		string webTestsContent = File.ReadAllText(
				Path.Combine(_testsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"));

		// Assert
		sharedTestsContent.Should().Contain("Shared.csproj",
				"Shared.Tests.Unit should reference Shared project");

		webTestsContent.Should().Contain("Web.csproj",
				"Web.Tests.Unit should reference Web project");

		webTestsContent.Should().Contain("Shared.csproj",
				"Web.Tests.Unit should reference Shared project");
	}

}