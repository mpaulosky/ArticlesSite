// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     ArchitectureTests.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Architecture.Tests
// =======================================================

namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class ArchitectureTests
{

	private readonly string _srcPath;

	private readonly string _testsPath;

	public ArchitectureTests()
	{

		// Dynamically find the solution root by walking up the directory tree
		string assemblyLocation = Assembly.GetExecutingAssembly().Location;
		string dir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
		Console.WriteLine($"[DEBUG] Starting solution root search from assembly location: {dir}");

		string? foundRoot = null;
		List<string> checkedDirs = [];
		int walkCount = 0;

		while (!string.IsNullOrEmpty(dir))
		{
			checkedDirs.Add(dir);
			walkCount++;
			string slnx = Path.Combine(dir, "ArticlesSite.slnx");
			Console.WriteLine($"[DEBUG] Walk {walkCount}: Checking {slnx}");

			if (File.Exists(slnx))
			{
				Console.WriteLine($"[DEBUG] Solution file found at: {dir}");
				foundRoot = dir;

				break;
			}

			DirectoryInfo? parent = Directory.GetParent(dir);

			if (parent == null)
			{
				Console.WriteLine($"[DEBUG] Reached drive root: {dir}");

				break;
			}

			dir = parent.FullName;
		}

		Console.WriteLine($"[DEBUG] Checked directories: {string.Join(", ", checkedDirs)}");

		if (foundRoot == null)
		{
			Console.WriteLine("[DEBUG] Solution root not found. Checked directories:");

			foreach (string d in checkedDirs)
			{
				Console.WriteLine($"  {d}");
			}

			throw new DirectoryNotFoundException("Could not find solution root (ArticlesSite.slnx) in parent directories.");
		}

		string solutionRoot = foundRoot;
		Path.Combine(solutionRoot, "ArticlesSite.slnx");
		_srcPath = Path.Combine(solutionRoot, "src");
		_testsPath = Path.Combine(solutionRoot, "tests");
		Console.WriteLine($"[DEBUG] Solution root: {solutionRoot}");
		Console.WriteLine($"[DEBUG] srcPath: {_srcPath}");
		Console.WriteLine($"[DEBUG] testsPath: {_testsPath}");

		if (!Directory.Exists(_srcPath))
		{
			Console.WriteLine($"ERROR: srcPath does not exist: {_srcPath}");

			throw new DirectoryNotFoundException($"srcPath not found: {_srcPath}");
		}

		if (!Directory.Exists(_testsPath))
		{
			Console.WriteLine($"ERROR: testsPath does not exist: {_testsPath}");

			throw new DirectoryNotFoundException($"testsPath not found: {_testsPath}");
		}
	}

	[Fact]
	public void AllSrcProjects_ShouldBeUsedOrEntryPoint()
	{
		Console.WriteLine($"[DEBUG] AllSrcProjects_ShouldBeUsedOrEntryPoint: _srcPath={_srcPath}");

		// Arrange
		string[] entryPoints = ["AppHost", "Web"];
		string[] srcProjects = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);

		HashSet<string> referenced = srcProjects.SelectMany(f => File.ReadAllText(f)
				.Split("<ProjectReference Include=\"")
				.Skip(1)
				.Select(x => x.Split('"')[0])
				.Select(r =>
						Path.GetFullPath(Path.Combine(Path.GetDirectoryName(f)!,
								r.Replace("/", Path.DirectorySeparatorChar.ToString()))))
		).ToHashSet();

		// Act & Assert
		foreach (string proj in srcProjects)
		{
			Path.GetFileNameWithoutExtension(proj);
			string dir = Path.GetFileName(Path.GetDirectoryName(proj)!);

			(referenced.Contains(proj) || entryPoints.Contains(dir)).Should()
					.BeTrue($"Project {proj} is unused and not an entry point");
		}
	}

	[Fact]
	public void SharedNugetPackages_ShouldHaveConsistentVersions()
	{
		Console.WriteLine($"[DEBUG] SharedNugetPackages_ShouldHaveConsistentVersions: _srcPath={_srcPath}");

		// Arrange
		string[] packages = ["FluentValidation", "MongoDB.Bson"];
		string[] csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		Dictionary<string, HashSet<string>> versions = new ();

		foreach (string pkg in packages)
		{
			versions[pkg] = new HashSet<string>();
		}

		// Act
		foreach (string file in csprojFiles)
		{
			string content = File.ReadAllText(file);

			foreach (string pkg in packages)
			{
				string[] lines = content.Split('\n').Where(l => l.Contains($"<PackageReference Include=\"{pkg}\"")).ToArray();

				foreach (string line in lines)
				{
					string? ver = line.Split("Version=\"").Skip(1).FirstOrDefault()?.Split('"')[0];

					if (!string.IsNullOrEmpty(ver))
					{
						versions[pkg].Add(ver);
					}
				}
			}
		}

		// Assert
		foreach (string pkg in packages)
		{
			versions[pkg].Count.Should()
					.BeLessThan(2, $"Package {pkg} has inconsistent versions: {string.Join(", ", versions[pkg])}");
		}
	}

	[Fact]
	public void TestProjects_ShouldNotReferenceEachOther()
	{
		Console.WriteLine($"[DEBUG] TestProjects_ShouldNotReferenceEachOther: _testsPath={_testsPath}");

		// Arrange
		string[] testProjects = Directory.GetFiles(_testsPath, "*.csproj", SearchOption.AllDirectories);

		foreach (string testProj in testProjects)
		{
			string content = File.ReadAllText(testProj);

			foreach (string otherTest in testProjects)
			{
				if (testProj != otherTest)
				{
					string otherName = Path.GetFileName(otherTest);
					content.Should().NotContain(otherName, $"Test project {testProj} should not reference {otherName}");
				}
			}
		}
	}

	[Fact]
	public void Web_ShouldNotReferenceAspireHostingPackages()
	{
		Console.WriteLine($"[DEBUG] Web_ShouldNotReferenceAspireHostingPackages: _srcPath={_srcPath}");

		// Arrange
		string webCsproj = Path.Combine(_srcPath, "Web", "Web.csproj");
		string content = File.ReadAllText(webCsproj);

		// Act & Assert
		content.Should().NotContain("Aspire.Hosting", "Web project should not reference Aspire.Hosting packages");
	}

	[Fact]
	public void OutputType_ShouldBeExeOnlyForEntryPoints()
	{
		Console.WriteLine($"[DEBUG] OutputType_ShouldBeExeOnlyForEntryPoints: _srcPath={_srcPath}");

		// Arrange
		string[] entryPoints = ["AppHost", "Web"];
		string[] csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);

		// Act & Assert
		foreach (string file in csprojFiles)
		{
			string content = File.ReadAllText(file);
			string dir = Path.GetFileName(Path.GetDirectoryName(file)!);

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
		Console.WriteLine($"[DEBUG] EachProject_ShouldContainGlobalUsingsFile: _srcPath={_srcPath}");

		// Arrange
		string[] projectDirs = Directory.GetDirectories(_srcPath)
			.Where(d => Directory.GetFiles(d, "*.csproj", SearchOption.TopDirectoryOnly).Any())
			.ToArray();

		// Act & Assert
		foreach (string dir in projectDirs)
		{
			string globalUsings = Path.Combine(dir, "GlobalUsings.cs");
			File.Exists(globalUsings).Should().BeTrue($"Missing GlobalUsings.cs in {dir}");
		}
	}

}