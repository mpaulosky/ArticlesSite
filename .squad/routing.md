# Squad Routing

## Signal → Agent

| Signal | Agent | Notes |
|--------|-------|-------|
| Architecture, scope, decisions, code review, PR review | Aragorn | Lead |
| Blazor, Razor, UI, frontend, components, CSS | Legolas | Frontend |
| MongoDB, repositories, API endpoints, backend services, MediatR handlers | Sam | Backend |
| Tests, quality, edge cases, test failures, test review | Gimli | Tester |
| CI/CD, GitHub Actions, NuGet, deployment, Aspire infra, protected branch, pipelines, builds | Boromir | CI/CD Expert |
| Docs, README, XML docs, comments, CONTRIBUTING | Frodo | Docs |
| Blog posts, article content, content strategy, SEO, article features, content workflows | Pippin | Blog Post Expert |
| Auth0, authentication, authorization, JWT, RBAC, security audit, vulnerabilities, injection, XSS, CSRF, secrets, HTTPS, CORS, security headers, security review | Gandalf | Security |
| GitHub board, issues, PRs, backlog, work queue | Ralph | Work Monitor |
| Untriaged issues (squad label, no squad:* sub-label) | Aragorn | Lead triages |
| squad:aragorn | Aragorn | — |
| squad:legolas | Legolas | — |
| squad:sam | Sam | — |
| squad:gimli | Gimli | — |
| squad:boromir | Boromir | — |
| squad:frodo | Frodo | — |
| squad:pippin | Pippin | — |
| squad:gandalf | Gandalf | — |
| squad:copilot | @copilot | Auto-assign: false |

## Branching Policy

- Squad work branches: `squad/{issue-number}-{slug}` — exempt from Protected Branch Guard
- NEVER commit `.squad/` files on `feature/*` branches — guard will block the PR
- Scribe commits `.squad/` changes on `squad/*` branches only
- Canonical git/gh standard: `.squad/workflows/git-gh-process-standard.md`

## Rules

1. **Protected branch rule** — never push directly to `main` or `dev`. All
   file-producing work uses `squad/{issue-number}-{slug}` branches.
2. **Issue lifecycle rule** — issue-linked work MUST follow
   `.squad/templates/issue-lifecycle.md`, which is aligned to
   `.squad/workflows/git-gh-process-standard.md`.
3. **PR review gate** — no merge without an explicit PR review approval.
4. **Pre-push gate** — required pre-push checks must pass before push.
5. **Cleanup gate** — after merge, delete the feature branch; if worktree mode
   was used, remove and prune the worktree.
6. **Flow selection rule** — single issue uses standard branch flow; 2+
   concurrent issues use worktree flow.
7. **Drift rule** — if local workflow standard version differs from canonical,
   prompt user to apply updates before continuing gated work.

## Skill-Aware Routing

Before spawning any agent, check `.squad/skills/` for relevant skills:

- Any issue lifecycle work → `.squad/skills/git-workflow-standard/SKILL.md`
- Any push/commit work → `.squad/skills/pre-push-test-gate/SKILL.md`
- Any build/test failure → `.github/prompts/build-repair.prompt.md`
- Any integration test work → `.squad/skills/pre-push-test-gate/SKILL.md` (Integration section)
