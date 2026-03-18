# Boromir — DevOps History

## Project Context
- **Project:** ArticlesSite — a blog management application built with Blazor Server, .NET Aspire, and MongoDB
- **Stack:** .NET 10, C# 14, Blazor Server, MongoDB (MongoDB.Driver), .NET Aspire, Redis Cache, MediatR, Mapster, FluentValidation, Vertical Slice Architecture, Auth0
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/ArticlesSite
- **Source:** src/AppHost, src/ServiceDefaults, src/Shared, src/Web
- **Tests:** tests/Architecture.Tests, tests/Shared.Tests.Unit, tests/Web.Tests.Integration, tests/Web.Tests.Unit
- **Squad exported from:** IssueTrackerApp (2026-03-18)

## Learnings

### 2025-07-18: Squad workflow audit — purge IssueTrackerApp references
- All 11 squad workflow files under `.github/workflows/` were copied from IssueTrackerApp and contained stale references
- Fixed solution name references: `IssueTrackerApp.slnx` and `IssueManager.sln` → `ArticlesSite.slnx` (5 files)
- Fixed MongoDB test DB name: `issuemanager-test` → `articlessite-test` in `squad-test.yml`
- Fixed documentation title: `IssueTrackerApp Documentation` → `ArticlesSite Documentation` in `squad-docs.yml`
- Removed references to `tests/Api.Tests.Unit` (does not exist) from 3 release workflows
- Removed 5 phantom test jobs from `squad-test.yml` (Domain.Tests, Web.Tests.Bunit, Persistence.MongoDb.Tests, Persistence.MongoDb.Tests.Integration, Persistence.AzureStorage.Tests, Persistence.AzureStorage.Tests.Integration)
- Replaced with 2 jobs matching actual test directories: `test-shared-unit` (Shared.Tests.Unit) and `test-web-unit` (Web.Tests.Unit)
- Kept `test-architecture` and `test-integration` jobs (Architecture.Tests and Web.Tests.Integration exist)
- Updated coverage and report job dependency lists to match the 4 actual test jobs
- **Key insight:** This repo has exactly 4 test projects: Architecture.Tests, Shared.Tests.Unit, Web.Tests.Unit, Web.Tests.Integration
