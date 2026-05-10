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

---

### 2026-05-09: Build Verification — Full Solution Health Check
**Task:** Full build repair (requested by Matthew Paulosky)
**Outcome:** ✅ No repairs needed — solution already clean
**Build Status:**
  - Restore: ✅ Success (1.1s)
  - Build: ✅ Success (2.9s)
  - Errors: 0
  - Warnings: 0
  - Projects: 8/8 compiled successfully

**Projects Verified:**
1. Shared → src/Shared/bin/Debug/net10.0/Shared.dll
2. Shared.Tests.Unit → tests/Shared.Tests.Unit/bin/Debug/net10.0/Shared.Tests.Unit.dll
3. ServiceDefaults → src/ServiceDefaults/bin/Debug/net10.0/ServiceDefaults.dll
4. Web → src/Web/bin/Debug/net10.0/Web.dll (1.4s build time)
5. AppHost → src/AppHost/bin/Debug/net10.0/AppHost.dll
6. Architecture.Tests → tests/Architecture.Tests/bin/Debug/net10.0/Architecture.Tests.dll
7. Web.Tests.Integration → tests/Web.Tests.Integration/bin/Debug/net10.0/Web.Tests.Integration.dll
8. Web.Tests.Unit → tests/Web.Tests.Unit/bin/Debug/net10.0/Web.Tests.Unit.dll

**Key Observations:**
- All NuGet dependencies resolved correctly from Directory.Packages.props
- No XML syntax errors in centralized package configuration

---

### 2026-05-10: Build Repair Session — Auth0 Test Stub Fix

**Task:** Validate solution build integrity as part of squad repair pipeline  
**Result:** ✅ Build validation passed cleanly (0 errors, 0 warnings, 8 projects compiled)  
**Output:** Logged to `build-log.txt`  
**Next Step:** Test suite execution by Gimli revealed 1 auth test failure → Fixed by Aragorn  
**Final Outcome:** All 906 tests now passing. Pre-push gate unblocked.
- All project references resolved cleanly
- Blazor static web assets built successfully
- Source control integration (SourceLink) configured correctly
- All conventions enforced: file-scoped namespaces, copyright headers, async naming
- Web project includes Tailwind CSS build step (completed successfully)

**Log:** Full build log written to `/home/mpaulosky/Repos/ArticlesSite/build-log.txt`

**Conclusion:** Solution is in excellent health. Previous PR #97 fixes (XML typo, MongoDB version alignment) are holding. No DevOps intervention required.
