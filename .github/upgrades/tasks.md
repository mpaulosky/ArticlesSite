# NET10 Upgrade — Bottom-Up (dependency-first)

## Overview

Apply the Bottom-Up strategy from Plan.md to upgrade the `ArticlesSite` solution to `net10.0` (preview), update CPM packages per assessment, and validate builds/tests per-tier. Tasks follow Plan §2 (Bottom-Up ordering), Plan §3.1 (topological order), Plan §5 (package updates), Plan §6 (breaking changes) and Plan §7 (testing/validation).

**Progress**: 0/10 tasks complete (0%) ![0%](https://progress-bar.xyz/0)

## Tasks

### [▶] TASK-001: Prerequisites — repo & SDK validation
**References**: Plan §1, Plan §9, Plan §12

- [▶] (1) Commit or stash any pending changes on the current branch per Plan §9/§12 (executor discretion: `git add -A && git commit -m "chore: save pending work before NET10 upgrade"` OR stash).  
- [▶] (2) Verify .NET 10 SDK (preview) is installed and available (`dotnet --list-sdks` / `dotnet --version`) (**Verify**) — see Plan §12.  
- [▶] (3) If `global.json` exists, validate or update SDK entry per Plan §12 to reference the required preview SDK (edit `global.json` only if explicitly required) (**Verify**).

---

### [ ] TASK-002: Upgrade Tier 1 — `src/Shared` to `net10.0`
**References**: Plan §3.2 (Phase 1), Plan §4 (Project: src/Shared)

- [ ] (1) Update `TargetFramework` to `net10.0` in `src/Shared/Shared.csproj` per Plan §4.  
- [ ] (2) Restore dependencies (`dotnet restore`) and build solution to identify compile issues (Plan §6).  
- [ ] (3) Fix compilation errors (API/obsolete changes) in `Shared` per Plan §6 (bounded to errors discovered) and rebuild.  
- [ ] (4) Solution compiles and `src/Shared` consumers (per Plan §3.1: `ServiceDefaults`, `Web`) compile successfully (**Verify**).  
- [ ] (5) Commit changes for Tier 1 with message: `"TASK-002: Upgrade Tier 1 - Shared to net10.0"` (**Verify**)

---

### [ ] TASK-003: Validate Tier 1 dependants (build verification)
**References**: Plan §3.1, Plan §4

- [ ] (1) Build and restore projects that directly depend on `src/Shared` (per Plan §3.1 list) to verify no downstream compilation regressions.  
- [ ] (2) All dependant projects compile with 0 errors (**Verify**).  
- [ ] (3) Fix any immediate consumer compile issues limited to adaptation to `Shared` changes (reference Plan §6) and commit small fixes if needed: `"TASK-003: Fix dependants after Shared upgrade"` (**Verify**)

---

### [ ] TASK-004: Upgrade Tier 2 — `src/ServiceDefaults` + Central Package Management
**References**: Plan §3.2 (Phase 2), Plan §5 (Package Update Reference), Plan §6 (Breaking Changes)

- [ ] (1) Update `src/ServiceDefaults/ServiceDefaults.csproj` `TargetFramework` → `net10.0` per Plan §4.  
- [ ] (2) Update Central Package Management (`Directory.Package.props`) versions for Tier 2 packages per Plan §5 (includes `Microsoft.Extensions.Http.Resilience` → `10.0.0`, `Microsoft.Extensions.ServiceDiscovery` → `10.0.0`, and aligned OpenTelemetry package updates `1.14.0-rc.1` or stable alternative) (**Action: edit `Directory.Package.props` as specified in Plan §5**).  
- [ ] (3) Restore (`dotnet restore`) and build solution to surface compilation and API incompatibilities.  
- [ ] (4) Apply code fixes for DI/registration and OpenTelemetry changes per Plan §6 (bounded to compilation/test failures). Note risk: OpenTelemetry `-rc` updates are higher risk — prefer stable if available (Plan §2, §5).  
- [ ] (5) Verify `ServiceDefaults` builds and OpenTelemetry/resilience registrations initialize (no compilation errors) (**Verify**).  
- [ ] (6) Commit changes for Tier 2 with message: `"TASK-004: Upgrade Tier 2 - ServiceDefaults and update CPM"` (**Verify**)

---

### [ ] TASK-005: Test Tier 2 — Integration & telemetry validation
**References**: Plan §7.1, Plan §5, Plan §6

- [ ] (1) Run integration tests that exercise `ServiceDefaults` behaviors and telemetry pipelines as specified in Plan §7.1 (reference Phase 4 test list in Plan §4 for exact test projects).  
- [ ] (2) Verify telemetry exporters and instrumentation initialize without errors and integration tests pass (**Verify**).  
- [ ] (3) If integration failures relate to package/API changes, apply bounded fixes per Plan §6 and re-run affected tests; commit fixes with message: `"TASK-005: Fix integration issues after ServiceDefaults upgrade"` (**Verify**)

---

### [ ] TASK-006: Upgrade Tier 3 — `src/Web` (Blazor) to `net10.0`
**References**: Plan §3.2 (Phase 3), Plan §4 (Project: src/Web), Plan §6

- [ ] (1) Update `TargetFramework` to `net10.0` in `src/Web/Web.csproj` per Plan §4.  
- [ ] (2) Restore and build solution to discover Blazor/hosting compilation issues.  
- [ ] (3) Apply required code updates for hosting/minimal API changes or Razor compilation issues (bounded to compiler/test results) per Plan §6.  
- [ ] (4) Verify `src/Web` builds with 0 errors (**Verify**).  
- [ ] (5) Commit Tier 3 changes with message: `"TASK-006: Upgrade Tier 3 - Web to net10.0"` (**Verify**)

---

### [ ] TASK-007: Test Tier 3 — Automated tests for Web (no manual UI)
**References**: Plan §7.1, Plan §4 (Phase 3)

- [ ] (1) Run unit and integration tests that target or depend on `src/Web` (reference Plan §4/Phase 3 for project mapping). Use `dotnet test` on the specified test projects per Plan §3.1/§4 (avoid manual UI steps).  
- [ ] (2) All automated tests targeting Web pass with 0 failures (**Verify**).  
- [ ] (3) If failures found, apply bounded fixes per Plan §6 and re-run tests; commit fixes: `"TASK-007: Fix Web test regressions after upgrade"` (**Verify**)

---

### [ ] TASK-008: Upgrade Tier 4 — Tests, `src/AppHost`, and E2E projects
**References**: Plan §3.2 (Phase 4), Plan §4 (Phase 4 project list), Plan §5 (test package updates)

- [ ] (1) Update `TargetFramework` to `net10.0` for test projects and `src/AppHost` per Plan §4 (projects: `tests/Web.Tests.Unit`, `tests/Web.Tests.Integration`, `tests/Shared.Tests.Unit`, `tests/Architecture.Tests`, `src/AppHost`, `e2e/Web.Tests.Playwright`).  
- [ ] (2) Update test-specific package(s) per Plan §5 (e.g., `Microsoft.AspNetCore.Mvc.Testing` → `10.0.0`) in `Directory.Package.props` or project files as applicable.  
- [ ] (3) Restore and build solution to surface test-hosting changes (e.g., `WebApplicationFactory` / test host behavior) per Plan §6.  
- [ ] (4) Apply bounded fixes to tests or `AppHost` required for compatibility and rebuild.  
- [ ] (5) Verify test projects and `AppHost` compile successfully (**Verify**).  
- [ ] (6) Commit Tier 4 changes with message: `"TASK-008: Upgrade Tier 4 - Tests, AppHost, E2E to net10.0 and update test packages"` (**Verify**)

---

### [ ] TASK-009: Test Tier 4 — Run unit, integration and E2E suites
**References**: Plan §7.1, Plan §4 (Phase 4 project list)

- [ ] (1) Run unit tests: `dotnet test tests/Web.Tests.Unit/Web.Tests.Unit.csproj --no-build` and `dotnet test tests/Shared.Tests.Unit/Shared.Tests.Unit.csproj --no-build` (reference exact paths in Plan §4).  
- [ ] (2) Run integration tests: `dotnet test tests/Web.Tests.Integration/Web.Tests.Integration.csproj --no-build` and other integration projects listed in Plan §4.  
- [ ] (3) Run E2E Playwright tests via `dotnet test e2e/Web.Tests.Playwright/Web.Tests.Playwright.csproj --no-build` (ensure Playwright prerequisites are met outside this automation if required).  
- [ ] (4) All automated test runs complete with 0 failures (**Verify**).  
- [ ] (5) If failures occur, apply bounded fixes per Plan §6 and re-run affected test suites; commit fixes with message: `"TASK-009: Fix tests after Tier 4 upgrade"` (**Verify**)

---

### [ ] TASK-010: Finalize upgrade and verification
**References**: Plan §11 (Success Criteria), Plan §9 (Source Control)

- [ ] (1) Ensure all projects target `net10.0` per Plan §11 (verify csproj `TargetFramework` values across projects) (**Verify**).  
- [ ] (2) Ensure central package updates from Plan §5 are applied and consistent (`Directory.Package.props`) — verify no unintended version changes in other projects (**Verify**).  
- [ ] (3) Run a final solution restore & build: `dotnet restore && dotnet build -c Release` — solution builds with 0 errors (**Verify**).  
- [ ] (4) Run a final `dotnet test` for the full solution or the specified test projects per Plan §7 and confirm 0 failures (**Verify**).  
- [ ] (5) Commit any remaining changes with message: `"TASK-010: Finalize NET10 upgrade and package audit"` and push branch / open PR per repository workflow (executor discretion) (**Verify**)

--- 

Generation checklist (applied)
- Strategy rules: Bottom-Up — batched by tier; project updates + package updates + compilation fixes grouped per-tier; testing separated per-tier.  
- Large lists referenced to Plan § sections (no duplication).  
- Non-automatable/manual UI steps excluded (manual smoke tests noted but not automated).  
- All verifications deterministic and bounded (build/test 0 errors/failures).
