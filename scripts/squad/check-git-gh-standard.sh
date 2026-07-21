#!/usr/bin/env bash
set -euo pipefail

if [[ $# -ne 1 ]]; then
  echo "Usage: $0 /absolute/path/to/target-repo"
  exit 1
fi

SOURCE_REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
TARGET_REPO="$1"

if [[ ! -d "$TARGET_REPO/.git" ]]; then
  echo "Target repo is not a git repository: $TARGET_REPO"
  exit 1
fi

CANONICAL_VERSION="$(grep -E '^Standard-Version:' "$SOURCE_REPO/.squad/workflows/git-gh-process-standard.md" | awk '{print $2}')"
LOCAL_VERSION_FILE="$TARGET_REPO/.squad/workflows/.git-gh-standard-version"
LOCAL_VERSION="missing"
HAS_FAILURE=0
HAS_DRIFT=0

if [[ -f "$LOCAL_VERSION_FILE" ]]; then
  LOCAL_VERSION="$(tr -d '[:space:]' < "$LOCAL_VERSION_FILE")"
fi

echo "Canonical version: ${CANONICAL_VERSION:-unknown}"
echo "Local version:     ${LOCAL_VERSION}"

if [[ -z "${CANONICAL_VERSION}" || "${CANONICAL_VERSION}" == "unknown" ]]; then
  echo "ERROR: Canonical version not found."
  exit 2
fi

if [[ "${LOCAL_VERSION}" != "${CANONICAL_VERSION}" ]]; then
  HAS_FAILURE=1
  HAS_DRIFT=1
  echo "STATUS: DRIFT DETECTED"
  echo "Policy: detect-and-prompt before gated issue work."
  echo "Choose one:"
  echo "  1) Update now: scripts/squad/sync-git-gh-standard.sh $TARGET_REPO"
  echo "  2) Defer: continue now, but rerun this check before next gated work"
  if [[ -f "$TARGET_REPO/.squad/workflows/git-gh-process-standard.md" ]]; then
    echo "  3) View diff: diff -u \\"
    echo "       $TARGET_REPO/.squad/workflows/git-gh-process-standard.md \\"
    echo "       $SOURCE_REPO/.squad/workflows/git-gh-process-standard.md"
  else
    echo "  3) View diff: local canonical file missing; sync first"
  fi
fi

assert_file_contains() {
  local file="$1"
  local expected="$2"
  local message="$3"

  if [[ ! -f "$file" ]]; then
    HAS_FAILURE=1
    echo "ADAPTER CHECK FAILED: missing file $file"
    return
  fi

  if ! grep -Fq "$expected" "$file"; then
    HAS_FAILURE=1
    echo "ADAPTER CHECK FAILED: $message"
  fi
}

assert_file_contains \
  "$TARGET_REPO/.squad/routing.md" \
  ".squad/workflows/git-gh-process-standard.md" \
  ".squad/routing.md must reference canonical workflow source"
assert_file_contains \
  "$TARGET_REPO/.squad/routing.md" \
  ".squad/templates/issue-lifecycle.md" \
  ".squad/routing.md must bind issue lifecycle template"
assert_file_contains \
  "$TARGET_REPO/.squad/routing.md" \
  "single issue uses standard branch flow; 2+" \
  ".squad/routing.md must enforce standard-vs-worktree flow selection"
assert_file_contains \
  "$TARGET_REPO/.squad/routing.md" \
  "never push directly to \`main\` or \`dev\`" \
  ".squad/routing.md must hard-gate direct main/dev pushes"

assert_file_contains \
  "$TARGET_REPO/.squad/ceremonies.md" \
  ".squad/workflows/git-gh-process-standard.md" \
  ".squad/ceremonies.md must reference canonical workflow source"

assert_file_contains \
  "$TARGET_REPO/.squad/templates/issue-lifecycle.md" \
  "Workflow Standard Binding" \
  ".squad/templates/issue-lifecycle.md must include workflow standard binding section"
assert_file_contains \
  "$TARGET_REPO/.squad/templates/issue-lifecycle.md" \
  "Standard version: \`${CANONICAL_VERSION}\`" \
  ".squad/templates/issue-lifecycle.md must bind to canonical standard version"
assert_file_contains \
  "$TARGET_REPO/.squad/templates/issue-lifecycle.md" \
  "Enforcement level: hard gate" \
  ".squad/templates/issue-lifecycle.md must explicitly declare hard gate enforcement"
assert_file_contains \
  "$TARGET_REPO/.squad/templates/issue-lifecycle.md" \
  "Default branch policy: branch from \`main\`, PR to \`main\`" \
  ".squad/templates/issue-lifecycle.md must enforce main-first branch + PR policy"

assert_file_contains \
  "$TARGET_REPO/.squad/skills/git-workflow-standard/SKILL.md" \
  "Standard version: \`${CANONICAL_VERSION}\`" \
  ".squad/skills/git-workflow-standard/SKILL.md must match canonical standard version"

if [[ "$HAS_FAILURE" -eq 0 ]]; then
  echo "STATUS: OK (version and hard-gate adapters in sync)"
  exit 0
fi

echo "STATUS: ENFORCEMENT INCOMPLETE"
echo "Fix drift and adapter bindings, then rerun this check."
echo "Suggested action: scripts/squad/sync-git-gh-standard.sh $TARGET_REPO"
echo "Exit code map: 0=ok, 2=canonical missing, 3=drift, 4=adapter enforcement failure"
if [[ "$HAS_DRIFT" -eq 1 ]]; then
  exit 3
fi
exit 4
