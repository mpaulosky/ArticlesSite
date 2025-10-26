namespace Architecture.Tests;

[ExcludeFromCodeCoverage]
public class ArchitectureTests
{

	private readonly string _srcPath;
	private readonly string _testsPath;

	public ArchitectureTests()
	{

		// Dynamically find the solution root by walking up the directory tree
		var assemblyLocation = Assembly.GetExecutingAssembly().Location;
		var dir = Path.GetDirectoryName(assemblyLocation) ?? Directory.GetCurrentDirectory();
		Console.WriteLine($"[DEBUG] Starting solution root search from assembly location: {dir}");

		string? foundRoot = null;
		var checkedDirs = new List<string>();
		int walkCount = 0;
		while (!string.IsNullOrEmpty(dir))
		{
			checkedDirs.Add(dir);
			walkCount++;
			var slnx = Path.Combine(dir, "ArticlesSite.slnx");
			Console.WriteLine($"[DEBUG] Walk {walkCount}: Checking {slnx}");
			if (File.Exists(slnx))
			{
				Console.WriteLine($"[DEBUG] Solution file found at: {dir}");
				foundRoot = dir;
				break;
			}
			var parent = Directory.GetParent(dir);
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
			foreach (var d in checkedDirs) Console.WriteLine($"  {d}");
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
			Path.GetFileNameWithoutExtension(proj);
			var dir = Path.GetFileName(Path.GetDirectoryName(proj)!);
			// Skip Api project as it has been removed from the solution
			if (dir.Equals("Api", StringComparison.OrdinalIgnoreCase))
				continue;
			(referenced.Contains(proj) || entryPoints.Contains(dir)).Should().BeTrue($"Project {proj} is unused and not an entry point");
		}
	}

	[Fact]
	public void SharedNugetPackages_ShouldHaveConsistentVersions()
	{
		Console.WriteLine($"[DEBUG] SharedNugetPackages_ShouldHaveConsistentVersions: _srcPath={_srcPath}");
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
		Console.WriteLine($"[DEBUG] TestProjects_ShouldNotReferenceEachOther: _testsPath={_testsPath}");
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
		Console.WriteLine($"[DEBUG] Web_ShouldNotReferenceAspireHostingPackages: _srcPath={_srcPath}");
		// Arrange
		var webCsproj = Path.Combine(_srcPath, "Web", "Web.csproj");
		var content = File.ReadAllText(webCsproj);
		// Act & Assert
		content.Should().NotContain("Aspire.Hosting", "Web project should not reference Aspire.Hosting packages");
	}

	[Fact]
	public void OutputType_ShouldBeExeOnlyForEntryPoints()
	{
		Console.WriteLine($"[DEBUG] OutputType_ShouldBeExeOnlyForEntryPoints: _srcPath={_srcPath}");
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
		Console.WriteLine($"[DEBUG] EachProject_ShouldContainGlobalUsingsFile: _srcPath={_srcPath}");
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