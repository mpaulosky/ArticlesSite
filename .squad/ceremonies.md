# Squad Ceremonies

## Defined Ceremonies

### Pre-Sprint Planning

- **Trigger:** manual ("run sprint planning", "plan the sprint")
- **When:** before
- **Facilitator:** Aragorn
- **Participants:** Aragorn, Sam, Legolas, Gimli, Boromir
- **Purpose:** Review open issues, prioritize, assign squad labels

### Build Repair Check

- **Trigger:** automatic, when: "before push" or "before PR"
- **When:** before
- **Facilitator:** Aragorn
- **Participants:** Aragorn (runs build-repair prompt)
- **Purpose:** Ensure zero errors, zero warnings, all tests pass before pushing

### Retro

- **Trigger:** manual ("run retro", "retrospective")
- **When:** after
- **Facilitator:** Aragorn
- **Participants:** all
- **Purpose:** What went well, what didn't, action items

### Code Review

- **Trigger:** automatic, when PR is opened
- **When:** after
- **Facilitator:** Aragorn
- **Participants:** Aragorn (reviewer), original author (locked out of their own revision if rejected)
- **Purpose:** Quality gate before merge

### Standard Task Workflow

- **Trigger:** When starting any new task or issue
- **When:** throughout (setup → planning → implementation → review → cleanup)
- **Facilitator:** Agent or human working the task
- **Participants:** Task owner, reviewers (for PR phase)
- **Purpose:** Ensure consistent task execution with proper branch isolation and verification
- **Enforcement:** The pre-push hook (Gate 0) blocks direct pushes to `main` and `dev` — you must use a `squad/{issue}-{slug}` feature branch
- **Source of truth:** `.squad/workflows/git-gh-process-standard.md` (version `2026.07.1`)

#### Phases

##### Phase 1: Setup

1. Choose setup mode:
   - **Standard flow** (single issue / one-off work)
   - **Worktree flow** (plans or 2+ concurrent issues)

2. Sync base branch:

   ```bash
   git checkout main
   git pull origin main
   ```

3. Create branch (standard flow):

   ```bash
   git checkout -b squad/{issue-number}-{kebab-slug}
   git push -u origin squad/{issue-number}-{kebab-slug}
   ```

4. Create isolated worktree (worktree flow):

   ```bash
   git fetch origin main
   git worktree add ../{repo-name}-{issue-number} \
     -b squad/{issue-number}-{kebab-slug} origin/main
   cd ../{repo-name}-{issue-number}
   git push -u origin squad/{issue-number}-{kebab-slug}
   ```

5. If branch falls behind base during work:

   ```bash
   git fetch origin main
   git merge origin/main
   ```

##### Phase 2: Planning

1. Analyze the problem
2. Document approach (in session plan, issue, or PR description)
3. Get user/stakeholder approval before implementing

##### Phase 3: Implementation

1. Make changes in the branch
2. Test locally
3. Iterate until complete
4. Commit and push — all pre-push gates must pass:
   - Copyright headers
   - Code formatting (`dotnet format`)
   - Shared.Tests.Unit
   - Web.Tests.Unit
   - Architecture.Tests
   - Web.Tests.Integration
5. If pre-push fails, fix issues and retry
6. Test command: `dotnet test tests/Shared.Tests.Unit tests/Web.Tests.Unit tests/Architecture.Tests`

##### Phase 4: Review & Merge

1. Create PR:

   ```bash
   gh pr create --title "..." --body "Closes #{issue-number}" --base main
   ```

2. Wait for CI checks to pass
3. Address any review comments
4. Once approved and green, merge (prefer squash merge for clean history)

##### Phase 5: Cleanup

1. Standard flow cleanup:

   ```bash
   git checkout main
   git pull origin main
   git branch -d squad/{issue-number}-{kebab-slug}
   git push origin --delete squad/{issue-number}-{kebab-slug}
   ```

2. Worktree flow cleanup:

   ```bash
   git worktree remove ../{repo-name}-{issue-number}
   git worktree prune
   git branch -d squad/{issue-number}-{kebab-slug}
   git push origin --delete squad/{issue-number}-{kebab-slug}
   ```

### Integration Points

- **Build Repair Check:** Enforced via pre-push hook (Phase 3, step 4)
- **Code Review:** Triggered when PR is opened (Phase 4, step 2-3)
- **Merged-PR Branch Guard:** Check before committing to avoid stranded commits
