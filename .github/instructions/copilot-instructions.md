# Copilot Instructions

**Last updated:** June 12, 2025

These instructions define the required coding, architecture, and project rules for all .NET code in this repository.
They are based on the actual practices and conventions in the Article ServiceApp solution. For more details,
see [CONTRIBUTING.md](../docs/CONTRIBUTING.md).

---

## Tone

- Be friendly and patient:** We're all here first and foremost to help each other.
- Be very descriptive in how you present a solution
- Use lots of emoting in your responses
- ASCII art... lots of ascii art when called for

## C# (Required)

### Style

- **Use .editorconfig:** `true`
- **Preferred Modifier Order:** `public`, `private`, `protected`, `internal`, `static`, `readonly`, `const`
  - _Example:_
    ```csharp
    public static readonly int MY_CONST = 42;
    ```
- **Use Explicit Type:** `true` (except where `var` improves clarity)
- **Use Var:** `true` (when the type is obvious)
- **Prefer Null Check:**
  - Use `is null`: `true`
  - Use `is not null`: `true`
- **Prefer Primary Constructors:** `false`
- **Prefer Records:** `true`
- **Prefer Minimal APIs:** `true`
- **Prefer File Scoped Namespaces:** `true`
- **Use Global Usings:** `true` (see `GlobalUsings.cs` in each project)
- **Use Nullable Reference Types:** `true`
- **Use Pattern Matching:** `true`
- **Max Line Length:** `120`
- **Indent Style:** `tab`
- **Indent Size:** `2`
- **End of Line:** `lf`
- **Trim Trailing Whitespace:** `true`
- **Insert Final Newline:** `true`
- **Charset:** `utf-8`

### Naming

- **Interface Prefix:** `I` (e.g., `IService`)
- **Async Suffix:** `Async` (e.g., `GetDataAsync`)
- **Private Field Prefix:** `_` (e.g., `_myField`)
- **Constant Case:** `UPPER_CASE` (e.g., `MAX_SIZE`)
- **Component Suffix:** `Component` (for Blazor components)
- **Blazor Page Suffix:** `Page` (for Blazor pages)

### Security (Required)

- **Require HTTPS:** `true` (see `Web/Program.cs`)
- **Require Authentication:** `true` (Auth0 integration)
- **Use Auth0 Identity Provider:** `true` (configure Domain, ClientId, ClientSecret in appsettings.json)
- **Use Auth0 .NET SDK:** `true` (AddAuth0WebAppAuthentication() in Program.cs)
- **Use Cookie Authentication:** `true` (for Blazor Server session management)
- **Protect API Endpoints:** `true` (use RequireAuthorization() for minimal APIs)
- **Implement Auth0 Roles:** `true` (use Auth0 roles and permissions for authorization)
- **Secure Configuration:** `true` (store Auth0 ClientSecret in user secrets, not appsettings.json)
- **Configure Auth0 Callbacks:** `true` (set proper callback URLs in Auth0 dashboard)
- **Implement Logout:** `true` (clear both local session and Auth0 session)
- **Require Authorization:** `true`
- **Use Antiforgery Tokens:** `true` (see `Web/Program.cs`)
- **Use CORS:** `true`
- **Use Secure Headers:** `true`

### Architecture (Required)

- **Enforce SOLID:** `true` (see `Domain/`, `ServiceDefaults/`)
- **Enforce Dependency Injection:** `true` (see `Web/Program.cs`, `ServiceDefaults/`)
- **Enforce Async/Await:** `true` (async methods and tests)
- **Enforce Strongly Typed Config:** `true`
- **Enforce CQRS:** `true` (see `Domain/Abstractions/`, `MyMediator/`)
- **Enforce Unit Tests:** `true` (see `tests/`)
- **Enforce Integration Tests:** `true` (see `tests/`)
- **Enforce Architecture Tests:** `true` (see `tests/Architecture.Tests.Unit/`)
- **Enforce Vertical Slice Architecture:** `true`
- **Enforce Aspire:** `true` (see `AppHost/`, `README.md`)
- **Centralize NuGet Package Versions:** `true` (all package versions must be managed in `Directory.Packages.props` at
  the repo root; do not specify versions in individual project files)

### Blazor (Required)

- **Enforce State Management:** `true` (see use of `@code` blocks and parameters)
- **Use Interactive Server Rendering:** `true` (see `Web/Program.cs`)
- **Use Stream Rendering:** `true`
- **Enforce Component Lifecycle:** `true` (see `OnInitialized`, `OnParametersSet` in components)
- **Use Cascading Parameters:** `true` (see shared layout/components)
- **Use Render Fragments:** `true` (see component parameters)
- **Use Virtualization:** `true` (see use of `Web.Virtualization`)
- **Use Error Boundaries:** `true` (see `MainLayout.razor` error UI)
- **Component Suffix:** `Component` (e.g., `FooterComponent`)
- **Page Suffix:** `Page` (e.g., `AboutPage`)

### Documentation (Required)

- **Require XML Docs:** `true` (see `<summary>` in test and code files)
- **Require Swagger:** `true` (for REST APIs)
- **Require OpenAPI:** `true` (OpenAPI/Swagger must be provided for all APIs)
- **Require Component Documentation:** `true` (see `<summary>` in Blazor tests)
- **Require README:** `true` (see `README.md`, `docs/README.md`)
- **Require CONTRIBUTING.md:** `true` (see `docs/CONTRIBUTING.md`)
- **Require LICENSE:** `true` (see `LICENSE`)
- **Require Code of Conduct:** `true` (see `CODE_OF_CONDUCT.md`)
- **Require File Copyright Headers:** `true`
- **Use Copyright Header Format:** `true` (see below for format)

```
=======================================================
Copyright (c) ${File.CreatedYear}. All rights reserved.
File Name :     ${File.FileName}
Company :       mpaulosky
Author :        Matthew Paulosky
Solution Name : ${File.SolutionName}
Project Name :  ${File.ProjectName}
=======================================================
```

### Logging (Required)

- **Require Structured Logging:** `true`
- **Require Health Checks:** `true`
- **Use OpenTelemetry:** `true`
- **Use Application Insights:** `true`

### Database (Required)

- **Use Entity Framework Core:** `true`
- **Use MongoDB:** `true` (see `Persistence.MongoDb/`)
- **Prefer Async Operations:** `true`
- **Use Migrations:** `false` (for MongoDB)
- **Use TestContainers:** `true` (for Integration testing, see
  `tests/Article Service.Persistence.MongoDb.Tests.Integration/`)
- **Use Change Tracking:** `true`
- **Use DbContext Pooling:** `true`
- **Use In-Memory Database:** `false`

### Auth0 Integration (Required)

- **Configure Auth0 Application:** `true` (Regular Web Application type in Auth0 dashboard)
- **Use Auth0 .NET SDK:** `true` (Auth0.AspNetCore.Authentication package)
- **Configure Dependency Injection:** `true` (AddAuth0WebAppAuthentication() in Program.cs)
- **Set Callback URLs:** `true` (configure in Auth0 dashboard to match application URLs)
- **Implement User Management:** `true` (access user claims via ClaimsPrincipal)
- **Use Auth0 Roles and Permissions:** `true` (implement role-based authorization)
- **Handle Authentication Errors:** `true` (implement proper error handling for auth failures)
- **Secure API Endpoints:** `true` (use [Authorize] attributes and RequireAuthorization())
- **Test Authentication:** `true` (include auth scenarios in integration tests)

### Versioning (Required)

- **Require API Versioning:** `true`
- **Use Semantic Versioning:** `true`

### Caching (Required)

- **Require Caching Strategy:** `true`
- **Use Distributed Cache:** `true`
- **Use Output Caching:** `true` (see `Web/Program.cs`)

### Middleware (Required)

- **Require Cross-Cutting Concerns:** `true`
- **Use Exception Handling:** `true` (see `Web/Program.cs`)
- **Use Request Logging:** `true`

### Background Services (Required)

- **Require Background Service:** `true`

### Environment (Required)

- **Require Environment Config:** `true`
- **Use User Secrets:** `true` (especially for Auth0 ClientSecret and sensitive configuration)
- **Configure Auth0 Settings:** `true` (Domain, ClientId in appsettings.json; ClientSecret in user secrets)
- **Use Key Vault:** `true`

### Validation (Required)

- **Require Model Validation:** `true`
- **Use FluentValidation:** `true`

### Testing (Required)

- **Require Unit Tests:** `true` (see `tests/`)
- **Require Integration Tests:** `true` (see `tests/`)
- **Require Architecture Tests:** `true` (see `tests/Architecture.Tests.Unit/`)
- **Use xUnit:** `true` (see `tests/`)
- **Use FluentAssertions:** `true` (see `tests/`)
- **Use NSubstitute:** `true`
- **Use Xunit:** `true` (see `tests/Shared.Tests.Unit/`)
- **Use bUnit:** `true` (see `tests/Web.Tests.Unit/`)
- **Use Playwright:** `true` (see `README.md`)

---

**Note:** These rules are enforced via `.editorconfig and other tooling where possible. For questions or
clarifications, see [CONTRIBUTING.md](../docs/CONTRIBUTING.md).
