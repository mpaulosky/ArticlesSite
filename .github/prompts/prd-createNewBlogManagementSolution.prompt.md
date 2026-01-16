````prompt
---
agent: agent
description: "Generate a landing page"
---

# PRD Plan: Create New Bootstrap-Based Article Management Solution

Repository: ArticleManagementApp (new project, similar to mpaulosky/ArticlesSite)
Project focus: Greenfield .NET 10 Blazor Server + MongoDB article management application
Goal: Build a modern, cloud-native article management platform with Bootstrap styling (CDN via jsDelivr), Auth0 role-based authentication (Admin/Author/User), CQRS architecture, local file storage, and comprehensive testing—without E2E tests.

- Let's use a modern style with graident colors that are in dark blue hues, nice fonts, and plenty of emoji, and have nice hover effects throughout bringing it to life.
- The blog should have dark mode support using Bootstrap 5.3's built-in dark mode capabilities.
- Make sure we have nice headers for navigation and a footer as well with standard links and copyright GitHub build information.

## 1) Architecture Overview

**Tech Stack**:

- .NET 10 with C# 13
- Blazor Server (interactive server-side rendering)
- MongoDB 8.0 (document database)
- .NET Aspire (cloud-native orchestration & service defaults)
- Redis (caching & output cache)
- Auth0 (OAuth authentication)
- Bootstrap 5.3 (CSS framework)
- xUnit + bUnit + TestContainers (testing)

**Architectural Pattern**:

- SOLID principles with Dependency Injection
- CQRS pattern (Command/Query separation via handlers)
- Vertical Slice Architecture (features self-contained in subfolders)
- Repository pattern for data access
- Service Defaults for consistent observability (OpenTelemetry)
- Result<T> error handling pattern

---

## 2) Project Structure & File Organization

```
ArticleManagementApp/
├── src/
│   ├── AppHost/                    # .NET Aspire orchestration
│   │   ├── AppHost.cs              # Main orchestration (Web, MongoDB, Redis)
│   │   ├── DatabaseService.cs      # MongoDB configuration
│   │   ├── RedisService.cs         # Redis configuration
│   │   └── AppHost.csproj
│   ├── Web/                        # Blazor Server application
│   │   ├── Program.cs              # Startup & DI configuration
│   │   ├── App.razor               # Root component with layout
│   │   ├── Components/
│   │   │   ├── Layout/
│   │   │   │   ├── MainLayout.razor
│   │   │   │   ├── NavMenuComponent.razor
│   │   │   │   ├── FooterComponent.razor
│   │   │   │   └── ThemeToggleComponent.razor
│   │   │   ├── Shared/
│   │   │   │   ├── ErrorAlertComponent.razor
│   │   │   │   ├── LoadingComponent.razor
│   │   │   │   ├── PageHeaderComponent.razor
│   │   │   │   └── ReconnectModal.razor
│   │   │   ├── Articles/           # Feature: Article management
│   │   │   │   ├── ArticlesListComponent.razor
│   │   │   │   ├── ArticleDetailsComponent.razor
│   │   │   │   ├── ArticleFormComponent.razor
│   │   │   │   └── Handlers/
│   │   │   │       ├── GetArticles.cs
│   │   │   │       ├── GetArticleById.cs
│   │   │   │       ├── CreateArticle.cs
│   │   │   │       └── UpdateArticle.cs
│   │   │   ├── Categories/         # Feature: Category management
│   │   │   │   ├── CategoriesListComponent.razor
│   │   │   │   ├── CategoryDetailsComponent.razor
│   │   │   │   ├── CategoryFormComponent.razor
│   │   │   │   └── Handlers/
│   │   │   │       ├── GetCategories.cs
│   │   │   │       ├── GetCategoryById.cs
│   │   │   │       ├── CreateCategory.cs
│   │   │   │       └── UpdateCategory.cs
│   │   ├── Pages/
│   │   │   ├── Home.razor
│   │   │   ├── About.razor
│   │   │   ├── Contact.razor
│   │   │   ├── Articles.razor
│   │   │   ├── Categories.razor
│   │   │   ├── Admin.razor
│   │   │   └── Error.razor
│   │   ├── Data/                   # MongoDB services & configuration
│   │   │   ├── MongoDbContext.cs
│   │   │   ├── MongoDbContextFactory.cs
│   │   │   └── ServiceConfiguration.cs
│   │   ├── Repositories/           # Data access layer
│   │   │   ├── IArticleRepository.cs
│   │   │   ├── ArticleRepository.cs
│   │   │   ├── ICategoryRepository.cs
│   │   │   └── CategoryRepository.cs
│   │   ├── Services/
│   │   │   ├── AuthenticationService.cs
│   │   │   ├── FileUploadService.cs
│   │   │   └── SeedDataService.cs
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   │   └── app.css         # Bootstrap + custom overrides (dark blue gradients)
│   │   │   ├── js/
│   │   │   │   └── theme-toggle.js
│   │   │   ├── uploads/            # Local file storage for article covers
│   │   │   └── img/
│   │   ├── package.json            # No npm build; Bootstrap via CDN
│   │   └── Web.csproj
│   ├── Shared/                     # Shared domain models
│   │   ├── Entities/
│   │   │   ├── Article.cs
│   │   │   ├── Category.cs
│   │   │   └── AuthorInfo.cs
│   │   ├── Models/                 # DTOs
│   │   │   ├── ArticleDto.cs
│   │   │   └── CategoryDto.cs
│   │   ├── Abstractions/
│   │   │   ├── Result.cs           # Result<T> pattern
│   │   │   ├── IRepository.cs
│   │   │   └── ICommandHandler.cs
│   │   ├── Interfaces/
│   │   │   ├── IArticleRepository.cs
│   │   │   └── ICategoryRepository.cs
│   │   ├── Validators/
│   │   │   ├── ArticleDtoValidator.cs
│   │   │   └── CategoryDtoValidator.cs
│   │   ├── Constants/
│   │   │   └── AppConstants.cs
│   │   └── Shared.csproj
│   └── ServiceDefaults/
│       ├── Extensions.cs           # DI, OpenTelemetry, Health checks
│       └── ServiceDefaults.csproj
└── tests/
    ├── Shared.Tests.Unit/          # Unit tests for domain models & validators
    │   ├── Validators/
    │   ├── Models/
    │   └── Shared.Tests.Unit.csproj
    ├── Web.Tests.Unit/             # Unit tests for components & services
    │   ├── Components/
    │   └── Web.Tests.Unit.csproj
    ├── Web.Tests.Integration/       # Integration tests
    │   ├── RepositoryTests/
    │   ├── HandlerTests/
    │   ├── PageTests/
    │   ├── AuthenticationTests/
    │   └── Web.Tests.Integration.csproj
    └── Architecture.Tests/          # Code structure & naming conventions
        └── Architecture.Tests.csproj
```

---

## 3) Bootstrap Styling & Dark Mode Implementation

**Bootstrap Setup**:

- **Delivery**: CDN (jsDelivr) for CSS + Bundle JS with SRI integrity hashes
  - CSS: `<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-..." crossorigin="anonymous">`
  - JS: `<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" integrity="sha384-..." crossorigin="anonymous"></script>`
  - Rationale: No npm/Node toolchain required; jsDelivr has 100% uptime SLA; SRI hashes prevent tampering; no external build dependencies
- **Alternative**: LibMan for self-hosted if CDN unavailable; place in `wwwroot/lib/bootstrap/`

**Dark Mode Implementation**:

- **Approach**: Bootstrap 5.3 built-in `data-bs-theme` attribute
- **Mechanism**:
  - Initialize from localStorage on page load (in `<head>` to prevent FOUC)
  - Toggle sets `data-bs-theme="dark"` or `"light"` on `<html>` element
  - Bootstrap CSS variables auto-adjust colors (no custom Tailwind mapping needed)
- **Custom Overrides**: Minimal `app.css` for:
  - Logo colors (light/dark variants)
  - Custom component accents
  - Spacing/typography fine-tuning if needed
  - Transitions for smooth theme switching

**Files**:

- `App.razor`: Include Bootstrap CDN link in `<head>` (via layout or dynamic)
- `wwwroot/js/theme-toggle.js`: Early script to read localStorage and set `data-bs-theme` before first paint
- `wwwroot/css/app.css`: Bootstrap overrides (scoped to `.app` container or CSS variables)
- `ThemeToggleComponent.razor`: Blazor component to toggle theme and persist to localStorage

**Example Dark Mode CSS**:

```css
:root {
  --bs-body-color: #212529;
  --bs-body-bg: #ffffff;
}

[data-bs-theme="dark"] {
  --bs-body-color: #f8f9fa;
  --bs-body-bg: #212529;
  --bs-border-color: #495057;
}
```

---

## 4) Core Features & Functionality

### 4.1 **Articles Feature**

- **Pages**:
  - `/articles` - List articles with filtering, search, archive toggle
  - `/articles/create` - Create new article form (protected)
  - `/articles/{id}` - View article details
  - `/articles/{id}/edit` - Edit article (protected)
- **Components**:
  - `ArticlesListComponent.razor` - Grid/list with Bootstrap table or card layout
  - `ArticleDetailsComponent.razor` - Single article view with metadata
  - `ArticleFormComponent.razor` - Reusable form for create/edit (Bootstrap `.form-control`, `.form-label`, validation feedback)
- **CQRS Handlers**:
  - `GetArticles` - Query with pagination, filters, search
  - `GetArticleById` - Single article fetch
  - `CreateArticle` - Validate & persist new article
  - `UpdateArticle` - Validate & update existing article
- **Data**:
  - Entity: Title, content (markdown), cover image URL, author, category FK, publish date, archive flag
  - DTO: ArticleDto with validation rules (FluentValidation)
  - Repository: `IArticleRepository` with CRUD + query methods

### 4.2 **Categories Feature**

- **Pages**:
  - `/categories` - List categories
  - `/categories/create` - Create category
  - `/categories/{id}` - View category with articles
  - `/categories/{id}/edit` - Edit category
- **Components**: CategoryListComponent, CategoryDetailsComponent, CategoryFormComponent (similar pattern to Articles)
- **CQRS Handlers**: GetCategories, GetCategoryById, CreateCategory, UpdateCategory
- **Data**: Entity with name, slug, archive flag; Repository with CRUD + archive methods

### 4.3 **Shared UI Components** (Bootstrap-native)

- **Layout**: `MainLayout.razor` - Container, nav, footer, theme toggle
- **Navigation**: `NavMenuComponent.razor` - Navbar with `.navbar`, `.navbar-brand`, `.nav-link`; collapse on mobile (`navbar-collapse`, `navbar-toggler`)
- **Footer**: `FooterComponent.razor` - Bootstrap grid layout with links, copyright
- **Theme Toggle**: `ThemeToggleComponent.razor` - Button triggering theme switch via JS module + localStorage
- **Forms**: `FormComponent.razor` (generic) - Renders `.form-group`, `.form-control`, `.form-label`, `.invalid-feedback` for errors
- **Alerts**: `ErrorAlertComponent.razor` - `.alert .alert-danger` (or other variants)
- **Loading**: `LoadingComponent.razor` - Spinner via Bootstrap utilities (`.spinner-border`)
- **Page Header**: `PageHeaderComponent.razor` - `.jumbotron` or `.page-header` section with title/description

### 4.4 **Authentication & Authorization**

- **Auth0 Integration**:
  - `Microsoft.AspNetCore.Authentication.OpenIdConnect` for OAuth
  - Login/logout endpoints: `/Account/Login`, `/Account/Logout`
  - Auth State Provider for Blazor context with role claims
- **Role-Based Access Control** (Auth0 custom claims):
  - **Admin**: Full access to create, edit, delete, archive articles and categories; manage users; view admin dashboard
  - **Author**: Create and edit their own articles; view all published articles; cannot delete or manage categories
  - **User**: Read-only access to published articles and categories; view author profiles
- **Authorization Policies**:
  - `AdminOnly`: `@attribute [Authorize(Policy = "AdminOnly")]` for admin pages
  - `AuthorOrAdmin`: For content creation pages
  - `Authenticated`: For user-specific actions
- **Protected Pages**: Redirect to login if not authenticated; redirect to unauthorized error page if insufficient role
- **Admin Dashboard** (`/admin`): Shows article statistics, recent activity, user management (Authors/Admins only)
- **User Context**: Available in components via `@context.User` or injected `AuthenticationStateProvider`; includes role claims

### 4.5 **Data Access & Validation**

- **Repositories**: `ArticleRepository`, `CategoryRepository` using MongoDB driver
  - Methods: `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, `QueryAsync` (with filters)
  - Concurrency: Version/timestamp for optimistic locking if needed
- **FluentValidation**: `ArticleDtoValidator`, `CategoryDtoValidator`
  - Injected into handlers; called before persistence
  - Client-side validation via Blazor/ASP.NET Core validation system
- **Error Handling**: `Result<T>` pattern (Success or Failure with error messages)

---

## 5) Technology & Packages

### NuGet Packages

**Core Framework**:

- `Microsoft.AspNetCore.Components.QuickGrid` - Data grid (optional; use Bootstrap tables as fallback)
- `Aspire.Hosting`, `Aspire.Hosting.MongoDB`, `Aspire.StackExchange.Redis.OutputCaching` (service orchestration)

**Database & Caching**:

- `MongoDB.Driver`, `MongoDB.Bson`
- `Aspire.MongoDB.Driver`, `Aspire.StackExchange.Redis.OutputCaching`
- `AspNetCore.HealthChecks.MongoDb` - Health checks

**Authentication**:

- `Microsoft.AspNetCore.Authentication.OpenIdConnect` (Auth0)
- `Auth0.AspNetCore.Authentication` (optional; simplified OAuth)

**UI & Validation**:

- `FluentValidation` - Server & client validation
- `MyMediator` - CQRS handler pattern
- `Blazored.LocalStorage` - Theme persistence (or use JS interop)

**Data Utilities**:

- `Mapster` - DTO mapping (optional)
- `Markdig` - Markdown parsing and HTML rendering for article content (server-side, no JavaScript editor)
- `Bogus` - Fake data generation (testing)

**File Upload**:

- Custom `IFileUploadService` interface
- `LocalFileUploadService` implementation: stores files in `wwwroot/uploads/` with path validation and security checks
- Supports cover image uploads and user file attachments

**Testing**:

- `xunit`, `xunit.v3`
- `bunit` - Blazor component testing
- `Testcontainers`, `Testcontainers.MongoDb` - Integration test containers
- `Microsoft.AspNetCore.Mvc.Testing` - WebApplicationFactory
- `FluentAssertions`, `NSubstitute`, `Moq`

**Observability**:

- `OpenTelemetry`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.Runtime`

### npm Packages (Minimal)

- Optional: Markdown editor package if using external library (e.g., `EasyMDE`)
- Alternative: Use server-side markdown rendering with Markdig

---

## 6) Development Workflow & Setup

### 6.1 Prerequisites

- .NET 10 SDK
- MongoDB 8.0 (via Docker or local)
- Redis (via Docker)
- Visual Studio Code, Visual Studio Insiders, Visual Studio 2026 or JetBrains Rider
- Git for version control
- Docker Desktop (for TestContainers, Aspire)

### 6.2 Project Initialization

1. Create solution: `dotnet new sln -n ArticleManagementApp`
2. Create projects:
   - `dotnet new aspire-host -n AppHost -o src/AppHost`
   - `dotnet new web -n Web -o src/Web` (Blazor Server)
   - `dotnet new classlib -n Shared -o src/Shared`
   - `dotnet new classlib -n ServiceDefaults -o src/ServiceDefaults`
3. Create test projects:
   - `dotnet new xunit -n Shared.Tests.Unit -o tests/Shared.Tests.Unit`
   - `dotnet new xunit -n Web.Tests.Unit -o tests/Web.Tests.Unit`
   - `dotnet new xunit -n Web.Tests.Integration -o tests/Web.Tests.Integration`
   - `dotnet new xunit -n Architecture.Tests -o tests/Architecture.Tests`

### 6.3 Bootstrap Integration

1. Add Bootstrap CDN link (jsDelivr with SRI integrity hashes) to `App.razor` layout
2. Create `wwwroot/js/theme-toggle.js` for early theme initialization
3. Create `wwwroot/css/app.css` for custom overrides with dark blue gradients and hover effects
4. Wire theme toggle component in layout
5. Create `wwwroot/uploads/` directory structure for local file storage with proper permissions

### 6.4 MongoDB & Redis Setup

- **Docker Compose** or **Aspire**:

  ```csharp
  var mongoDb = builder.AddMongoDB("mongodb").AddMongoDbExpress();
  var redis = builder.AddRedis("redis");

  var web = builder.AddProject<Projects.Web>("web")
      .WithReference(mongoDb).WaitFor(mongoDb)
      .WithReference(redis).WaitFor(redis);
  ```

### 6.5 DI Configuration (Program.cs)

```csharp
// Repositories
services.AddScoped<IArticleRepository, ArticleRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();

// Validators
services.AddScoped<IValidator<ArticleDto>, ArticleDtoValidator>();
services.AddScoped<IValidator<CategoryDto>, CategoryDtoValidator>();

// CQRS Handlers
services.AddScoped<GetArticles.IHandler, GetArticles.Handler>();
services.AddScoped<CreateArticle.IHandler, CreateArticle.Handler>();
// ... etc

// File Upload Service (local wwwroot/uploads)
services.AddScoped<IFileUploadService, LocalFileUploadService>();

// Auth0 with role-based authorization
services.AddOpenIdConnectAuthentication(Configuration);
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AuthorOrAdmin", policy => policy.RequireRole("Author", "Admin"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});

// Cache & Output Cache
services.AddOutputCache(options => options.AddPolicy("default", ...));
services.AddStackExchangeRedisOutputCache(Configuration);

// Service Defaults (OpenTelemetry, Health Checks)
services.AddServiceDefaults();
```

---

## 7) Testing Strategy

### 7.1 Unit Tests (Shared.Tests.Unit)

- **Validators**: Test ArticleDtoValidator, CategoryDtoValidator for valid/invalid inputs
- **Models**: Test entity constructors, computed properties, defaults
- **Result<T>**: Test success/failure cases, error accumulation

### 7.2 Unit Tests (Web.Tests.Unit)

- **Components**: bUnit tests for ArticlesListComponent, CategoryFormComponent, ThemeToggleComponent
  - Assert rendered Bootstrap classes (`.btn`, `.form-control`, etc.)
  - Mock repositories and handlers
  - Test user interactions (clicks, form submissions)
- **Services**: Test AuthenticationService, FileUploadService with mocks

### 7.3 Integration Tests (Web.Tests.Integration)

- **WebApplicationFactory**: Custom `TestWebApplicationFactory` with in-memory DI override
- **Repository Tests**: MongoDB TestContainers
  - `ArticleRepository`: CRUD, queries, filters, pagination, concurrency
  - `CategoryRepository`: Similar coverage
- **Handler Tests**: Inject mocked repositories, test business logic
- **Page/Route Tests**: Verify `/articles`, `/categories` pages render with Bootstrap layout
- **Auth Tests**: Verify protected pages redirect to login; authorized users can access protected routes
- **Health Checks**: Verify `/health`, `/alive` endpoints respond

### 7.4 Architecture Tests

- Naming conventions (folders, classes, methods follow Vertical Slice pattern)
- Dependencies don't violate layering (Entities → Models → Handlers; no circular refs)
- All public APIs have XML documentation

### 7.5 Test Data & Fixtures

- `Bogus` for fake Article/Category generation
- Reusable fixtures in test helpers (`ArticleTestFixture`, `CategoryTestFixture`)
- Extension methods for assertions (`.ShouldBeSuccess()`, `.ShouldHaveError()`)

---

## 8) CI/CD Pipeline

### 8.1 GitHub Actions Workflow

- **Trigger**: Push to `main` or PR against `main`
- **Jobs**:
  1. **Build & Restore**: `dotnet build`, `dotnet restore`
  2. **Unit Tests**: `dotnet test tests/Shared.Tests.Unit tests/Web.Tests.Unit`
  3. **Integration Tests**: `dotnet test tests/Web.Tests.Integration` (Docker required)
  4. **Architecture Tests**: `dotnet test tests/Architecture.Tests`
  5. **Code Analysis**: (optional) SonarQube, CodeQL
  6. **Build Artifacts**: (optional) Pack NuGet packages
- **No E2E**: No Playwright or browser automation

### 8.2 Local Development

- `dotnet build` - Compile all projects
- `dotnet watch run --project src/AppHost` - Run Aspire with hot reload (MongoDB, Redis, Web auto-start)
- `dotnet test` - Run all unit/integration tests
- `dotnet test --collect:"XPlat Code Coverage"` - Coverage report

---

## 9) Deployment & Hosting

### 9.1 Target Environment

- **Container**: Docker (Azure Container Instances, AKS, Docker Compose, or local)
- **Orchestration**: Aspire (development) or Kubernetes (production)
- **Database**: MongoDB Atlas (cloud) or self-hosted MongoDB
- **Cache**: Azure Cache for Redis or self-hosted Redis
- **Auth**: Auth0 (managed service)

### 9.2 Docker Image

- Multistage Dockerfile: build stage (SDK) → publish stage → runtime (ASP.NET runtime)
- Environment variables for MongoDB connection, Redis, Auth0 client ID/secret
- Health check endpoint `/health`

### 9.3 Deployment Steps

1. Build Docker image: `docker build -t newblogapp:latest .`
2. Push to registry: `docker push registry.example.com/newblogapp:latest`
3. Deploy to Aspire/K8s: Wire services via orchestration manifest
4. Seed initial data: Run SeedDataService on startup or manual SQL/import

---

## 10) Acceptance Criteria

- ✅ Solution builds with `dotnet build` without errors
- ✅ MongoDB and Redis containers start via Aspire (`dotnet watch run`)
- ✅ Web app loads on `http://localhost:5000` with Bootstrap layout (nav, footer, theme toggle visible)
- ✅ Articles page displays list with Bootstrap table/cards; supports filtering, search, pagination
- ✅ Create/edit article forms render with `.form-control`, `.form-label`, validation feedback (`.invalid-feedback`)
- ✅ Dark mode toggle switches `data-bs-theme="dark"` on `<html>`; persists to localStorage; survives page refresh
- ✅ Categories feature mirrors Articles (list, create, edit, view)
- ✅ Auth0 OAuth flow works: login redirects to Auth0 → callback returns to app; protected pages show login prompt if unauthenticated
- ✅ Markdown content renders correctly on article details page
- ✅ All unit tests pass (`Shared.Tests.Unit`, `Web.Tests.Unit`); code coverage ≥ 80% for core logic
- ✅ Integration tests pass with TestContainers (MongoDB isolation, no external dependencies)
- ✅ Architecture tests enforce Vertical Slice structure and naming conventions
- ✅ CI pipeline green on all checks (build, unit, integration, architecture)
- ✅ No E2E tests; no Playwright or browser automation in codebase
- ✅ Documentation: README with setup, feature overview, testing instructions, deployment guide

---

## 11) Out-of-Scope

- Visual redesign beyond Bootstrap defaults; strict parity with ArticlesSite not required
- Pure Blazor + Bootstrap JS
- Advanced markdown features (embeds, custom plugins); basic Markdig rendering sufficient
- Admin UI for user/role management; keep simple with Auth0 claims
- Full-text search; basic string filtering acceptable initially
- Caching strategy optimization; basic output cache sufficient
- Performance profiling/load testing; functional correctness prioritized
- Mobile app; responsive web only

---

## 12) Key Decisions & Rationale

| Decision                        | Rationale                                                                                                     |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| **CDN Bootstrap**               | No npm/Node toolchain = faster greenfield setup; CDN has good uptime; SRI hashes mitigate tampering           |
| **data-bs-theme for dark mode** | Bootstrap 5.3 native support; avoids custom Tailwind dark: mapping complexity                                 |
| **CQRS + Handlers**             | Proven pattern in ArticlesSite; clear separation of concerns; testable                                        |
| **Vertical Slice**              | Self-contained features (Articles/, Categories/) reduce coupling; easier to add new features                  |
| **MongoDB**                     | Same as ArticlesSite; flexible schema for iterative development; Aspire integration                           |
| **Redis**                       | Output cache + session store; Aspire integration; optional scaling lever                                      |
| **No E2E tests from inception** | Reduces CI overhead; bUnit + Integration tests sufficient for core functionality; E2E adds maintenance burden |
| **Auth0**                       | OAuth simplicity; no credential management; consistent with ArticlesSite pattern                              |
| **FluentValidation**            | Declarative, reusable validation rules; server + client-side capable                                          |
| **Aspire**                      | Modern .NET cloud-native orchestration; local dev-prod parity; observability built-in                         |

---

## 13) Timeline Estimate

| Phase                         | Duration    | Deliverables                                                                           |
| ----------------------------- | ----------- | -------------------------------------------------------------------------------------- |
| **Setup & Infrastructure**    | 1–2 days    | Solution scaffolding, Aspire configuration, Bootstrap/theme integration, test projects |
| **Core Data Layer**           | 2–3 days    | MongoDB context, Article & Category repositories, CQRS handlers, validators            |
| **UI Components & Pages**     | 3–4 days    | Layout, shared UI (navbar, footer, theme toggle), Articles & Categories pages, forms   |
| **Authentication & Services** | 1–2 days    | Auth0 integration, file upload service, seed data                                      |
| **Testing & Refinement**      | 2–3 days    | Unit/integration/architecture tests, bug fixes, documentation                          |
| **CI/CD & Deployment**        | 1 day       | GitHub Actions workflow, Docker image, deployment instructions                         |
| **Total**                     | ~10–15 days | Fully functional, tested, deployable blog platform                                     |

---

## 14) Auth0 Setup Instructions

For developers setting up Auth0 for the first time:

### Create Auth0 Tenant & Application
1. Sign up for Auth0 free tier at https://auth0.com/signup
2. Create a new Application (name: "ArticleManagementApp")
3. Set Application Type: **Regular Web Application**
4. Configure OAuth Settings:
   - **Allowed Callback URLs**: `http://localhost:5000/callback, http://localhost:5000/signin-oidc`
   - **Allowed Logout URLs**: `http://localhost:5000/`
   - **Allowed Web Origins**: `http://localhost:5000`
5. Copy credentials to `appsettings.json`:
   ```json
   "Auth0": {
     "Domain": "your-tenant.auth0.com",
     "ClientId": "your-client-id",
     "ClientSecret": "your-client-secret"
   }
   ```

### Configure Roles
1. In Auth0 Dashboard → User Management → Roles
2. Create three roles: **Admin**, **Author**, **User**
3. Assign roles to test users or use Auth0 Actions to auto-assign on signup
4. Include roles in ID token via Auth0 Actions or custom claim rules

### Environment-Specific Configuration
- **Development**: Use test Auth0 tenant with sample users
- **Production**: Use production Auth0 tenant with proper security policies
- **Testing**: Mock Auth0 provider in test projects (optional)

---

## 15) Finalized Design Decisions

✅ **Project Name**: ArticleManagementApp
✅ **File Uploads**: Local wwwroot/uploads directory (no external storage)
✅ **Markdown Editor**: Server-side Markdig (no JavaScript editor)
✅ **Authentication**: Auth0 with role-based access (Admin, Author, User)
✅ **Admin Dashboard**: Yes - basic stats, article/category CRUD, user management
✅ **Additional Features**: None for MVP (no Pages, Tags, Comments, Search)
✅ **APIs**: None for MVP (pure Blazor Server)
✅ **Deployment**: Generic Docker + docker-compose.yml (no cloud-specific setup)
✅ **Bootstrap CDN**: jsDelivr with SRI integrity hashes
✅ **Database Seeding**: Auto-seed on startup with sample articles/categories (toggle via appsettings)

---

## Next Steps

Plan is now finalized and ready for **solution scaffolding**. All architectural decisions and implementation details have been resolved.

````
