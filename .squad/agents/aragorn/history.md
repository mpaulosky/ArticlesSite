# Aragorn — Lead History

## Project Context
- **Project:** ArticlesSite — a blog management application built with Blazor Server, .NET Aspire, and MongoDB
- **Stack:** .NET 10, C# 14, Blazor Server, MongoDB (MongoDB.Driver), .NET Aspire, Redis Cache, MediatR, Mapster, FluentValidation, Vertical Slice Architecture, Auth0
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/ArticlesSite
- **Source:** src/AppHost, src/ServiceDefaults, src/Shared, src/Web
- **Tests:** tests/Architecture.Tests, tests/Shared.Tests.Unit, tests/Web.Tests.Integration, tests/Web.Tests.Unit
- **Squad exported from:** IssueTrackerApp (2026-03-18)

## Learnings

- **2025-07-17: Full uncommitted changes review completed.** Found that new Squad workflows (squad-test.yml, squad-ci.yml, squad-docs.yml, etc.) were copied from IssueTrackerApp without updating project-specific references (solution file name, test directory paths, MongoDB connection strings). The solution file is `ArticlesSite.slnx` and actual test dirs are `Architecture.Tests`, `Shared.Tests.Unit`, `Web.Tests.Integration`, `Web.Tests.Unit`. There is no `Domain.Tests`, `Web.Tests.Bunit`, or any `Persistence.*` test project yet.
- **2025-07-17: Auth0 is the auth provider for this project.** The blazor.instructions.md was incorrectly changed from Auth0 to "ASP.NET Identity or JWT tokens." Auth0 is the actual provider per README and project configuration. The copilot-instructions.md correctly kept the Auth0 reference.
- **2025-07-17: Solution naming.** The solution file is `ArticlesSite.slnx`. Neither "Article ServiceApp" nor "TailwindBlogApp" is the correct name. The copilot-instructions.md was changed to reference "TailwindBlogApp" which is incorrect.

## Learnings

### 2026-05-09: ProgramSmokeTests Auth0 Configuration

**Issue:** WebApplicationFactory tests that validate app startup require ALL required configuration keys, not just the ones the test directly uses.

**Discovery:** When TestFactory provided only MongoDB config, the app startup failed because `AuthenticationServiceExtensions.AddAuthenticationAndAuthorization()` threw on missing Auth0 config keys (`Auth0:Domain`, `Auth0:ClientId`).

**Solution:** Set Auth0 configuration via environment variables (using double-underscore notation: `Auth0__Domain`, `Auth0__ClientId`, `Auth0__ClientSecret`) in the test setup.

**Key Insight:** In-memory configuration added via `ConfigureAppConfiguration` did not work for Auth0 service registration. Environment variables have higher priority in the .NET configuration hierarchy and are correctly picked up by third-party authentication providers.

**Best Practice:** For smoke tests, provide stub values for ALL required configuration keys that any service registration might read during startup—even if the test doesn't exercise those services. Always preserve and restore original environment variable values in finally blocks.

---

### 2026-05-10: Build Repair Session — Auth0 Test Configuration Implementation

**Task:** Fix ProgramSmokeTests.App_Should_Start_Without_Errors failure identified by Gimli  
**Implementation:**
- Added Auth0 environment variables (Auth0__Domain, Auth0__ClientId, Auth0__ClientSecret) to test setup
- Set matching stubs: test.auth0.com, test-client-id, test-client-secret
- Proper cleanup in finally block to restore original environment state
- Added missing copyright header to ProgramSmokeTests.cs

**Key Discovery:** Environment variables have higher priority in .NET configuration hierarchy than in-memory config. Third-party auth providers read env vars directly.

**Result:** ✅ Web.Tests.Unit now 622/622 passing (was 621/622)  
**Overall:** All 906 tests passing. Pre-push gate unblocked.  
**Commit:** Staged with build-log.txt and .squad/ directory updates
