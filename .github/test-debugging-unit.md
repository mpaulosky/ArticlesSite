# Unit Testing Guide

## Overview

This guide provides comprehensive instructions for writing, running, and debugging unit tests in solutions scaffolded by the solution-scaffolder skill.

## Test Framework

All unit tests use **xUnit** with **FluentAssertions** for readable assertions and **NSubstitute** for mocking.

## Project Structure

Unit tests are organized by feature in the `tests/{SolutionName}.Tests.Unit/` project:

```
tests/
└── YourSolution.Tests.Unit/
    ├── Features/
    │   ├── Articles/
    │   │   ├── CreateArticleHandlerTests.cs
    │   │   ├── CreateArticleValidatorTests.cs
    │   │   └── GetArticlesHandlerTests.cs
    │   └── Comments/
    │       ├── CreateCommentHandlerTests.cs
    │       └── GetCommentsHandlerTests.cs
    └── GlobalUsings.cs
```

## Writing Unit Tests

### Test Class Structure

```csharp
namespace YourSolution.Tests.Unit.Features.Articles;

/// <summary>
/// Unit tests for CreateArticleHandler
/// </summary>
public sealed class CreateArticleHandlerTests
{
	private readonly IArticleRepository _mockRepository;
	private readonly IValidator<CreateArticleCommand> _mockValidator;
	private readonly CreateArticleHandler _handler;

	public CreateArticleHandlerTests()
	{
		_mockRepository = Substitute.For<IArticleRepository>();
		_mockValidator = Substitute.For<IValidator<CreateArticleCommand>>();
		_handler = new CreateArticleHandler(_mockRepository, _mockValidator);
	}

	[Fact]
	public async Task Handle_ValidCommand_CreatesArticle()
	{
		// Arrange
		var command = new CreateArticleCommand("Test Title", "Test Content");
		_mockValidator.ValidateAsync(command, default).Returns(new ValidationResult());
		
		// Act
		var result = await _handler.Handle(command, CancellationToken.None);
		
		// Assert
		result.Should().NotBeNull();
		result.IsSuccess.Should().BeTrue();
		await _mockRepository.Received(1).AddAsync(Arg.Any<Article>(), Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Handle_InvalidCommand_ReturnsValidationErrors()
	{
		// Arrange
		var command = new CreateArticleCommand("", "");
		var validationFailures = new List<ValidationFailure>
		{
			new("Title", "Title is required")
		};
		_mockValidator.ValidateAsync(command, default)
			.Returns(new ValidationResult(validationFailures));
		
		// Act
		var result = await _handler.Handle(command, CancellationToken.None);
		
		// Assert
		result.Should().NotBeNull();
		result.IsSuccess.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.Message.Contains("Title is required"));
	}
}
```

### Naming Conventions

- **Test Class**: `{ClassUnderTest}Tests.cs`
- **Test Method**: `{MethodName}_{Scenario}_{ExpectedResult}`
- Examples:
  - `Handle_ValidCommand_CreatesArticle`
  - `Validate_EmptyTitle_ReturnsValidationError`
  - `GetById_NonExistentId_ReturnsNotFound`

### Arrange-Act-Assert Pattern

Always structure tests using the AAA pattern:

```csharp
[Fact]
public async Task ExampleTest()
{
	// Arrange - Set up test data and dependencies
	var dependency = Substitute.For<IDependency>();
	var sut = new SystemUnderTest(dependency);
	
	// Act - Execute the method under test
	var result = await sut.DoSomethingAsync();
	
	// Assert - Verify expected behavior
	result.Should().NotBeNull();
	result.Value.Should().Be(42);
}
```

## Running Tests

### Command Line

```bash
# Run all unit tests
dotnet test tests/YourSolution.Tests.Unit/

# Run tests with detailed output
dotnet test tests/YourSolution.Tests.Unit/ --logger "console;verbosity=detailed"

# Run tests with code coverage
dotnet test tests/YourSolution.Tests.Unit/ --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateArticleHandlerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~CreateArticleHandlerTests.Handle_ValidCommand_CreatesArticle"
```

### Visual Studio

1. Open **Test Explorer** (Test → Test Explorer)
2. Click **Run All** to run all unit tests
3. Right-click a test class or method to run specific tests
4. Use **Debug Selected Tests** for debugging

### Visual Studio Code

1. Install the **.NET Core Test Explorer** extension
2. Tests appear in the Test Explorer sidebar
3. Click the play button next to tests to run them
4. Click the debug icon to debug tests

## Debugging Unit Tests

### Visual Studio

1. Set breakpoints in your test method or the code under test
2. Right-click the test in Test Explorer
3. Select **Debug Selected Tests**
4. Use F10 (Step Over), F11 (Step Into), and F5 (Continue) to navigate

### Visual Studio Code

1. Set breakpoints in your test file
2. Add a debug configuration to `.vscode/launch.json`:

```json
{
	"version": "0.2.0",
	"configurations": [
		{
			"name": ".NET Core Test",
			"type": "coreclr",
			"request": "launch",
			"preLaunchTask": "build",
			"program": "dotnet",
			"args": [
				"test",
				"${workspaceFolder}/tests/YourSolution.Tests.Unit/YourSolution.Tests.Unit.csproj"
			],
			"cwd": "${workspaceFolder}",
			"console": "internalConsole",
			"stopAtEntry": false
		}
	]
}
```

3. Press F5 to start debugging

### Command Line Debugging

```bash
# Run tests with verbose output for troubleshooting
dotnet test tests/YourSolution.Tests.Unit/ -v detailed

# Run specific failing test to isolate issue
dotnet test --filter "FullyQualifiedName~FailingTestName" -v detailed
```

## Best Practices

### 1. Test One Thing Per Test

```csharp
// Good - Tests one scenario
[Fact]
public async Task Validate_EmptyTitle_ReturnsError()
{
	var validator = new CreateArticleValidator();
	var command = new CreateArticleCommand("", "Content");
	
	var result = await validator.ValidateAsync(command);
	
	result.IsValid.Should().BeFalse();
	result.Errors.Should().ContainSingle(e => e.PropertyName == "Title");
}

// Bad - Tests multiple scenarios
[Fact]
public async Task Validate_InvalidInput_ReturnsErrors()
{
	// Testing both empty title AND empty content in one test
}
```

### 2. Use Descriptive Test Names

```csharp
// Good
[Fact]
public async Task GetArticleById_ArticleExists_ReturnsArticle()

// Bad
[Fact]
public async Task Test1()
```

### 3. Avoid Test Interdependencies

Each test should run independently and not rely on other tests' state.

```csharp
// Good - Self-contained test
[Fact]
public async Task Handle_ValidCommand_CreatesArticle()
{
	var repository = Substitute.For<IArticleRepository>();
	var handler = new CreateArticleHandler(repository);
	// ... test implementation
}

// Bad - Relies on shared state
private static Article _sharedArticle;

[Fact]
public async Task Test1_CreateArticle()
{
	_sharedArticle = new Article(); // Sets shared state
}

[Fact]
public async Task Test2_UpdateArticle()
{
	// Depends on _sharedArticle from Test1
}
```

### 4. Mock External Dependencies

Always mock dependencies like repositories, HTTP clients, file systems, etc.

```csharp
[Fact]
public async Task Handle_ValidCommand_CallsRepository()
{
	// Mock the repository instead of using a real database
	var mockRepository = Substitute.For<IArticleRepository>();
	var handler = new CreateArticleHandler(mockRepository);
	
	await handler.Handle(new CreateArticleCommand("Title", "Content"), default);
	
	await mockRepository.Received(1).AddAsync(Arg.Any<Article>(), Arg.Any<CancellationToken>());
}
```

### 5. Use FluentAssertions

FluentAssertions provides readable, expressive assertions:

```csharp
// Good - Fluent and readable
result.Should().NotBeNull();
result.Value.Should().Be(42);
collection.Should().HaveCount(5);
exception.Should().BeOfType<ValidationException>();

// Less readable
Assert.NotNull(result);
Assert.Equal(42, result.Value);
Assert.Equal(5, collection.Count);
Assert.IsType<ValidationException>(exception);
```

## Common Issues and Solutions

### Issue: Test Fails Intermittently

**Cause**: Race conditions, timing issues, or shared state  
**Solution**: Ensure tests are isolated and don't depend on timing

```csharp
// Bad - Timing dependent
await Task.Delay(1000); // Don't use delays in tests

// Good - Wait for specific condition
await WaitForConditionAsync(() => result.IsComplete, TimeSpan.FromSeconds(5));
```

### Issue: Mock Not Working

**Cause**: Method not virtual or interface not properly configured  
**Solution**: Ensure mocked methods are virtual or from interfaces

```csharp
// NSubstitute requires virtual methods or interfaces
public interface IArticleRepository
{
	Task<Article> GetByIdAsync(Guid id); // Can be mocked
}

public class ConcreteRepository
{
	public Task<Article> GetByIdAsync(Guid id) { } // Cannot be mocked (not virtual)
	public virtual Task<Article> GetByIdAsync(Guid id) { } // Can be mocked
}
```

### Issue: Async Test Not Awaiting

**Cause**: Forgetting to await async methods  
**Solution**: Always await async methods and mark tests as async

```csharp
// Bad
[Fact]
public void Handle_ValidCommand_CreatesArticle()
{
	var result = handler.Handle(command, default); // Not awaited
}

// Good
[Fact]
public async Task Handle_ValidCommand_CreatesArticle()
{
	var result = await handler.Handle(command, default);
}
```

## Code Coverage

### Generating Coverage Reports

```bash
# Generate coverage data
dotnet test tests/YourSolution.Tests.Unit/ --collect:"XPlat Code Coverage"

# Install ReportGenerator tool (if not already installed)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  -reports:"tests/YourSolution.Tests.Unit/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:Html

# View report
open TestResults/CoverageReport/index.html
```

### Coverage Goals

- **Minimum**: 80% code coverage
- **Target**: 90% code coverage
- **Critical paths**: 100% coverage (authentication, authorization, payment processing)

## Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [.NET Testing Best Practices](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
