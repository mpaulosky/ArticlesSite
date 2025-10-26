using System.Diagnostics.CodeAnalysis;

using Xunit;

namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class ArchitectureTests
{
	private readonly string _solutionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ArticlesSite.slnx");
	private readonly string _srcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "src");
	private readonly string _testsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "tests");

	[Fact]
	public void AllSrcProjects_ShouldBeUsedOrEntryPoint()
	{
		// Arrange
		var entryPoints = new[] { "AppHost", "Web" };
		var srcProjects = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		var referenced = srcProjects.SelectMany(f => File.ReadAllText(f)
			.Split("<ProjectReference Include=\"")
			.Skip(1)
			.Select(x => x.Split('"')[0])
			.Select(r => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(f)!, r.Replace("/", Path.DirectorySeparatorChar.ToString()))))
		).ToHashSet();

		// Act & Assert
		foreach (var proj in srcProjects)
		{
			var name = Path.GetFileNameWithoutExtension(proj);
			var dir = Path.GetFileName(Path.GetDirectoryName(proj)!);
			(referenced.Contains(proj) || entryPoints.Contains(dir)).Should().BeTrue($"Project {proj} is unused and not an entry point");
		}
	}

	[Fact]
	public void TestProjects_ShouldReferenceOnlyCorrespondingMainProject()
	{
		// Arrange
		var testProjects = Directory.GetFiles(_testsPath, "*.csproj", SearchOption.AllDirectories);
		foreach (var testProj in testProjects)
		{
			var content = File.ReadAllText(testProj);
			var mainProjectName = Path.GetFileNameWithoutExtension(testProj).Replace(".Tests.Unit", "").Replace(".Tests.Integration", "");
			// Act & Assert
			if (mainProjectName != "Architecture")
			{
				content.Should().Contain($"..\\..\\src\\{mainProjectName}\\{mainProjectName}.csproj");
			}
		}
	}

	[Fact]
	public void SharedNugetPackages_ShouldHaveConsistentVersions()
	{
		// Arrange
		var packages = new[] { "FluentValidation", "MongoDB.Bson" };
		var csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		var versions = new Dictionary<string, HashSet<string>>();
		foreach (var pkg in packages)
		{
			versions[pkg] = new HashSet<string>();
		}
		// Act
		foreach (var file in csprojFiles)
		{
			var content = File.ReadAllText(file);
			foreach (var pkg in packages)
			{
				var lines = content.Split('\n').Where(l => l.Contains($"<PackageReference Include=\"{pkg}\"")).ToArray();
				foreach (var line in lines)
				{
					var ver = line.Split("Version=\"").Skip(1).FirstOrDefault()?.Split('"')[0];
					if (!string.IsNullOrEmpty(ver)) versions[pkg].Add(ver);
				}
			}
		}
		// Assert
		foreach (var pkg in packages)
		{
			versions[pkg].Count.Should().BeLessThan(2, $"Package {pkg} has inconsistent versions: {string.Join(", ", versions[pkg])}");
		}
	}

	[Fact]
	public void TestProjects_ShouldNotReferenceEachOther()
	{
		// Arrange
		var testProjects = Directory.GetFiles(_testsPath, "*.csproj", SearchOption.AllDirectories);
		foreach (var testProj in testProjects)
		{
			var content = File.ReadAllText(testProj);
			foreach (var otherTest in testProjects)
			{
				if (testProj != otherTest)
				{
					var otherName = Path.GetFileName(otherTest);
					content.Should().NotContain(otherName, $"Test project {testProj} should not reference {otherName}");
				}
			}
		}
	}

	[Fact]
	public void Web_ShouldNotReferenceAspireHostingPackages()
	{
		// Arrange
		var webCsproj = Path.Combine(_srcPath, "Web", "Web.csproj");
		var content = File.ReadAllText(webCsproj);
		// Act & Assert
		content.Should().NotContain("Aspire.Hosting", "Web project should not reference Aspire.Hosting packages");
	}

	[Fact]
	public void OutputType_ShouldBeExeOnlyForEntryPoints()
	{
		// Arrange
		var entryPoints = new[] { "AppHost", "Web" };
		var csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		// Act & Assert
		foreach (var file in csprojFiles)
		{
			var content = File.ReadAllText(file);
			var dir = Path.GetFileName(Path.GetDirectoryName(file)!);
			if (entryPoints.Contains(dir))
			{
				content.Should().Contain("<OutputType>Exe</OutputType>", $"{dir} should be an Exe");
			}
			else
			{
				content.Should().NotContain("<OutputType>Exe</OutputType>", $"{dir} should not be an Exe");
			}
		}
	}

	[Fact]
	public void EachProject_ShouldContainGlobalUsingsFile()
	{
		// Arrange
		var projectDirs = Directory.GetDirectories(_srcPath);
		// Act & Assert
		foreach (var dir in projectDirs)
		{
			var globalUsings = Path.Combine(dir, "GlobalUsings.cs");
			File.Exists(globalUsings).Should().BeTrue($"Missing GlobalUsings.cs in {dir}");
		}
	}
}
