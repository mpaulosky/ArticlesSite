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

---

### 2026-05-10: Aspire AppHost Startup — Secrets.json Corruption Fix

**Task:** Start and monitor the Aspire AppHost dashboard  
**Date:** 2026-05-10  
**Requested by:** Matthew Paulosky  

**Problem Found:** AppHost startup failed with JSON parsing error in user secrets file.
- **Location:** `/home/mpaulosky/.microsoft/usersecrets/8882ea07-326b-4521-bbdc-1f1fd68961c9/secrets.json`
- **Issue:** Stray `S` character appended at end of file (line 8), breaking JSON parser
- **Error Message:** `'S' is invalid after a single JSON value. Expected end of data. LineNumber: 7 | BytePositionInLine: 1`

**Root Cause:** Corrupted secrets.json file in user secrets directory (not source-controlled, but cached locally).

**Solution Applied:** 
1. Identified and removed stray `S` character from secrets.json
2. Verified JSON syntax
3. Restarted AppHost

**Startup Verification:** ✅ SUCCESS

**Aspire Resources Confirmed:**
1. **Redis Cache** — Container started (health: Healthy)
   - Ports: 127.0.0.1:33291→6379 (TLS), 127.0.0.1:33292→6380 (password-protected)
2. **MongoDB Server** — Container reused from earlier session (health: Healthy)
   - Port: 127.0.0.1:33294→27017
3. **Web (Blazor App)** — Resource configured (port 5057, HTTP binding)
   - Status: Starting → Running (transitioned during startup)
   - Health checks: `/health` endpoint configured

**Aspire Dashboard:**
- **URL:** https://localhost:17057
- **Login Token:** Available at startup (token: 0ddb01d13b1771fff9493570d6bd345f)
- **Full Login URL:** `https://localhost:17057/login?t=0ddb01d13b1771fff9493570d6bd345f`
- **Version:** 13.3.0+4517e4a1ffb7f00a4c0e66882c2db952d637c0cc

**AppHost Configuration:**
- Aspire version: 13.3.0
- Entry point: `src/AppHost/AppHost.cs`
- Resources defined: `AppHost.AddMongoDbServices()`, `AppHost.AddRedisServices()`
- Web project binds to: `http://localhost:5057` (hardcoded for Playwright compatibility)
- Health checks enabled on `/health` endpoint

**Key Learning:**
- User secrets file at `~/.microsoft/usersecrets/{ProjectId}/secrets.json` stores sensitive config (API keys, DB passwords, Aspire dashboard tokens)
- JSON parsing errors in this file prevent app startup (no recovery path)
- Aspire DCP (Distributed Control Plane) automatically manages Docker containers and health checks
- Resources auto-transition from Unhealthy → Healthy as their dependencies startup
