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

mkdir -p "$TARGET_REPO/.squad/workflows"
mkdir -p "$TARGET_REPO/.squad/skills/git-workflow-standard"

copy_if_distinct() {
  local source_file="$1"
  local target_file="$2"

  if [[ -f "$target_file" ]] && [[ "$(realpath "$source_file")" == "$(realpath "$target_file")" ]]; then
    return
  fi

  if [[ -f "$target_file" ]] && cmp -s "$source_file" "$target_file"; then
    return
  fi

  cp "$source_file" "$target_file"
}

copy_if_distinct \
  "$SOURCE_REPO/.squad/workflows/git-gh-process-standard.md" \
  "$TARGET_REPO/.squad/workflows/git-gh-process-standard.md"

copy_if_distinct \
  "$SOURCE_REPO/.squad/workflows/README.md" \
  "$TARGET_REPO/.squad/workflows/README.md"

copy_if_distinct \
  "$SOURCE_REPO/.squad/skills/git-workflow-standard/SKILL.md" \
  "$TARGET_REPO/.squad/skills/git-workflow-standard/SKILL.md"

VERSION="$(grep -E '^Standard-Version:' "$SOURCE_REPO/.squad/workflows/git-gh-process-standard.md" | awk '{print $2}')"
echo "${VERSION:-unknown}" > "$TARGET_REPO/.squad/workflows/.git-gh-standard-version"

cat <<EOF
Synced git/gh process standard into:
  $TARGET_REPO/.squad/workflows/git-gh-process-standard.md
  $TARGET_REPO/.squad/workflows/README.md
  $TARGET_REPO/.squad/skills/git-workflow-standard/SKILL.md
  $TARGET_REPO/.squad/workflows/.git-gh-standard-version

Next step: run scripts/squad/check-git-gh-standard.sh $TARGET_REPO
EOF
