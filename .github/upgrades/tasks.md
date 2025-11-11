# Big Bang: Migrate `Newtonsoft.Json` to `System.Text.Json`

## Overview

Migrate all projects in the solution simultaneously from `Newtonsoft.Json` to `System.Text.Json` using the Big Bang (atomic) approach. Tasks below follow the Big Bang batching rules: combine all project file updates, package updates, restore, build and compilation fixes into a single atomic task; run automated tests in a separate task; perform final commit as the last task. Manual or visual validations described in the plan are excluded from tasks.

**Progress**: 3/3 tasks complete (100%) ![100%](https://progress-bar.xyz/100)

## Tasks

### [✓] TASK-001: Atomic framework and package upgrade (all projects) *(Completed: 2025-11-11 11:56)*
**References**: Plan §4 (Project-by-Project Migration Plans), Plan §3.2 (Dependency-Based Ordering), Plan §8 (Source Control Strategy), `\.github/upgrades/plan.md`

- [✓] (1) Update `TargetFramework` (to `net10.0` where applicable), remove `Newtonsoft.Json` and add `System.Text.Json` across all projects per Plan §4 (see project list)
- [✓] (2) Update package references per Plan §4 Package Updates / Package Update Reference (use versions specified in the Plan)
- [✓] (3) Restore dependencies for the solution (`dotnet restore`) (**Verify**: restore completes without errors)
- [✓] (4) Build the solution and fix all compilation errors caused by framework/package updates per Plan §4 (Breaking Changes Catalog). This is a bounded, single pass: apply fixes documented in the Plan and relevant converters/settings updates.
- [✓] (5) Solution builds with 0 errors (**Verify**)

### [✓] TASK-002: Run automated test suites and fix failures *(Completed: 2025-11-11 12:08)*
**References**: Plan §6 (Testing and Validation Strategy), Plan §4 (Project-by-Project Migration Plans - Test Projects)

- [✓] (1) Run tests in the following test projects: `Web.Tests.Unit`, `Web.Tests.Integration`, `Shared.Tests.Unit`, `Architecture.Tests`, `Web.Tests.Playwright` (per Plan §4)
- [✓] (2) Investigate and fix any test failures attributable to the migration (reference Plan §4 Breaking Changes and test helper updates)
- [✓] (3) Re-run the affected test suites after fixes
- [✓] (4) All automated tests pass with 0 failures (**Verify**)

### [✓] TASK-003: Final commit and push *(Completed: 2025-11-11 12:09)*
**References**: Plan §8 (Source Control Strategy), Plan §2.1 (Approach Selection)

- [✓] (1) Commit all changes with message: "Migrate all projects from Newtonsoft.Json to System.Text.Json" (or the repository's standard upgrade commit message as specified in Plan §8)
- [✓] (2) Push branch and create PR as required by the repository workflow (**Verify**: commit/push succeed and PR is created per Plan §8)
