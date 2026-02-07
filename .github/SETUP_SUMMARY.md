# Setup and Debugging Summary

## Overview

This document provides a comprehensive overview of setting up and debugging .NET solutions scaffolded by the solution-scaffolder skill. It serves as a quick reference for developers getting started with a new project.

## Quick Start

### Prerequisites

- **.NET 10 SDK** (latest stable version)
- **Docker** (for running TestContainers in tests)
- **MongoDB** (optional, if not using Docker)
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Git** for version control

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/yourorg/yourrepo.git
cd yourrepo

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run tests to verify setup
dotnet test

# Run the application (if applicable)
dotnet run --project src/YourSolution.Web/
```

## Project Structure

```
YourSolution/
├── .github/                         # GitHub configurations and workflows
│   ├── workflows/                   # CI/CD pipelines
│   ├── agents/                      # Custom Copilot agents
│   ├── prompts/                     # Copilot prompt templates
│   ├── skills/                      # Custom Copilot skills
│   ├── instructions/                # Copilot coding instructions
│   ├── test-debugging-unit.md       # Unit testing guide
│   ├── test-debugging-integration.md # Integration testing guide
│   ├── test-debugging-e2e.md        # E2E testing guide
│   └── SETUP_SUMMARY.md             # This file
├── src/                             # Source code
│   ├── YourSolution.Api/            # API entry point
│   ├── YourSolution.Web/            # Blazor web UI
│   ├── YourSolution.Features/       # Feature slices (Vertical Slice Architecture)
│   ├── YourSolution.Domain/         # Domain models and abstractions
│   ├── YourSolution.Persistence/    # Data access layer
│   ├── YourSolution.Common/         # Shared utilities
│   ├── YourSolution.AppHost/        # Aspire orchestration (if applicable)
│   └── YourSolution.ServiceDefaults/ # Shared service configurations
├── tests/                           # All tests
│   ├── YourSolution.Tests.Unit/     # Unit tests (xUnit + FluentAssertions + NSubstitute)
│   ├── YourSolution.Tests.Integration/ # Integration tests (TestContainers)
│   ├── YourSolution.Tests.Architecture/ # Architecture tests (NetArchTest)
│   ├── YourSolution.Tests.Bunit/    # Blazor component tests (bUnit)
│   └── YourSolution.Tests.E2E/      # End-to-end tests (Playwright)
├── docs/                            # Documentation
│   ├── ARCHITECTURE.md              # Architecture overview
│   └── README.md                    # Project documentation
├── scripts/                         # Utility scripts
├── .editorconfig                    # Code style rules
├── .gitignore                       # Git ignore patterns
├── Directory.Build.props            # Centralized build properties
├── Directory.Packages.props         # Centralized NuGet package versions
├── global.json                      # .NET SDK version lock
├── README.md                        # Getting started guide
├── CONTRIBUTING.md                  # Contribution guidelines
└── YourSolution.sln                 # Solution file
```

## Testing Overview

### Test Types

| Test Type | Purpose | Framework | Run Time |
|-----------|---------|-----------|----------|
| **Unit** | Test individual components in isolation | xUnit, FluentAssertions, NSubstitute | Fast (~ms) |
| **Integration** | Test components working together with real dependencies | xUnit, TestContainers, WebApplicationFactory | Medium (~seconds) |
| **Architecture** | Enforce architectural rules and dependencies | NetArchTest | Fast (~ms) |
| **bUnit** | Test Blazor components | bUnit, xUnit | Fast (~ms) |
| **E2E** | Test complete user workflows | Playwright, xUnit | Slow (~seconds to minutes) |

### Running Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/YourSolution.Tests.Unit/

# Run only integration tests
dotnet test tests/YourSolution.Tests.Integration/

# Run only E2E tests
dotnet test tests/YourSolution.Tests.E2E/

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test
dotnet test --filter "FullyQualifiedName~CreateArticleHandlerTests.Handle_ValidCommand_CreatesArticle"

# Run tests in category
dotnet test --filter "Category=Smoke"
```

### Test Documentation

Detailed testing guides are available:
- **[Unit Testing Guide](.github/test-debugging-unit.md)** - Writing and debugging unit tests
- **[Integration Testing Guide](.github/test-debugging-integration.md)** - Integration tests with TestContainers
- **[E2E Testing Guide](.github/test-debugging-e2e.md)** - End-to-end tests with Playwright

## Debugging

### Visual Studio

#### Debug Application

1. Set startup project (right-click project → Set as Startup Project)
2. Press **F5** to start debugging
3. Set breakpoints by clicking in the left margin
4. Use debug windows:
   - **Locals** - View local variables
   - **Watch** - Monitor specific expressions
   - **Call Stack** - View execution path
   - **Immediate** - Execute code while debugging

#### Debug Unit Tests

1. Open **Test Explorer** (Test → Test Explorer)
2. Right-click a test → **Debug Selected Tests**
3. Breakpoints in test or application code will be hit

#### Debug Integration/E2E Tests

1. Set breakpoints in test code
2. Right-click test in Test Explorer → **Debug Selected Tests**
3. For TestContainers, tests may take longer to start (container initialization)

### Visual Studio Code

#### Debug Configuration

Create `.vscode/launch.json`:

```json
{
	"version": "0.2.0",
	"configurations": [
		{
			"name": ".NET Core Launch (web)",
			"type": "coreclr",
			"request": "launch",
			"preLaunchTask": "build",
			"program": "${workspaceFolder}/src/YourSolution.Web/bin/Debug/net10.0/YourSolution.Web.dll",
			"args": [],
			"cwd": "${workspaceFolder}/src/YourSolution.Web",
			"stopAtEntry": false,
			"serverReadyAction": {
				"action": "openExternally",
				"pattern": "\\bNow listening on:\\s+(https?://\\S+)"
			},
			"env": {
				"ASPNETCORE_ENVIRONMENT": "Development"
			}
		},
		{
			"name": ".NET Core Test",
			"type": "coreclr",
			"request": "launch",
			"preLaunchTask": "build",
			"program": "dotnet",
			"args": [
				"test",
				"${workspaceFolder}/tests/YourSolution.Tests.Unit/YourSolution.Tests.Unit.csproj",
				"--filter",
				"FullyQualifiedName~CreateArticleHandlerTests"
			],
			"cwd": "${workspaceFolder}",
			"console": "internalConsole",
			"stopAtEntry": false
		}
	]
}
```

### Command Line Debugging

#### Enable Verbose Logging

```bash
# Set log level to Debug
export ASPNETCORE_ENVIRONMENT=Development
export Logging__LogLevel__Default=Debug

dotnet run --project src/YourSolution.Web/
```

#### Use dotnet-trace

```bash
# Install dotnet-trace
dotnet tool install --global dotnet-trace

# Collect trace
dotnet-trace collect --process-id <PID> --providers Microsoft-Extensions-Logging

# Analyze trace in Visual Studio or PerfView
```

## Common Issues and Solutions

### Issue: Build Fails with Package Restore Errors

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

### Issue: Tests Fail Due to Missing Docker

**Cause:** Integration/E2E tests use TestContainers which require Docker  
**Solution:**
```bash
# Install Docker Desktop
# Start Docker
sudo systemctl start docker  # Linux
# Or start Docker Desktop GUI on Windows/Mac

# Verify Docker is running
docker ps
```

### Issue: MongoDB Connection Errors in Tests

**Cause:** TestContainers not starting MongoDB properly  
**Solution:**
```bash
# Pull MongoDB image manually
docker pull mongo:7.0

# Verify image exists
docker images | grep mongo

# Run tests again
dotnet test tests/YourSolution.Tests.Integration/
```

### Issue: Port Already in Use

**Cause:** Previous instance still running  
**Solution:**
```bash
# Find process using port
lsof -i :5000  # macOS/Linux
netstat -ano | findstr :5000  # Windows

# Kill process
kill -9 <PID>  # macOS/Linux
taskkill /PID <PID> /F  # Windows
```

### Issue: Hot Reload Not Working

**Solution:**
```bash
# Use dotnet watch for hot reload
dotnet watch run --project src/YourSolution.Web/

# Or in Visual Studio, enable Hot Reload (Tools → Options → Debugging → .NET Hot Reload)
```

### Issue: Blazor Component Not Updating

**Cause:** State change not triggering re-render  
**Solution:**
```csharp
// Call StateHasChanged() after state changes
private async Task UpdateData()
{
	await LoadDataAsync();
	StateHasChanged(); // Force re-render
}
```

### Issue: Authentication Not Working Locally

**Solution:**
```bash
# Add Auth0 credentials to user secrets
dotnet user-secrets set "Auth0:Domain" "your-domain.auth0.com"
dotnet user-secrets set "Auth0:ClientId" "your-client-id"
dotnet user-secrets set "Auth0:ClientSecret" "your-client-secret"

# Verify secrets
dotnet user-secrets list --project src/YourSolution.Web/
```

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/add-article-comments
```

### 2. Implement Feature Using Vertical Slice

```
src/YourSolution.Features/Comments/
├── CreateComment/
│   ├── CreateCommentCommand.cs
│   ├── CreateCommentHandler.cs
│   ├── CreateCommentValidator.cs
│   └── CreateCommentPage.razor
└── GetComments/
    ├── GetCommentsQuery.cs
    ├── GetCommentsHandler.cs
    └── CommentsListComponent.razor
```

### 3. Write Tests

```bash
# Unit tests
tests/YourSolution.Tests.Unit/Features/Comments/
├── CreateCommentHandlerTests.cs
└── CreateCommentValidatorTests.cs

# Integration tests
tests/YourSolution.Tests.Integration/Features/Comments/
└── CreateCommentEndpointTests.cs

# bUnit tests (if Blazor UI)
tests/YourSolution.Tests.Bunit/Features/Comments/
└── CreateCommentPageTests.cs
```

### 4. Run Tests Locally

```bash
dotnet test
```

### 5. Commit and Push

```bash
git add .
git commit -m "feat: add article comments feature"
git push origin feature/add-article-comments
```

### 6. Create Pull Request

- CI/CD pipeline runs automatically
- All tests must pass
- Code coverage must meet threshold (typically 80%+)
- Architecture tests must pass

## CI/CD Pipeline

### GitHub Actions Workflow

The solution includes pre-configured workflows:

**build-and-test.yml** - Main CI pipeline:
- Runs on every push and PR
- Executes: restore → build → unit tests → integration tests
- Uploads test results and coverage reports

**code-quality.yml** - Code quality checks:
- Runs linters and code analyzers
- Checks for security vulnerabilities
- Enforces code style (.editorconfig)

### Viewing CI Results

1. Go to **Actions** tab on GitHub
2. Click on latest workflow run
3. View test results, logs, and artifacts
4. Download coverage reports if needed

## Code Quality Tools

### Linting and Formatting

```bash
# Format code (if dotnet-format installed)
dotnet format

# Analyze code
dotnet build /p:RunAnalyzers=true
```

### Architecture Testing

```bash
# Run architecture tests
dotnet test tests/YourSolution.Tests.Architecture/

# These tests enforce:
# - Dependency rules (e.g., Domain doesn't reference Persistence)
# - Naming conventions
# - Layer isolation
# - Clean architecture principles
```

## Environment Configuration

### Development

Uses `appsettings.Development.json` and user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:MongoDB" "mongodb://localhost:27017"
dotnet user-secrets set "Auth0:Domain" "dev.auth0.com"
```

### Staging/Production

Uses environment variables or Azure Key Vault:

```bash
export ConnectionStrings__MongoDB="mongodb://prod-server:27017"
export Auth0__Domain="prod.auth0.com"
```

## Performance Profiling

### Using dotnet-counters

```bash
# Install dotnet-counters
dotnet tool install --global dotnet-counters

# Monitor performance
dotnet-counters monitor --process-id <PID> System.Runtime Microsoft.AspNetCore.Hosting
```

### Using Application Insights

If configured, view telemetry in Azure Portal:
- Response times
- Dependency calls
- Exception tracking
- Custom metrics

## Additional Resources

### Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [Contributing Guidelines](CONTRIBUTING.md)
- [Unit Testing Guide](.github/test-debugging-unit.md)
- [Integration Testing Guide](.github/test-debugging-integration.md)
- [E2E Testing Guide](.github/test-debugging-e2e.md)

### External Resources

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Vertical Slice Architecture](https://jimmybogard.com/vertical-slice-architecture/)
- [xUnit Documentation](https://xunit.net/)
- [TestContainers Documentation](https://dotnet.testcontainers.org/)
- [Playwright Documentation](https://playwright.dev/dotnet/)

## Getting Help

- **GitHub Issues**: Report bugs or request features
- **Discussions**: Ask questions and share ideas
- **Pull Requests**: Contribute code improvements
- **Documentation**: Check docs/ folder for detailed guides

---

**Last Updated:** February 2026  
**Version:** 1.0
