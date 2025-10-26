---
mode: 'agent'
description: 'Universal .NET Solution Build & Error Resolution and warnings'
---

# Copilot Build Prompt: Universal .NET Solution Build & Error Resolution

## Instructions

1. **Locate Solution File**
  - Check for `*.slnx` file in current directory
  - If found, continue to next step
  - If not found, run: `cd ..` and check again
  - Repeat until `*.slnx` file is found

2. **Restore Dependencies**
  - Run: `dotnet restore`

3. **Build Solution**
  - Run: `dotnet build --no-restore`
  - Capture all build output, including errors and warnings.

4. **Error & Warning Resolution**
  - For each error or warning in the build output:
    - Identify the affected file and line number.
    - Research the error/warning code and message.
    - Apply the recommended fix to the codebase.
    - Rebuild the solution to verify the fix.
    - Repeat until the build completes with zero errors and warnings.

5. **Verification**
  - Ensure the final build output shows “Build succeeded” and no warnings.
  - Document any changes made to resolve issues.

## Notes

- These steps apply to any .NET solution (`*.slnx`).
- Use PowerShell or your default shell for commands.
- Always re-run the build after each fix to confirm resolution.

---

**Use this prompt to automate building any .NET solution and iteratively resolve all build errors and warnings until the
build is clean.**
