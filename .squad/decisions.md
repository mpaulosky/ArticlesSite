# Squad Decisions

## Active Decisions

### 2026-03-18: Squad initialized (export from IssueTrackerApp)
**By:** Squad Coordinator
**What:** Squad exported from IssueTrackerApp to ArticlesSite. Same team, same universe (Lord of the Rings), fresh project state.
**Why:** Consistent team across mpaulosky projects

---

### 2025-07-17: Block Squad Workflows Until Fixed
**By:** Aragorn (Lead Developer)
**Status:** ✅ Resolved (2026-03-18)
**What:** Squad workflows contained stale IssueTrackerApp/IssueManager references and wrong test paths. Blocked from commit until fixed.
**Fixes Applied:** All 11 workflow files updated; solution names corrected; test directory paths aligned to ArticlesSite structure; MongoDB database names fixed.
**Outcome:** `fix: purge IssueTrackerApp references from squad workflows` committed.

---

### 2025-07-18: Squad workflow audit — purge IssueTrackerApp references
**By:** Boromir (DevOps)
**What:** Comprehensive audit of 11 squad workflow files. Replaced all stale IssueTrackerApp/IssueManager references with ArticlesSite equivalents. Removed 5 phantom test jobs from `squad-test.yml` that referenced non-existent test projects.
**Why:** Workflows were copied from IssueTrackerApp during squad export and had never been adapted. Would fail immediately in CI due to wrong solution file names and non-existent test directories.
**Impact:**
  - `squad-test.yml`, `squad-ci.yml`, `squad-release.yml`, `squad-preview.yml`, `squad-insider-release.yml`, `squad-docs.yml` all updated
  - CI now builds `ArticlesSite.slnx` and runs tests against 4 actual test projects
  - MongoDB integration tests use `articlessite-test` database name
**Status:** ✅ Committed

---

### 2026-03-18: Triage Decision: PR #85 (Build Repair)
**By:** Aragorn (Lead Developer)
**Date:** 2026-03-18
**PR:** #85 — "Build repair: fix compilation errors and test failures across ArticlesSite solution"
**What:** PR claims to fix 22 failing tests and build errors that no longer exist on main (commit `6a6d094`).
**Analysis:**
  - Main branch builds cleanly with 0 errors (verified 2026-03-18)
  - PR base (`621d743`) is 9 commits behind current main
  - Merge conflicts confirmed (mergeable_state: "dirty")
  - 321 files changed; intervening commits (#82–#84) have superseded all fixes this PR claims to make
**Decision:** ✅ **CLOSE PR** — No merge
**Rationale:**
  1. Main already builds cleanly; no pending issues
  2. PR is stale (9+ commits behind)
  3. Merge surface too large; conflicts outweigh value
  4. Auto-generated PR timing is too sensitive for large repair changes
**Actions:**
  1. Close PR without merging
  2. Continue monitoring main for regressions
  3. If new build issues arise, create fresh PR from current main
**Notes for Future:** Auto-generated PRs are time-sensitive; prefer incremental fixes; rebase frequently

---

### 2026-05-09: PR #97 CI Fix — XML Typo in Directory.Packages.props
**By:** Sam (Backend Developer)  
**Status:** ✅ Implemented & Merged
**PR:** #97 (`build-repair-update-aspire`)  
**Merge:** 2026-05-09T23:16:58Z (Commit: 0485b05)

**Problem:** PR #97 failed CI with NU1015 errors (missing package versions) across all projects.

**Root Cause:** Malformed XML in `Directory.Packages.props` line 16 — stray double-quote on Aspire.MongoDB.Driver:
```xml
<PackageVersion Include="Aspire.MongoDB.Driver" Version="13.3.0"" />  <!-- ❌ Extra quote -->
```
XML parsing stops at syntax error; all subsequent package versions not loaded → cascading NU1015 errors.

**Additional Issues Found:**
1. MongoDB.Driver.Core (3.7.1) version mismatch with MongoDB.Driver (3.8.0) — should match
2. 8 .lscache files from VS Code C# Dev Kit committed (build cache should not be in source control)
3. Transitive security warnings (NU1902/NU1903) from MongoDB.Driver dependencies (SharpCompress 0.30.1, Snappier 1.0.0)

**Solution Applied:**
1. Fixed XML typo: Removed stray double-quote → `Version="13.3.0"`
2. Updated MongoDB.Driver.Core to 3.8.0 to match MongoDB.Driver
3. Removed all 8 .lscache files from repository
4. Added NU1902 and NU1903 to NoWarn (upstream MongoDB responsibility)

**Verification:**
- ✅ `dotnet restore` success
- ✅ `dotnet build` success (1 unrelated warning)
- ✅ All NU1015 errors resolved
- ✅ Package version resolution complete across all projects

**Key Learning:** XML parsing errors in centralized package files create cascading failures. One typo → dozens of errors across entire solution. Always verify XML syntax before commit. Always align related package versions (e.g., MongoDB.Driver ↔ MongoDB.Driver.Core).
