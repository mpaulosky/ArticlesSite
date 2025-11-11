# .github/upgrades/plan.md

## 1. Executive Summary

- Scenario: Upgrade solution `ArticlesSite` from current targets (`net8.0` and `net9.0`) to `net10.0` (preview) across all projects and apply security-related NuGet updates.
- Scope: 9 projects identified in analysis (libraries, Blazor/Web, AppHost, unit/integration/e2e tests). Current branches: source `wip`, upgrade branch recommended `upgrade-to-NET10`. Repository has pending changes that should be committed before branching.
- Target State: All projects target `net10.0`; packages updated to suggested versions from assessment; Central Package Management (`Directory.Package.props`) updated where applicable; CI and tests validated.
- Selected Strategy: Bottom-Up (dependency-first). Rationale: solution contains multiple projects with clear dependency ordering and a Blazor/Web app; Bottom-Up minimizes risk by upgrading leaf libraries first and stabilizing upwards.
- Complexity Assessment: Medium. Codebase spans multiple projects (including tests and Blazor web app) and uses Central Package Management. Several packages require updates; OpenTelemetry updates are release-candidate versions (higher risk).
- Critical Issues: Pending Git changes (must commit/stash/handle); OpenTelemetry updates include `-rc` versions (1.14.0-rc.1) which may introduce API or behavior changes; must update central package props rather than individual csproj for ServiceDefaults.
- Recommended Approach: Incremental Bottom-Up upgrade that includes security updates. Address OpenTelemetry RC risk by validating in integration tests and consider pinning to stable if available.

---

## 2. Migration Strategy

### 2.1 Approach Selection
- Chosen Strategy: Bottom-Up (dependency-first).
- Strategy Rationale: There are multiple projects (9) with a clear dependency order. Bottom-Up reduces blast radius by upgrading libraries first and letting higher-level projects compile against upgraded dependencies.
- Strategy-Specific Considerations: Batch upgrades per-tier; update project target frameworks and grouped package updates in Directory.Package.props (CPM) where applicable; treat test projects last.

### 2.2 Dependency-Based Ordering
- Projects will be migrated in strict topological order derived from analysis. Cannot start a higher-tier until lower-tier validated.
- Critical path: `Shared` -> `ServiceDefaults` -> `Web` -> tests/AppHost/e2e
- No circular dependencies reported in analysis.

### 2.3 Parallel vs Sequential Execution
- Within a tier containing multiple independent projects, projects can be upgraded in parallel when capacity allows.
- Across tiers: strictly sequential (Tier N must be validated complete before Tier N+1 starts).

---

## 3. Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

Tier 4: [tests/Web.Tests.Unit] [tests/Web.Tests.Integration] [tests/Shared.Tests.Unit] [tests/Architecture.Tests] [src/AppHost] [e2e/Web.Tests.Playwright]
         ↓
Tier 3: [src/Web]
         ↓
Tier 2: [src/ServiceDefaults]
         ↓
Tier 1 (leaf): [src/Shared]

(Topological order from analysis: `Shared` → `ServiceDefaults` → `Web` → unit/integration/shared/architecture tests → `AppHost` → e2e Playwright)

### 3.2 Project Groupings (Phases)
- Phase 0: Preparation and repo/tooling checks
- Phase 1 (Tier 1): Foundation — `src/Shared\Shared.csproj`
- Phase 2 (Tier 2): Mid-tier — `src/ServiceDefaults\ServiceDefaults.csproj`
- Phase 3 (Tier 3): Application — `src/Web\Web.csproj`
- Phase 4 (Tier 4): Tests & AppHost & E2E — all test projects, `src/AppHost\AppHost.csproj`, `e2e/Web.Tests.Playwright\Web.Tests.Playwright.csproj`

---

## 4. Project-by-Project Migration Plans

NOTE: All file paths below are relative to repository root `E:\github\ArticlesSite` and use exact project file paths from assessment.

### Project: `src/Shared\Shared.csproj`

Current State
- Current Target Framework: `net8.0`
- Dependants: `src/ServiceDefaults\ServiceDefaults.csproj`, `src/Web\Web.csproj` (per topology)
- LOC / metrics: use assessment.md for exact metrics.

Target State
- Target Framework: `net10.0`
- Package updates: none indicated in assessment for this project

Migration Steps
1. Prerequisites: None (leaf). Ensure Phase 0 completed.
2. Framework Update: Update TargetFramework to `net10.0` in `src/Shared\Shared.csproj`.
3. Package Updates: none required per assessment.
4. Expected Breaking Changes: API-level changes between net8/net10 are minimal for plain class libraries but watch for obsolete APIs used.
5. Code Modifications: Replace obsolete APIs discovered on build; update any conditional compilation symbols if present.
6. Testing: Run unit tests that directly test Shared (if any) and run dependent consumers in integration mode (ServiceDefaults build referencing updated Shared).

Validation Checklist
- [ ] Project builds without errors
- [ ] No new warnings introduced
- [ ] Unit tests passing for Shared
- [ ] Dependants compile against updated Shared

---

### Project: `src/ServiceDefaults\ServiceDefaults.csproj`

Current State
- Current Target Framework: `net9.0`
- Depends on: `src/Shared\Shared.csproj`
- Uses Central Package Management (CPM) — versions defined in `Directory.Package.props`.
- Packages of interest (from assessment): `Microsoft.Extensions.Http.Resilience` 9.10.0, `Microsoft.Extensions.ServiceDiscovery` 9.5.2, `OpenTelemetry.*` packages 1.13.x

Target State
- Target Framework: `net10.0`
- Package updates: apply suggested security/compat updates in central props

Migration Steps
1. Prerequisites: Phase 1 (Shared) must be validated complete.
2. Framework Update: Update TargetFramework to `net10.0` in `src/ServiceDefaults\ServiceDefaults.csproj`.
3. Package Updates (update in `Directory.Package.props`):
   - `Microsoft.Extensions.Http.Resilience`: `9.10.0` → `10.0.0`
   - `Microsoft.Extensions.ServiceDiscovery`: `9.5.2` → `10.0.0`
   - `OpenTelemetry.Instrumentation.AspNetCore`: `1.13.0` → `1.14.0-rc.1`
   - `OpenTelemetry.Instrumentation.Http`: `1.13.0` → `1.14.0-rc.1`
   - `OpenTelemetry.Extensions.Hosting`: `1.13.1` → `1.14.0-rc.1`
   - `OpenTelemetry.Exporter.OpenTelemetryProtocol`: `1.13.1` → `1.14.0-rc.1`
   - `OpenTelemetry.Instrumentation.Runtime`: `1.13.0` → `1.14.0-rc.1`
4. Expected Breaking Changes:
   - OpenTelemetry 1.14.0-rc.1 may introduce API changes. Verify instrumentation registration calls and exporter configuration.
   - Microsoft.Extensions.* 10.0.0 packages may have API surface changes aligned with .NET 10. Review usage sites.
5. Code Modifications:
   - Update DI registration calls if any APIs changed (e.g., resilience/service discovery extension methods).
   - Update OpenTelemetry setup: `AddOpenTelemetry*` registration APIs or options objects may have changed; verify middleware and HTTP instrumentation configuration.
6. Testing:
   - Unit tests for ServiceDefaults (if present).
   - Integration tests that exercise OpenTelemetry pipelines and resilience/service discovery behaviors.

Validation Checklist
- [ ] Project builds without errors
- [ ] All OpenTelemetry exporters and instrumentation initialize correctly
- [ ] Integration tests that depend on tracing/logging pass
- [ ] No security vulnerabilities remain for packages updated

Notes
- Because CPM is used, update central `Directory.Package.props` and verify no other projects are unexpectedly affected.
- Treat OpenTelemetry RC packages as higher risk — if a stable 1.14.0 is available by execution time, prefer it.

---

### Project: `src/Web\Web.csproj` (Blazor web project)

Current State
- Current Target Framework: `net9.0`
- Likely depends on Shared and ServiceDefaults (per topology). This is the Blazor web UI in the workspace.

Target State
- Target Framework: `net10.0`

Migration Steps
1. Prerequisites: Phase 1 & Phase 2 completed and validated.
2. Framework Update: Update `TargetFramework` to `net10.0` in `src/Web\Web.csproj`.
3. Package Updates: No explicit package updates in assessment for Web. However, verify ASP.NET Core package alignment with SDK and test packages.
4. Expected Breaking Changes:
   - Blazor startup patterns: review `Program.cs` minimal hosting changes if any. net10 may change some hosting behaviors; adjust calls to `builder.Services` or `builder.RootComponents` as needed.
   - Static web assets, razor compilation or default route behaviors could differ; run app and smoke test UI.
5. Code Modifications:
   - Replace obsolete APIs discovered during build.
   - Update any JS interop or third-party component packages if incompatible.
6. Testing:
   - Run unit tests referencing Web.
   - Manual smoke test the Blazor UI: navigation, authentication (if present), core flows.

Validation Checklist
- [ ] Project builds without errors
- [ ] Blazor app starts locally
- [ ] Core UI flows function
- [ ] Integration tests that depend on Web pass

---

### Phase 4: Tests, AppHost and E2E

Projects:
- `tests/Web.Tests.Unit\Web.Tests.Unit.csproj` (net9.0 → net10.0)
- `tests/Web.Tests.Integration\Web.Tests.Integration.csproj` (net9.0 → net10.0)
- `tests/Shared.Tests.Unit\Shared.Tests.Unit.csproj` (net9.0 → net10.0)
- `tests/Architecture.Tests\Architecture.Tests.csproj` (net9.0 → net10.0)
- `src/AppHost\AppHost.csproj` (net9.0 → net10.0)
- `e2e/Web.Tests.Playwright\Web.Tests.Playwright.csproj` (net9.0 → net10.0)

Special package updates:
- `tests/Web.Tests.Integration`: `Microsoft.AspNetCore.Mvc.Testing` `9.0.10` → `10.0.0`

Migration Steps
1. Prerequisites: All previous phases complete.
2. Update test project TargetFrameworks to `net10.0`.
3. Update test packages (integration test hosting) per assessment.
4. Run unit, integration and e2e tests. Pay special attention to WebApplicationFactory changes with ASP.NET 10 hosting.

Validation Checklist
- [ ] All unit tests pass
- [ ] Integration tests pass (including ones using `WebApplicationFactory`)
- [ ] E2E Playwright tests pass

---

## 5. Package Update Reference (grouped by phase/tier)

Tier 1 (Phase 1 - `Shared`):
  - No package updates indicated in assessment

Tier 2 (Phase 2 - `ServiceDefaults`):
  - `Microsoft.Extensions.Http.Resilience`: `9.10.0` → `10.0.0` (affects networking/resilience behaviors)
  - `Microsoft.Extensions.ServiceDiscovery`: `9.5.2` → `10.0.0` (service discovery APIs)
  - `OpenTelemetry.Instrumentation.AspNetCore`: `1.13.0` → `1.14.0-rc.1` (telemetry instrumentation)
  - `OpenTelemetry.Instrumentation.Http`: `1.13.0` → `1.14.0-rc.1`
  - `OpenTelemetry.Extensions.Hosting`: `1.13.1` → `1.14.0-rc.1`
  - `OpenTelemetry.Exporter.OpenTelemetryProtocol`: `1.13.1` → `1.14.0-rc.1`
  - `OpenTelemetry.Instrumentation.Runtime`: `1.13.0` → `1.14.0-rc.1`
  - Notes: Update versions in `Directory.Package.props` (CPM). Ensure all related OpenTelemetry packages move together.

Tier 3 (Phase 3 - `Web`):
  - No explicit package updates in assessment. Verify ASP.NET/Core packages implicitly aligned by SDK.

Tier 4 (Phase 4 - Tests/AppHost/E2E):
  - `Microsoft.AspNetCore.Mvc.Testing`: `9.0.10` → `10.0.0` (affects integration testing)

---

## 6. Breaking Changes Catalog (expected / likely)

- OpenTelemetry 1.14.0-rc.1: Potential minor API surface changes, configuration option renames, or different defaults. Review `AddOpenTelemetryTracing` and exporter configuration.
- Microsoft.Extensions.* 10.0.0: Some extension methods and behavior might change; validate resilience and service discovery APIs and DI registration.
- Blazor (.NET 10 preview): minimal but watch for hosting startup differences; verify `Program.cs`/`Main` and component registration.
- Test hosting: `WebApplicationFactory` and test server behaviors may change in ASP.NET Core 10 preview; update integration tests accordingly.

(Full breaking-change discovery will occur during compilation and testing; update this section with exact errors found during execution.)

---

## 7. Testing and Validation Strategy

### 7.1 Phase-by-Phase Testing
- Phase 1 (Shared): Build and unit tests.
- Phase 2 (ServiceDefaults): Build, unit tests, integration tests that exercise telemetry and resilience.
- Phase 3 (Web): Build, run Blazor app locally, smoke tests, component tests.
- Phase 4 (Tests & AppHost/E2E): Run full unit, integration, and e2e tests.

### 7.2 Smoke Tests (after each project/tier)
- Build succeeds
- Key services start
- Minimal end-to-end scenario for Web: load home page, basic navigation

### 7.3 Comprehensive Validation (before marking tier complete)
- All tests pass
- No outstanding security vulnerabilities
- CI pipeline runs (if available) and succeeds with new SDK

---

## 8. Timeline and Effort Estimates (high-level)
- Phase 0 (Prepare repo + SDK validation): 0.5 - 1 day
- Phase 1 (Shared): 0.5 - 1 day
- Phase 2 (ServiceDefaults + package updates): 1 - 2 days (higher risk due to OpenTelemetry RC)
- Phase 3 (Web - Blazor): 1 - 2 days
- Phase 4 (Tests, AppHost, E2E): 1 - 2 days
- Buffer & stabilization: 1 - 2 days
- Total estimate: 5 - 10 working days depending on blocker discovery and team capacity

---

## 9. Source Control Strategy

- Starting branch: `wip` (current)
- Upgrade branch: `upgrade-to-NET10` (create from `wip`)
- Pending changes: commit them to `wip` before creating `upgrade-to-NET10` (or stash if preferred). Recommended commit message: `chore: commit pending work before NET10 upgrade`.
- Branching/PR: Create a single upgrade branch and open PR per-phase or one PR per major tier (recommended per-tier PRs to keep diffs manageable).
- Commit granularity: group changes per-tier (one commit for target framework updates across the tier; one commit for package updates in central props; separate commit for code fixes if needed).

Sample commands (executor):
- `git add -A && git commit -m "chore: save pending changes before NET10 upgrade"`
- `git checkout -b upgrade-to-NET10`

---

## 10. Risk Management

### 10.1 High-Risk Items
- OpenTelemetry `1.14.0-rc.1` updates (Tier 2) — risk: API changes and instability. Mitigation: run integration tests, consider using stable versions if available.
- Central Package Management changes — risk: unintended version bumps across projects. Mitigation: validate project list after change and run solution build.

| Project | Risk | Mitigation |
|--------:|------|-----------|
| `src/ServiceDefaults` | High (OpenTelemetry RC + multiple package upgrades) | Extra integration testing, code review, consider delaying RC if unstable |
| `src/Web` | Medium (Blazor app behavior changes) | Smoke tests, manual UI validation |
| Tests (integration/e2e) | Medium | Update test host packages, run CI locally if possible |

### 10.2 Contingency Plans
- If package RCs cause blockers: revert package changes (commit) and pin to previous stable versions; re-evaluate upgrade timeline.
- If compilation errors are large: revert branch or create temporary multi-targeting for consumer projects (NOT preferred under Bottom-Up strategy) as a stopgap.
- Use feature flags or conditional compilation to isolate behavior changes if necessary.

---

## 11. Success Criteria

- [ ] All projects target `net10.0` as proposed in assessment
- [ ] All package updates applied per assessment
- [ ] Solution builds without errors or warnings
- [ ] All automated tests pass (unit, integration, e2e)
- [ ] No security vulnerabilities remain in NuGet dependencies
- [ ] CI pipeline validates new SDK and packages

---

## 12. Actionable Next Steps (for executor)

Phase 0 (Execute before editing projects):
1. Commit or stash pending changes on `wip`.
2. Create and switch to branch `upgrade-to-NET10` from `wip`.
3. Ensure .NET 10 SDK is available on build agents / local dev machines (note: preview). Validate `global.json` if present.

Phase 1..N (per-tier, repeatable):
1. Update `TargetFramework` for all projects in the tier to `net10.0`.
2. Update package versions for the tier in `Directory.Package.props` (or project files if not using CPM).
3. Build solution and address compilation errors.
4. Run unit and integration tests scoped to the tier and its lower-tier dependencies.
5. Stabilize and commit changes, open PR for review.
6. Proceed to next tier only after acceptance.

---

## 13. Open Questions & Assumptions
- Assumption: All projects in assessment that listed `net9.0` or `net8.0` should target `net10.0`.
- Question: Prefer using `net10.0` preview now or target `net9.0` (STS) instead? (Recommendation: If production stability is required, prefer `net9.0` unless preview features are required.)
- Assumption: CPM file is at repository root `Directory.Package.props` — verify actual path before editing.

---

***End of plan.***

## 14. CI / Pipeline Changes (required)

- Validate CI agents can run `.NET 10` preview. If not, add a job step to install the preview SDK or pin to a self-hosted image that has it.
- Update pipeline YAML where the SDK version is explicitly set (search for `actions/setup-dotnet`, `UseDotNet@2`, or image names). Replace `9.x` entries with `10.0.0-preview` where appropriate, or add a conditional matrix for `net9.0`/`net10.0` during validation.
- Update test matrix to run unit and integration tests after package updates. Add an 'integration' job that runs `tests/Web.Tests.Integration` and `src/AppHost` host scenarios.
- If Central Package Management (`Directory.Packages.props`/`Directory.Package.props`) is changed, add a step to restore with the updated file and fail early if other projects resolve incompatible versions.
- Add pipeline gating: require successful build and test jobs on `upgrade-to-NET10` branch before merging.

Recommended pipeline verification commands (CI steps):
- `dotnet --info` (verify SDK)
- `dotnet restore --use-lock-file` (if using lock file)
- `dotnet build --configuration Release --no-restore`
- `dotnet test tests/Shared.Tests.Unit --no-build --verbosity minimal`
- `dotnet test tests/Web.Tests.Unit --no-build --verbosity minimal`
- `dotnet test tests/Web.Tests.Integration --no-build --verbosity normal`

## 15. global.json and SDK management

- If `global.json` exists, update it to reference the desired `dotnet` SDK version (preview build) so all contributors and CI use a consistent SDK.
  - Example: update `sdk.version` to `10.0.100-preview` (verify exact preview version available).
- If `global.json` does not exist and you want to lock the SDK for the upgrade branch, add one at the repository root.
- Document local SDK installation steps in the repo README or `.github/upgrades/tasks.md` so developers can install the required preview SDK.

## 16. Recommended git workflow / commands

- Prerequisite (on `wip`):
  - `git status` -> ensure no uncommitted changes
  - `git add -A && git commit -m "chore: save pending work before NET10 upgrade"`
- Create the upgrade branch:
  - `git checkout -b upgrade-to-NET10`
- Make changes in small commits (per-tier):
  - Commit 1: `chore: target net10.0 for src/Shared`
  - Commit 2: `chore: target net10.0 for src/ServiceDefaults`
  - Commit 3: `chore: update Directory.Package.props for OpenTelemetry and Microsoft.Extensions packages`
  - Commit 4: `chore: target net10.0 for src/Web and tests`
  - Commit 5: `fix: address build/test failures after upgrade`
- Push and open PR: `git push --set-upstream origin upgrade-to-NET10`

## 17. PR Checklist (what to verify before merge)

- [ ] All projects build successfully in CI with `net10.0` SDK
- [ ] Unit tests pass (all test projects)
- [ ] Integration tests pass (including `WebApplicationFactory` tests)
- [ ] E2E Playwright tests pass or are gated behind a manual check if environment is not available in CI
- [ ] Central Package Management updated and no unexpected version drift in `dotnet restore`
- [ ] `global.json` updated (if added/modified)
- [ ] `Directory.Package.props` changes documented in PR description
- [ ] Any code fixes or API changes are isolated and have explanatory comments
- [ ] Owners / reviewers from infra and platform teams have approved (if applicable)

## 18. Rollback Plan

- If upgrade introduces blocking regressions in CI or production, revert the upgrade branch with:
  - `git switch wip`
  - `git revert -m 1 <merge-commit-hash>` (if merge already happened) or reset branch and force push to remove upgrade branch
- For package-related issues, revert `Directory.Package.props` changes and restore previous package versions in a hotfix branch.
- For OpenTelemetry RC instability, revert to the latest stable 1.13.x versions and reopen a follow-up PR after re-evaluation.

## 19. Post-Upgrade Tasks

- Monitor application and telemetry for discrepancies introduced by new OpenTelemetry versions.
- Update documentation to reflect SDK changes and any runtime behavior modifications.
- File follow-up tasks for non-blocking deprecations or warnings that appeared during the upgrade.
- Schedule a short retrospective to capture lessons learned and any remaining technical debt.

## 20. Owners and Responsibilities

- Migration lead: assign an owner (example: `@team/platform`)
- Package/version reviewer: assign someone with NuGet/Central Package Management experience
- CI owner: responsible for pipeline changes and verifying agent SDKs
- QA owner: responsible for integration and e2e verification

## 21. Timeline & Milestones (example)

- Day 0: Prepare repo, install SDK, create branch
- Day 1: Upgrade `src/Shared` and validate
- Day 2-3: Upgrade `src/ServiceDefaults`, update `Directory.Package.props`, fix issues
- Day 4: Upgrade `src/Web`, run smoke tests
- Day 5: Upgrade tests/AppHost/e2e, run full test suite
- Day 6: Stabilization, CI fixes, PR review and merge

## 22. Appendix: Useful commands and troubleshooting notes

- Rebuild clean: `dotnet clean && dotnet restore && dotnet build -v minimal`
- Restore lock file (if used): `dotnet restore --use-lock-file`
- List SDKs installed: `dotnet --list-sdks`
- If build errors reference obsolete APIs: search for `Obsolete` attributes or deprecated extension method names and consult Microsoft docs for migration guidance.
- If OpenTelemetry API errors occur: align all OpenTelemetry package versions to the same major/minor (1.14.0-rc.1) and review migration notes on the OpenTelemetry .NET repo.


---

End of continued plan.
