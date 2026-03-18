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
