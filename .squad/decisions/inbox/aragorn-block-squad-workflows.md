# Decision: Block Squad Workflows Until Fixed

**By:** Aragorn (Lead Developer)
**Date:** 2025-07-17
**Status:** Proposed

## Context

New Squad workflows were added to `.github/workflows/` but they contain multiple references to `IssueTrackerApp` and `IssueManager` — the previous project they were exported from. These will fail in CI immediately.

## Decision

Do NOT commit the new Squad workflows (`squad-test.yml`, `squad-ci.yml`, `squad-docs.yml`, `squad-insider-release.yml`, `squad-preview.yml`, `squad-release.yml`) until all of the following are fixed:

1. `squad-test.yml` line 70: `IssueTrackerApp.slnx` → `ArticlesSite.slnx`
2. `squad-test.yml`: All test directory paths must match actual project structure (`Shared.Tests.Unit`, `Web.Tests.Unit`, `Architecture.Tests`, `Web.Tests.Integration` — not Domain.Tests, Web.Tests, Web.Tests.Bunit, or any Persistence.* paths)
3. `squad-test.yml` line 293: MongoDB connection string database name `issuemanager-test` → `articlessite-test`
4. `squad-insider-release.yml`, `squad-preview.yml`, `squad-release.yml`: `IssueManager.sln` → `ArticlesSite.slnx`
5. `squad-docs.yml` line 46: `IssueTrackerApp Documentation` → `ArticlesSite Documentation`
6. `blazor.instructions.md`: Revert Auth0 → "ASP.NET Identity or JWT tokens" change — Auth0 is the actual provider
7. `copilot-instructions.md`: Fix "TailwindBlogApp" → "ArticlesSite" and `TailwindBlog.Persistence.MongoDb.Tests.Integration` path

## Rationale

Committing broken CI workflows will block all PRs and erode team confidence in the pipeline. These are straightforward find-and-replace fixes but must be done before the workflows are committed.
