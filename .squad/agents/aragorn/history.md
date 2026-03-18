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
