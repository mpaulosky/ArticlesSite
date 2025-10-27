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
		List<string> checkedDirs = new ();
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
		List<string> unusedProjects = new();
		foreach (string proj in srcProjects)
		{
			Path.GetFileNameWithoutExtension(proj);
			string dir = Path.GetFileName(Path.GetDirectoryName(proj)!);

			// Skip Api project as it has been removed from the solution
			if (dir.Equals("Api", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			if (!(referenced.Contains(proj) || entryPoints.Contains(dir)))
			{
				unusedProjects.Add(proj);
			}

			(referenced.Contains(proj) || entryPoints.Contains(dir)).Should()
					.BeTrue($"Project {proj} is unused and not an entry point");
		}
		if (unusedProjects.Count > 0)
		{
			Console.WriteLine($"[DEBUG] Unused projects: {string.Join(", ", unusedProjects)}");
		}
	}

	[Fact]
	public void SharedNugetPackages_ShouldHaveConsistentVersions()
	{
		Console.WriteLine($"[DEBUG] SharedNugetPackages_ShouldHaveConsistentVersions: _srcPath={_srcPath}");

		// Arrange
		string[] csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		Dictionary<string, HashSet<string>> packageVersions = new();

		// Act
		foreach (string file in csprojFiles)
		{
			string content = File.ReadAllText(file);
			var lines = content.Split('\n').Where(l => l.Contains("<PackageReference Include="));
			foreach (var line in lines)
			{
				var pkgSplit = line.Split("<PackageReference Include=\"");
				if (pkgSplit.Length < 2) continue;
				var pkgName = pkgSplit[1].Split('"')[0];
				var verSplit = line.Split("Version=\"");
				string? ver = verSplit.Length > 1 ? verSplit[1].Split('"')[0] : null;
				if (!string.IsNullOrEmpty(pkgName) && !string.IsNullOrEmpty(ver))
				{
					if (!packageVersions.ContainsKey(pkgName))
						packageVersions[pkgName] = new HashSet<string>();
					packageVersions[pkgName].Add(ver);
				}
			}
		}

		// Assert: Only check packages referenced in more than one project
		foreach (var kvp in packageVersions)
		{
			if (kvp.Value.Count > 1)
			{
				kvp.Value.Count.Should()
					.BeLessThan(2, $"Package {kvp.Key} has inconsistent versions: {string.Join(", ", kvp.Value)}");
			}
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
			var referencedProjects = content.Split("<ProjectReference Include=\"")
				.Skip(1)
				.Select(x => x.Split('"')[0])
				.Select(r => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(testProj)!, r.Replace("/", Path.DirectorySeparatorChar.ToString()))))
				.ToList();

			foreach (string otherTest in testProjects)
			{
				if (testProj != otherTest)
				{
					if (referencedProjects.Contains(otherTest))
					{
						throw new Xunit.Sdk.XunitException($"Test project {testProj} should not reference {otherTest}");
					}
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
		var forbiddenPrefixes = new[] { "Aspire." };
		var allowedAspirePackages = new[] { "Aspire.MongoDB.Driver" };
		var forbiddenRefs = content.Split('\n')
			.Where(l => l.Contains("<PackageReference Include="))
			.Select(l => l.Split("<PackageReference Include=\"")[1].Split('"')[0])
			.Where(pkg => forbiddenPrefixes.Any(pkg.StartsWith) && !allowedAspirePackages.Contains(pkg))
			.ToList();
		forbiddenRefs.Count.Should().Be(0, $"Web project should not reference forbidden Aspire.* packages: {string.Join(", ", forbiddenRefs)}");
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

			bool hasExeOutputType = content.Contains("<OutputType>Exe</OutputType>");
			if (entryPoints.Contains(dir))
			{
				hasExeOutputType.Should().BeTrue($"{dir} should be an Exe");
			}
			else
			{
				hasExeOutputType.Should().BeFalse($"{dir} should not be an Exe");
			}
		}
	}

	[Fact]
	public void EachProject_ShouldContainGlobalUsingsFile()
	{
		Console.WriteLine($"[DEBUG] EachProject_ShouldContainGlobalUsingsFile: _srcPath={_srcPath}");

		// Arrange
		string[] csprojFiles = Directory.GetFiles(_srcPath, "*.csproj", SearchOption.AllDirectories);
		var missingGlobalUsings = new List<string>();

		// Act & Assert
		foreach (string csproj in csprojFiles)
		{
			string dir = Path.GetDirectoryName(csproj)!;
			string globalUsings = Path.Combine(dir, "GlobalUsings.cs");
			if (!File.Exists(globalUsings))
			{
				missingGlobalUsings.Add(dir);
			}
			File.Exists(globalUsings).Should().BeTrue($"Missing GlobalUsings.cs in {dir}");
		}
		if (missingGlobalUsings.Count > 0)
		{
			Console.WriteLine($"[DEBUG] Missing GlobalUsings.cs in: {string.Join(", ", missingGlobalUsings)}");
		}
	}

}