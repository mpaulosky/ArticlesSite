
# References Used In ArticleSite

## Technologies & Frameworks

- [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) – Main platform for all projects
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) – Cloud-native orchestration (see `.aspire/` folder)
- [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor/) – UI framework for `src/Web`
- [MongoDB](https://www.mongodb.com/docs/) – NoSQL database for persistence
- [MongoDB .NET Driver](https://www.mongodb.com/docs/drivers/csharp/) – Used in API and Shared projects
- [MongoDB EF Core Provider](https://mongodb.github.io/mongo-efcore-provider/) – Optional, for EF Core integration

## Testing Tools

- [xUnit](https://xunit.net/) – Unit and integration tests (`tests/`)
- [bUnit](https://bunit.dev/) – Blazor component testing (`tests/Web.Tests.Unit`)
- [TestContainers for .NET](https://dotnet.testcontainers.org/) – Integration test containers (`tests/Api.Tests.Integration`)
- [Playwright for .NET](https://playwright.dev/dotnet/) – E2E browser testing (`tests/Web.Tests.Integration`)
- [FluentAssertions](https://fluentassertions.com/) – Assertion library for all tests
- [NSubstitute](https://nsubstitute.github.io/) – Mocking for unit tests
- [Bogus](https://github.com/bchavez/Bogus) – Fake data generation for tests

## Package Management

- [Central Package Management (CPM)](https://learn.microsoft.com/nuget/consume-packages/central-package-management) – Centralized NuGet versioning (`Directory.Packages.props`)

## Workflows & Actions

- [GitHub Actions](https://github.com/features/actions) – CI/CD for build, test, and deploy (`.github/workflows/dotnet.yml`)
- [Codecov](https://codecov.io/) – Code coverage tracking (`codecov.yml`)
- [Test coverage report](https://github.com/marketplace/actions/test-coverage-report) – Generate coverage reports
- [Publish Test Results](https://github.com/marketplace/actions/publish-test-results) – Publish test results to GitHub
- [ToDo to Issue](https://github.com/marketplace/actions/todo-to-issue) – Convert TODO comments to issues

## Development Tools

- [Visual Studio 2022](https://visualstudio.microsoft.com/) – Primary IDE
- [JetBrains Rider](https://www.jetbrains.com/rider/) – Alternative IDE
- [Visual Studio Code](https://code.visualstudio.com/) – Lightweight editor, cross-platform

## Architecture & Patterns

- [CQRS Pattern](https://learn.microsoft.com/azure/architecture/patterns/cqrs) – Command Query Responsibility Segregation
- [Repository Pattern](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design) – Data access abstraction
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/) – Feature-focused organization
