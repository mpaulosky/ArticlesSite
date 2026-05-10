# Gimli — Tester History

## Project Context
- **Project:** ArticlesSite — a blog management application built with Blazor Server, .NET Aspire, and MongoDB
- **Stack:** .NET 10, C# 14, Blazor Server, MongoDB (MongoDB.Driver), .NET Aspire, Redis Cache, MediatR, Mapster, FluentValidation, Vertical Slice Architecture, Auth0
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/ArticlesSite
- **Source:** src/AppHost, src/ServiceDefaults, src/Shared, src/Web
- **Tests:** tests/Architecture.Tests, tests/Shared.Tests.Unit, tests/Web.Tests.Integration, tests/Web.Tests.Unit
- **Squad exported from:** IssueTrackerApp (2026-03-18)

## Learnings

### Test Suite Baseline (2026-05-09)
- **Pre-Push Gate Tests:** 721 tests total (Shared.Tests.Unit: 57, Web.Tests.Unit: 622, Architecture.Tests: 43)
- **Integration Tests:** 185 tests (require Docker for mongo:8.0 container via TestContainers)
- **Total Suite:** 906 tests covering unit, integration, architecture, and component testing
- **Standard Duration:** ~10 seconds for unit tests, ~90 seconds for integration tests with container spin-up

### Known Issues
- **ProgramSmokeTests.App_Should_Start_Without_Errors** fails when Auth0:Domain configuration is missing
  - Root cause: WebApplicationFactory triggers full app startup including Auth0 authentication setup
  - Impact: Pre-push gate fails if Auth0 config not present in test environment
  - Solution needed: Mock Auth0 configuration or override WebApplicationFactory to bypass Auth0 during smoke tests
  - Assigned to: Aragorn (Web lead) for resolution

### Test Patterns Observed
- All unit tests use FluentAssertions (`.Should()` syntax) consistently
- Integration tests use `[Collection("MongoDb Collection")]` with `MongoDbFixture : IAsyncLifetime`
- bUnit tests for Blazor components use `TestContext` and render fragments
- Architecture tests enforce naming conventions, dependencies, and structural rules via NetArchTest.Rules

---

### 2026-05-10: Build Repair Session — Smoke Test Failure Analysis

**Task:** Execute full test suite as part of automated repair pipeline  
**Results:** 905/906 tests passing (99.9%)  
**Failure Found:** ProgramSmokeTests.App_Should_Start_Without_Errors  
**Root Cause:** WebApplicationFactory startup required Auth0 configuration that TestFactory didn't provide  
**Analysis:** Documented in gimli-test-failures.md with 3 solution options  
**Recommendation:** Option 1 (mock Auth0 config in TestFactory) chosen by Aragorn and implemented  
**Final Outcome:** All 906 tests now passing after Aragorn's fix. Pre-push gate unblocked.
