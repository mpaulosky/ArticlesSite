## Plan: Fix failing tests and unblock CI ✅

**TL;DR** — The test run shows a single blocker: `src/Web` fails to build (≈210 compile errors). Fix those compile errors (missing packages, usings, ambiguous types) first to unblock `Web.*` and `Architecture` tests, then iterate: reproduce each failing test, diagnose, fix code or test, and re-run until all pass.

---

### Steps 🔧

1. Run full tests and capture failures — `dotnet test` across `tests/` (gather failure messages & stack traces).
2. Fix `src/Web` build errors (add missing package refs, add usings, disambiguate DTO types) so `dotnet build src/Web/Web.csproj` is green.
3. Run `tests/Web.Tests.Unit` and `tests/Architecture.Tests`, reproduce each failing test, and document failure details (`test name`, `file`, `stack trace`).
4. For each failing test: reproduce locally, inspect test + production code (`symbol` and `file` references), apply fix (code or test), and re-run the single test until it passes.
5. Run integration tests (`tests/Web.Tests.Integration`) with Docker/Testcontainers running; fix infra/timeouts or test assumptions.
6. Run full test suite and CI; create follow-up tasks for flaky or skipped tests and add regression tests as needed.

---

### Further Considerations 💡

- Option A — Prioritize compilation fixes first (recommended). Option B — Triage critical test failures only if time-boxed.
- Integration tests need Docker running and possible local secrets; ensure env readiness before running them.
- For ambiguous DTOs, prefer using `Shared.Models` DTOs or renaming/qualifying Web-local types to avoid future drift.

---

Would you like me to:
- (A) Produce a targeted list of exact `src/Web` edits (packages/usings & ambiguous types) to make it build, or
- (B) Continue and attempt a safe sequence of edits and re-run tests to get to green?

Please pick A or B (or give different next steps).
