# Decision: Squad workflow audit — purge IssueTrackerApp references

**By:** Boromir (DevOps)
**Date:** 2025-07-18

## What
Audited and fixed all 11 squad workflow files under `.github/workflows/`. Replaced every stale IssueTrackerApp/IssueManager reference with ArticlesSite equivalents. Removed 5 phantom test jobs from `squad-test.yml` that referenced non-existent test projects (Domain.Tests, Web.Tests.Bunit, Persistence.MongoDb.Tests, Persistence.AzureStorage.Tests and their integration counterparts). The CI pipeline now targets exactly the 4 test projects that exist in this repo.

## Why
These workflows were copied from the IssueTrackerApp project during squad export and had never been adapted. They would fail immediately in CI due to wrong solution file names and non-existent test directories.

## Impact
- `squad-test.yml`, `squad-ci.yml`, `squad-release.yml`, `squad-preview.yml`, `squad-insider-release.yml`, `squad-docs.yml` all updated
- CI will now build `ArticlesSite.slnx` and run tests against the 4 actual test projects
- MongoDB integration tests use `articlessite-test` database name
