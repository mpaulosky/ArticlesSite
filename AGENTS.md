# AGENTS.md – AI Agent Guide for ArticlesSite

## Quick Start

```powershell
# Run the full application (requires Docker)
dotnet run --project src/AppHost

# Run all tests
dotnet test

# Filter by category
dotnet test --filter "FullyQualifiedName~Unit"
dotnet test --filter "FullyQualifiedName~Integration"
```

**Integration tests require Docker** – they spin up `mongo:8.0` via TestContainers automatically.
Web binds to `http://localhost:5057` (hardcoded in AppHost for Playwright compatibility).

---

## Architecture Overview

```
src/
  AppHost/          ← .NET Aspire orchestrator (entry point for running the app)
  ServiceDefaults/  ← Shared OpenTelemetry, health checks, resilience (AddServiceDefaults())
  Shared/           ← Cross-project contracts: Result<T>, constants, enums, helpers
  Web/              ← Blazor Server app (all features + data layer live here)
    Components/Features/   ← Vertical slices (Articles, Categories, AuthorInfo)
    Data/                  ← MongoDB context, repositories, seeder
    Services/              ← Auth0 provider, file storage, database seeder
    Infrastructure/        ← Concurrency policies (Polly), metrics publisher
tests/
  Architecture.Tests/      ← NetArchTest.Rules structural/naming enforcement
  Web.Tests.Unit/          ← xUnit + bUnit + NSubstitute
  Web.Tests.Integration/   ← TestContainers MongoDB (real driver, no mocks)
  Shared.Tests.Unit/
```

There is **no separate API project at runtime** – the Web Blazor app is the only service, backed directly by MongoDB and Redis (via Aspire).

---

## Vertical Slice Layout

Every domain feature under `src/Web/Components/Features/<Feature>/` follows this structure:

```
Articles/
  ArticlesList/       ← CQRS handler + Razor component in one folder
    GetArticles.cs    ← static outer class with nested IHandler + Handler
    ArticlesList.razor
  ArticleCreate/      ← same pattern (CreateArticle.cs + Create.razor)
  ArticleDetails/     ← GetArticle.cs + Details.razor
  ArticleEdit/        ← EditArticle.cs + Edit.razor
  Entities/Article.cs
  Extensions/ArticleMappingExtensions.cs  ← .ToDto() / .ToEntity() hand-coded mapping; no AutoMapper
  Fakes/FakeArticle.cs      ← Bogus entity factory, seed=621
  Fakes/FakeArticleDto.cs   ← Bogus DTO factory, same seed
  Interfaces/IArticleRepository.cs
  Models/ArticleDto.cs
  Models/ConcurrencyConflictResponseDto.cs  ← returned by PUT /api/articles/{id} on 409
  Validators/ArticleDtoValidator.cs  ← FluentValidation (DTO)
  Validators/ArticleValidator.cs     ← FluentValidation (entity)
```

Categories follows the same structure: `CategoriesList/`, `CategoryCreate/`, `CategoryDetails/`, `CategoryEdit/`.

Repositories are implemented at `src/Web/Data/Repositories/` (`ArticleRepository.cs`, `CategoryRepository.cs`) and registered via DI — the per-feature `Repositories/` subdirectory is intentionally empty.

### CQRS Handler Pattern

All handlers use a **static outer class with nested interface + implementation**:

```csharp
public static class GetArticles
{
    public interface IGetArticlesHandler { Task<Result<IEnumerable<ArticleDto>>> HandleAsync(...); }

    public class Handler(IArticleRepository repository, ILogger<Handler> logger) : IGetArticlesHandler
    { ... }
}
```

Handlers are registered in `src/Web/Data/MongoDbServiceExtensions.cs → RegisterRepositoriesAndHandlers()`. Add new handlers there when creating a new feature.

**Registered handlers (as of current codebase):**

| Handler | Feature slice |
|---|---|
| `GetArticles.Handler` | `ArticlesList/` |
| `GetArticle.Handler` | `ArticleDetails/` |
| `CreateArticle.Handler` | `ArticleCreate/` |
| `EditArticle.Handler` | `ArticleEdit/` – also receives `IAsyncPolicy<Result<Article>>` (concurrency) and `IMetricsPublisher` |
| `GetCategories.Handler` | `CategoriesList/` |
| `GetCategory.Handler` | `CategoryDetails/` |
| `CreateCategory.Handler` | `CategoryCreate/` |
| `EditCategory.Handler` | `CategoryEdit/` |

Validators (`ArticleDtoValidator`, `CategoryDtoValidator`, `ArticleValidator`, `CategoryValidator`) are also registered in the same method.

---

## Result Pattern

All operations return `Result<T>` from `Shared.Abstractions` – **not** exceptions or MediatR:

```csharp
// Success
return Result.Ok<IEnumerable<ArticleDto>>(dtos);

// Failure
return Result.Fail<IEnumerable<ArticleDto>>("message", ResultErrorCode.NotFound);

// Check
if (result.Failure) { /* result.Error, result.ErrorCode, result.Details */ }
```

`ResultErrorCode` values: `None`, `Concurrency`, `NotFound`, `Validation`, `Conflict`.

---

## MongoDB Access

- Uses **native MongoDB.Driver** (not EF Core) via `IMongoDbContextFactory → IMongoDbContext`.
- Context exposes `IMongoCollection<Article> Articles`, `IMongoCollection<Category> Categories`, and `IMongoDatabase Database`.
- Config keys (checked in order): `MongoDb:ConnectionString` → `ConnectionStrings:articlesdb` → env `MONGODB_CONNECTION_STRING`.
- Database name: `MongoDb:Database` → `MongoDb:DatabaseName` → env `MONGODB_DATABASE_NAME` → default `"articlesdb"`.
- In AppHost, MongoDB uses `ContainerLifetime.Persistent` with a named data volume (`mongo-server-data`), plus MongoExpress UI.

---

## Fake Data

Use Bogus-based factories in each feature's `Fakes/` folder for test data. These live in the `Web` project and are available in tests via GlobalUsings:

```csharp
Article single = FakeArticle.GetNewArticle(useSeed: true);   // deterministic
List<Article> list = FakeArticle.GetArticles(5, useSeed: true);

ArticleDto dto = FakeArticleDto.GetNewArticleDto(useSeed: true);
List<ArticleDto> dtos = FakeArticleDto.GetArticleDtos(5, useSeed: true);

// Category equivalents: FakeCategory / FakeCategoryDto with same API
```

Seed constant is `621`. Always prefer `useSeed: true` in unit tests for determinism.

---

## Integration Tests

Integration tests use `[Collection("MongoDb Collection")]` with `MongoDbFixture : IAsyncLifetime`, which starts a real `mongo:8.0` container via TestContainers. Always call `await _fixture.ClearCollectionsAsync()` at the start of each test to ensure isolation.

```csharp
[Collection("MongoDb Collection")]
public class MyRepositoryTests(MongoDbFixture fixture) { ... }
```

The `tests/Web.Tests.Integration/Api/` subfolder contains `ArticleApiEndToEndTests` and `ArticleApiConcurrencyTests` which test the `PUT /api/articles/{id}` minimal API endpoint using `WebApplicationFactory<Program>` wired to the TestContainers MongoDB instance.

---

## Minimal API Endpoints

Registered in `src/Web/Program.cs` alongside the Blazor app:

| Method | Route | Description |
|--------|-------|-------------|
| `PUT` | `/api/articles/{id}` | Update article; returns `ArticleDto` (200), `ConcurrencyConflictResponseDto` (409), or problem (400) |
| `GET` | `/api/files/{fileName}` | Serve uploaded images from `wwwroot/uploads/` |
| `GET` | `/Account/Login` | Auth0 challenge with `returnUrl` redirect |
| `GET` | `/Account/Logout` | Auth0 + cookie sign-out |

The `PUT /api/articles/{id}` endpoint delegates to `EditArticle.IEditArticleHandler`. On `ResultErrorCode.Concurrency` it returns a `ConcurrencyConflictResponseDto` that includes `ServerVersion`, `ServerArticle`, and `ChangedFields`.

---

## File Copyright Header (Required on every file)

```csharp
// =======================================================
// Copyright (c) 2025. All rights reserved.
// File Name :     FileName.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : ArticlesSite
// Project Name :  Web
// =======================================================
```

---

## NuGet Package Versions

All versions are centrally managed in `Directory.Packages.props` at the repo root. **Never specify a version in a `.csproj` file** – use `<PackageReference Include="..." />` only.

---

## Key Conventions

| Convention | Detail |
|---|---|
| File-scoped namespaces | Always (`namespace Web.Data;`) |
| Global usings | Each project has `GlobalUsings.cs` – add common usings there |
| Async suffix | All async methods end in `Async` |
| Private fields | Prefixed `_` (e.g., `_repository`) |
| Null checks | `is null` / `is not null`, not `== null` |
| Components | Suffix `Component`; pages suffix `Page` |
| Constants | `UPPER_CASE` |
| No primary constructors | Use regular constructors; handler classes may use record-style injection via `class Foo(IDep dep)` but not full primary constructor syntax for non-handlers |
| Auth | Auth0 via `Auth0.AspNetCore.Authentication`; user identity via `ClaimTypes.NameIdentifier` or `"sub"` claim |
| Concurrency | Polly retry policy (`ConcurrencyPolicies`) for optimistic concurrency on article updates; configured via `ConcurrencyOptions` in appsettings |
| Type alias | `IArticleConcurrencyPolicy` = `IAsyncPolicy<Result<Article>>` – defined in `Web/GlobalUsings.cs`; use this alias in DI and tests |
| Local storage | `Blazored.LocalStorage` (`ILocalStorageService`) registered via `builder.Services.AddBlazoredLocalStorage()` |
| Mapping | Hand-coded extension methods in `Extensions/` (e.g., `article.ToDto()`, `dto.ToEntity()`); no AutoMapper |

---

## Architecture Tests

`tests/Architecture.Tests/` uses `NetArchTest.Rules` to enforce naming, dependency direction, and structure. Run them before submitting any structural changes. The tests locate the solution root by walking up directories to find `ArticlesSite.slnx`.
