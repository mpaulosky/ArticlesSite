// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     TestProjectTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticleSite
// Project Name :  Architecture.Tests.Unit
// =======================================================

namespace Architecture.Tests.Unit;

/// <summary>
///   Tests to validate that all required test projects exist and are properly configured
/// </summary>
[ExcludeFromCodeCoverage]
public class TestProjectTests
{

	private const string SolutionRoot = @"e:\github\ArticleSite\src";
	private const string TestsFolder = @"e:\github\ArticleSite\tests";

	[Fact]
	public void SharedTestsUnit_ShouldExist()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Shared.Tests.Unit project should exist");
	}

	[Fact]
	public void WebTestsUnit_ShouldExist()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Web.Tests.Unit project should exist");
	}

	[Fact]
	public void ArchitectureTestsUnit_ShouldExist()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Architecture.Tests.Unit project should exist");
	}

	[Fact]
	public void WebTestsIntegration_ShouldExist()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj");

		// Act & Assert
		File.Exists(projectPath).Should().BeTrue("Web.Tests.Integration project should exist");
	}

	[Fact]
	public void AllTestProjects_ShouldBeMarkedAsTestProjects()
	{
		// Arrange
		var testProjectPaths = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj"),
			Path.Combine(SolutionRoot, "ArticleSite.E2E", "ArticleSite.E2E.csproj")
		};

		// Act & Assert
		foreach (var projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				var content = File.ReadAllText(projectPath);
				content.Should().Contain("<IsTestProject>true</IsTestProject>",
					$"{Path.GetFileName(projectPath)} should be marked as a test project");
			}
		}
	}

	[Fact]
	public void AllTestProjects_ShouldReferenceXUnit()
	{
		// Arrange
		var testProjectPaths = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj")
		};

		// Act & Assert
		foreach (var projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				var content = File.ReadAllText(projectPath);
				content.Should().Contain("xunit",
					$"{Path.GetFileName(projectPath)} should reference xUnit");
			}
		}
	}

	[Fact]
	public void AllTestProjects_ShouldReferenceFluentAssertions()
	{
		// Arrange
		var testProjectPaths = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj")
		};

		// Act & Assert
		foreach (var projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				var content = File.ReadAllText(projectPath);
				content.Should().Contain("FluentAssertions",
					$"{Path.GetFileName(projectPath)} should reference FluentAssertions");
			}
		}
	}

	[Fact]
	public void UnitTestProjects_ShouldReferenceNSubstitute()
	{
		// Arrange
		var unitTestProjectPaths = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj")
		};

		// Act & Assert
		foreach (var projectPath in unitTestProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				var content = File.ReadAllText(projectPath);
				content.Should().Contain("NSubstitute",
					$"{Path.GetFileName(projectPath)} should reference NSubstitute for mocking");
			}
		}
	}

	[Fact]
	public void ArchitectureTests_ShouldReferenceNetArchTest()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj");

		// Act
		var content = File.ReadAllText(projectPath);

		// Assert
		content.Should().Contain("NetArchTest.Rules",
			"Architecture.Tests.Unit should reference NetArchTest.Rules");
	}

	[Fact]
	public void IntegrationTests_ShouldReferenceTestcontainers()
	{
		// Arrange
		var projectPath = Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj");

		// Act
		var content = File.ReadAllText(projectPath);

		// Assert
		content.Should().Contain("Testcontainers",
			"Web.Tests.Integration should reference Testcontainers");
	}

	[Fact]
	public void E2ETests_ShouldReferenceMicrosoftPlaywright()
	{
		// Arrange
		var projectPath = Path.Combine(SolutionRoot, "ArticleSite.E2E", "ArticleSite.E2E.csproj");

		// Act & Assert
		if (File.Exists(projectPath))
		{
			var content = File.ReadAllText(projectPath);
			content.Should().Contain("Microsoft.Playwright",
				"ArticleSite.E2E should reference Microsoft.Playwright");
		}
	}

	[Fact]
	public void AllTestProjects_ShouldHaveGlobalUsingsFile()
	{
		// Arrange
		var testProjectFolders = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit"),
			Path.Combine(TestsFolder, "Web.Tests.Unit"),
			Path.Combine(TestsFolder, "Architecture.Tests.Unit"),
			Path.Combine(TestsFolder, "Web.Tests.Integration")
		};

		// Act & Assert
		foreach (var folder in testProjectFolders)
		{
			if (Directory.Exists(folder))
			{
				var globalUsingsPath = Path.Combine(folder, "GlobalUsings.cs");
				File.Exists(globalUsingsPath).Should().BeTrue(
					$"{Path.GetFileName(folder)} should have a GlobalUsings.cs file");
			}
		}
	}

	[Fact]
	public void TestProjects_ShouldNotBePackable()
	{
		// Arrange
		var testProjectPaths = new[]
		{
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Architecture.Tests.Unit", "Architecture.Tests.Unit.csproj"),
			Path.Combine(TestsFolder, "Web.Tests.Integration", "Web.Tests.Integration.csproj"),
			Path.Combine(SolutionRoot, "ArticleSite.E2E", "ArticleSite.E2E.csproj")
		};

		// Act & Assert
		foreach (var projectPath in testProjectPaths)
		{
			if (File.Exists(projectPath))
			{
				var content = File.ReadAllText(projectPath);
				content.Should().Contain("<IsPackable>false</IsPackable>",
					$"{Path.GetFileName(projectPath)} should not be packable");
			}
		}
	}

	[Fact]
	public void UnitTestProjects_ShouldReferenceAppropriateSourceProjects()
	{
		// Arrange & Act
		var sharedTestsContent = File.ReadAllText(
			Path.Combine(TestsFolder, "Shared.Tests.Unit", "Shared.Tests.Unit.csproj"));
		var webTestsContent = File.ReadAllText(
			Path.Combine(TestsFolder, "Web.Tests.Unit", "Web.Tests.Unit.csproj"));

		// Assert
		sharedTestsContent.Should().Contain("Shared.csproj",
			"Shared.Tests.Unit should reference Shared project");

		webTestsContent.Should().Contain("Web.csproj",
			"Web.Tests.Unit should reference Web project");
		webTestsContent.Should().Contain("Shared.csproj",
			"Web.Tests.Unit should reference Shared project");
	}

}