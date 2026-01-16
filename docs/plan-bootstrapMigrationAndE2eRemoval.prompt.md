# PRD Plan: Migrate TailwindCSS to Bootstrap and Remove E2E Tests

Repository: mpaulosky/ArticlesSite (branch: upgrade-net10; default: main)
Project focus: src/Web (Blazor Web frontend)
Goal: Replace TailwindCSS with Bootstrap for styling and remove Playwright E2E tests, while preserving functionality and CI stability.

## 1) Current Frontend Styling Setup

- Tailwind configuration
  - Config file:
    - darkMode: 'class'
    - content targets typically include Razor components and pages (e.g., `./Components/**/*.razor`, `./Pages/**/*.razor`, plus `.html`)
    - Usually no plugins; no PostCSS config assumed
  - NPM deps/scripts:
    - dependencies: tailwindcss and a Tailwind CLI
    - scripts: build:css (input.css → app.css), watch:css (watch mode)
  - MSBuild integration:
    - Custom targets to run `npm install` and `npm run build:css` before build
  - Generated CSS
    - Input: a Tailwind entry (e.g., `input.css`) importing Tailwind layers and custom component classes via `@layer` and `@apply`
    - Output: compiled CSS (e.g., `wwwroot/css/app.css`) referenced by the app
  - App include
    - `_Host.cshtml`/`App.razor`/layout includes the compiled CSS link in the `<head>`
    - Theme initialization via localStorage + toggling `dark` class on `<html>`
- Where Tailwind classes appear
  - Razor components under `src/Web/Components/**/*.razor` and pages
  - Shared UI pieces (alerts, buttons, forms, nav) use Tailwind utilities and/or small custom classes (`@apply`)
  - Dark mode uses Tailwind’s `dark:` variant via `class="dark"` on the root element
- CI references to Tailwind
  - CI may install Node and run Tailwind build prior to build/tests

## 2) E2E Testing Footprint

- Code and assets to remove
  - Entire `e2e/Web.Tests.Playwright` project (Fixtures/, PageObjects/, tests/, package.json, csproj, docs/, test-results/, etc.)
  - `scripts/start-apphost-and-run-e2e.ps1`
  - Any dedicated CI workflow for Playwright (if present)
- Solution/Project references
  - Remove solution entries for the E2E project
  - Remove any `InternalsVisibleTo` references targeting the E2E project
- Docs/README references
  - Update or remove E2E-related documentation sections and examples

## 3) Bootstrap Migration Touchpoints

- Files likely to change
  - App host/layout: `App.razor`, main layout(s), and nav components to wire Bootstrap CSS/JS and convert structure
  - Shared components: alerts, buttons, forms, validation states
  - Pages: update Tailwind utility classes to Bootstrap equivalents
  - Theme toggling component: migrate from Tailwind `dark` class to Bootstrap color modes (`data-bs-theme`) or custom approach
  - Static assets: retire `tailwind.config.js`, Tailwind entry CSS, and compiled Tailwind CSS; introduce Bootstrap and minimal overrides
  - Project files: remove Tailwind MSBuild targets from `Web.csproj`; remove Tailwind deps/scripts from `package.json`
- How to include Bootstrap (choose one)
  - CDN
    - Pros: simplest, no build tooling
    - Cons: CSP/CDN availability; offline dev
  - LibMan (wwwroot/lib/bootstrap)
    - Pros: no Node; versioned assets in repo
    - Cons: extra manifest/maintenance
  - npm + bundler (esbuild/Vite/webpack)
    - Pros: control, tree-shaking
    - Cons: new tooling overhead
- Conversion strategy (high level)
  - Layout/grid: Tailwind flex/grid → Bootstrap grid (`.container/.row/.col-*`) and utilities (`.d-flex`, `.gap-*`, `.g-*`)
  - Spacing: Tailwind `m-*/p-*` scales → Bootstrap `m*/p*` scales (0–5); add custom CSS where finer control is needed
  - Typography: Tailwind `text-*`, `font-*` → Bootstrap headings/utilities (`.h1–.h6`, `.fs-*`, `.fw-*`)
  - Colors: Tailwind `bg-*`/`text-*` → Bootstrap `bg-*`/`text-*` plus variables
  - Buttons: Tailwind custom `.btn-*` → Bootstrap `.btn` + variants (`.btn-primary`, `.btn-secondary`, etc.)
  - Alerts: Tailwind blocks → `.alert .alert-*`
  - Forms/validation: `.form-control`, `.form-label`, `.is-invalid` + `.invalid-feedback`
  - Navbar: `.navbar`, `.navbar-brand`, `.navbar-nav`, `.nav-link` and `bootstrap.bundle.js`
  - Dark mode: use Bootstrap 5.3 color modes (`data-bs-theme="dark"`/`light`), update toggle logic
  - Custom `@apply` utilities: replicate via Bootstrap utilities or minimal overrides stylesheet

## 4) Risks and Compatibility

- Styling/layout regressions
  - Breakpoint differences and container widths
  - Spacing scale mismatches may alter density; mitigate with small override CSS
- Dark mode parity
  - Tailwind variants vs Bootstrap color modes; rework custom `@apply` that relied on `.dark`
- Hidden Tailwind features
  - `group-hover`, `peer`, arbitrary values may not map directly; use Bootstrap utilities or lightweight custom CSS/JS
- Build/CI impacts
  - Remove Tailwind CLI steps (MSBuild and CI); ensure no orphaned npm steps or missing static asset paths
- Performance
  - Bootstrap bundle may be larger than purged Tailwind; consider minimal import or CDN with integrity

## 5) Testing Strategy Post-Migration

- What remains
  - Unit tests (Shared, Web), Integration tests (Web), Architecture tests
- Updates needed
  - Update bUnit/component tests that assert class names to expect Bootstrap classes
  - Add minimal component smoke tests: layout renders `.navbar` and `.container`; forms render `.form-control` and show `.invalid-feedback` on error
- Without E2E
  - Remove Playwright projects/workflows; rely on integration tests for routing/endpoints
  - Use bUnit/component-level checks for critical UI flows

## 6) PRD Skeleton

- Objectives
  - Replace TailwindCSS with Bootstrap 5.x in `src/Web`
  - Remove all E2E tests and related CI/scripts
  - Preserve functionality, responsiveness, and dark mode support
- Scope
  - In-scope: `src/Web` styling migration; theme toggle adaptation; removal of E2E project/workflows/scripts; CI and docs updates
  - Out-of-scope: visual redesign beyond parity; new JS bundler unless chosen; new components beyond necessary replacements
- Functional Requirements
  - Bootstrap CSS/JS is loaded; Tailwind fully removed
  - All pages/components render with Bootstrap utilities/components with comparable layout
  - Dark mode via Bootstrap color modes or equivalent
  - Forms/validation use Bootstrap patterns
- Non-functional Requirements
  - CI passes without Tailwind/Node steps (unless npm approach chosen)
  - No 404s for static assets; acceptable CSS size/load times
  - Docs reflect Bootstrap usage and E2E removal
- Acceptance Criteria (repo-specific examples)
  - App no longer references `css/app.css` produced by Tailwind; Bootstrap assets are present and load
  - `tailwind.config.js`, Tailwind deps, and Tailwind MSBuild targets are removed
  - Main pages (Home, etc.) render with Bootstrap layout; navbar collapses on small screens; footer persists
  - Buttons, alerts, and forms use Bootstrap classes; invalid inputs show `.is-invalid` with `.invalid-feedback`
  - Theme toggle sets `data-bs-theme` and switches color theme app-wide
  - `e2e/Web.Tests.Playwright` removed; `scripts/start-apphost-and-run-e2e.ps1` deleted; solution and `InternalsVisibleTo` references cleaned
  - CI has no Tailwind build steps; pipeline is green
  - Updated bUnit/component tests pass with Bootstrap assertions
- Migration Plan
  - Introduce Bootstrap (CDN or LibMan), wire CSS/JS alongside Tailwind temporarily
  - Convert layout and shared components (MainLayout, Nav, Footer, alerts, buttons)
  - Convert pages and forms; implement Bootstrap validation; adapt theme toggle
  - Remove Tailwind assets/config and Node steps; clean up CI and docs
  - Update tests; remove E2E project/workflows; final CI verification
- Rollout/Release
  - Work on feature branch; incremental commits; PR with CI gating
  - Manual smoke across key pages and breakpoints; verify dark/light
  - Tag release with migration notes
- Risks/Mitigations
  - Visual drift: incremental PRs and visual checks per page
  - Dark mode mismatch: adopt Bootstrap 5.3 color modes; custom CSS as needed
  - JS dependency: ensure `bootstrap.bundle.js` is included and CSP aligned
- Out-of-scope E2E removal tasks (explicit)
  - Delete `e2e/Web.Tests.Playwright` and references (solution, InternalsVisibleTo)
  - Remove `scripts/start-apphost-and-run-e2e.ps1`
  - Remove E2E sections from README and docs or mark as deprecated

Open questions for refinement:

- Bootstrap delivery preference: CDN vs LibMan vs npm+bundler?
- Dark mode approach: Bootstrap color modes (`data-bs-theme`) or small override CSS to mimic current theme?
- Visual parity tolerance: strict parity (with small overrides) vs allow minor drift for simplicity?
