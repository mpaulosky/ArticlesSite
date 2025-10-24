# ArticlesSite

[![GitHub](https://img.shields.io/github/license/mpaulosky/ArticlesSite?style=flat-square&logo=github)](LICENSE.txt)
[![.NET Build](https://img.shields.io/github/actions/workflow/status/mpaulosky/ArticlesSite/dotnet.yml?style=flat-square&label=Build)](https://github.com/mpaulosky/ArticlesSite/actions/workflows/dotnet.yml)
[![CodeCov](https://img.shields.io/codecov/c/github/mpaulosky/ArticlesSite/main?style=flat-square&logo=codecov)](https://codecov.io/gh/mpaulosky/ArticlesSite)
![.NET version](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)

⭐ If you like this project, star it on GitHub — it helps a lot!

[Overview](#overview) • [Features](#features) • [Getting Started](#getting-started) • [Architecture](#architecture) • [Testing](#testing)

---

A modern, cloud-native blog management application built with **BlazorServer**, **.NET Aspire**, and **MongoDB**. This project demonstrates best practices in serverless architecture, comprehensive testing strategies, and production-ready patterns for building scalable web applications with .NET 9.

> [!TIP]
> This application is designed to showcase enterprise-grade patterns including CQRS, Vertical Slice Architecture, and comprehensive test coverage with unit, integration, and end-to-end tests.

## Overview

ArticlesSite is a full-featured blog management system that enables users to create, manage, and publish articles with categories and rich content. Built on .NET Aspire's cloud-native orchestration, the application leverages modern serverless patterns to deliver a scalable, maintainable solution.

The application uses **Blazor Server** for interactive server-side rendering, providing a responsive user experience without the complexity of client-side JavaScript frameworks. MongoDB serves as the primary data store, providing flexible document-based storage for articles and categories.

### Key Technologies

- **.NET 9** with **C# 13** - Latest .NET platform features
- **.NET Aspire** - Cloud-native orchestration and service defaults
- **Blazor Server** - Interactive server-side rendering
- **MongoDB 8.0** - Document database with flexible schema
- **xUnit** - Unit and integration testing framework
- **bUnit** - Blazor component testing
- **TestContainers** - Isolated integration test environments

## Features

- **Article Management** - Create, edit, and publish blog posts with rich content
- **Category Organization** - Organize articles with hierarchical categories
- **Interactive UI** - Blazor Server components with real-time updates
- **Cloud-Native Architecture** - Built with .NET Aspire for container orchestration
- **Persistent Storage** - MongoDB container with persistent data volumes
- **Health Checks** - Built-in monitoring and diagnostics
- **OpenTelemetry Integration** - Distributed tracing and metrics
- **Comprehensive Testing** - Unit, integration, and E2E test coverage
- **Security First** - HTTPS, authentication, CORS, and secure headers by default

## Getting Started

### Prerequisites

To run this application, you'll need:

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker** - Required for MongoDB container and TestContainers
- **Visual Studio 2022 / Rider / VS Code** - Recommended for development

### Running the Application

1. **Clone the repository**

   ```bash
   git clone https://github.com/mpaulosky/ArticlesSite.git
   cd ArticlesSite
   ```

2. **Start the application**

   ```bash
   dotnet run --project src/AppHost
   ```

   The AppHost will orchestrate all services including MongoDB container and Redis cache.

3. **Access the application**

   Once started, navigate to the URL displayed in the console output (typically `http://localhost:5000`).

### Running Tests

Execute the full test suite with:

```bash
dotnet test
```

This runs:
- **Unit tests** - Fast, isolated tests for business logic
- **Integration tests** - Database and service integration tests using TestContainers
- **bUnit tests** - Blazor component tests
- **E2E tests** - End-to-end scenarios

## Architecture

ArticlesSite follows a clean, layered architecture with clear separation of concerns:

### Project Structure

```
ArticlesSite/
├── src/
│   ├── AppHost/              # .NET Aspire orchestration
│   ├── Web/                  # Blazor Server UI
│   ├── Api/                  # Web API endpoints
│   ├── ServiceDefaults/      # Shared service configuration
│   └── Shared/               # Domain models and contracts
└── tests/
    ├── Api.Tests.Unit/           # API unit tests
    ├── Api.Tests.Integration/    # API integration tests
    ├── Web.Tests.Unit/           # Web unit tests
    ├── Web.Tests.Integration/    # Web integration tests
    ├── Shared.Tests.Unit/        # Shared unit tests
    └── Architecture.Tests/       # Architecture tests
```

### Design Principles

- **SOLID Principles** - Maintainable, testable code structure
- **Dependency Injection** - Constructor injection throughout
- **CQRS Pattern** - Command/Query separation for clarity
- **Vertical Slice Architecture** - Feature-focused organization
- **Repository Pattern** - Clean data access abstraction
- **Service Defaults** - Consistent observability and resilience

### Database

MongoDB provides flexible document storage with:
- Persistent container storage for data durability
- Flexible document structure for articles and categories
- Native support for complex nested structures
- Full async/await support with MongoDB.Driver
- Integration with .NET Aspire for orchestration

The MongoDB container is configured with `ContainerLifetime.Persistent` to ensure data persists across application restarts.

## Testing

ArticlesSite employs a comprehensive testing strategy:

### Unit Tests
- Fast, isolated tests for business logic
- Test domain entities, helpers, and services
- Located in `tests/` directory

### Integration Tests
- Database integration with TestContainers
- Ensures MongoDB compatibility
- Isolated test data per test run

### Component Tests (bUnit)
- Blazor component behavior verification
- Render testing and interaction testing
- Example: `tests/ArticlesSite.Web.Tests.Bunit/`

### End-to-End Tests
- Full application scenarios
- Database and UI interaction
- Located in `tests/ArticlesSite.Tests.E2E/`

### Running Specific Tests

```bash
# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run with coverage
dotnet test /p:CollectCoverage=true
For end-to-end browser tests with Playwright, see docs/E2E_TESTS.md.
```

## Code Standards

This project enforces strict coding standards through `.editorconfig` and analysis rules:

- **File-scoped namespaces** and **global usings**
- **Nullable reference types** enabled
- **Async/await** for all I/O operations
- **Pattern matching** over traditional conditionals
- **Record types** for DTOs and value objects
- **XML documentation** for public APIs
- **Centralized package management** in `Directory.Packages.props`

See [`.github/instructions/copilot-instructions.md`](.github/instructions/copilot-instructions.md) for complete coding guidelines.

## Deployment

The application is designed for deployment to Azure or any container orchestration platform:

1. **Container Build** - Dockerfiles included for all services
2. **.NET Aspire Manifest** - Deploy to Azure Container Apps
3. **MongoDB** - Use Azure Cosmos DB for MongoDB API or containerized deployment
4. **Observability** - Built-in OpenTelemetry for Application Insights

## Dev Container detection

When running the solution inside a Dev Container (Rider or VS Code), this repository sets explicit environment variables from .devcontainer/devcontainer.json:

- RIDER_DEVCONTAINER=true
- IN_DEVCONTAINER=true

You can detect this in code via the helper ArticlesSite.Shared.Helpers.RuntimeEnvironment:

- RuntimeEnvironment.IsRunningInDevContainer() -> true when in a Dev Container
- RuntimeEnvironment.IsRunningInContainer() -> true when in any container

The Web app logs a startup message indicating whether it is running inside a Dev Container.

## Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Blazor Server Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [MongoDB .NET Driver](https://www.mongodb.com/docs/drivers/csharp/)
- [MongoDB Documentation](https://www.mongodb.com/docs/)
- [TestContainers for .NET](https://dotnet.testcontainers.org/)

## Contributing

Contributions are welcome! This project follows standard open-source contribution guidelines. Please ensure all tests pass and code follows the established standards before submitting a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
