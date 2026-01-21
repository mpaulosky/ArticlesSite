# Test Debugging Instructions Template

**Last updated:** January 20, 2026

This template provides standard instructions for debugging failing unit tests in the ArticlesSite solution. Use this template when investigating test failures to ensure consistent, correct debugging practices.

---

## 🔴 CRITICAL RULE: The Codebase is the Specification

When a unit test fails:
- **The production code/subject being tested is the SOURCE OF TRUTH**
- **Fix the test to match the implementation, NOT the reverse**
- Only modify production code if you verify the implementation is genuinely wrong

This prevents unnecessary refactoring and maintains the intended architecture.

---

## Debugging Decision Tree

```
Test Fails
    ↓
Is this an Architecture Test?
├─→ YES: Go to "Architecture Test Workflow" (below)
└─→ NO: Go to "Functional Test Workflow" (below)
```

---

## Architecture Test Workflow

Architecture tests validate project structure, organization, and conventions. The codebase IS the specification.

### Step 1: Examine the Test
- [ ] Read the test name and comments
- [ ] Understand what namespace/structure it expects
- [ ] Note any exclusions or special cases

### Step 2: Examine the Actual Code Structure
- [ ] Use `get_symbols_by_name` to find the class being tested
- [ ] Use `get_file` to inspect its actual namespace
- [ ] Check if there are multiple instances of the class
- [ ] Document the actual structure

### Step 3: Compare & Decide
Does the test expectation match the actual code structure?

- **YES** → Test should pass. Debug why it's failing (compilation issue, filtering issue, etc.)
- **NO** → Test is wrong. Update the test to match reality.

### Step 4: Update the Test
If updating the test:

```csharp
// BEFORE (incorrect expectation)
repository.Namespace.Should().Contain("Data",
    $"{repository.Name} should be in Data namespace");

// AFTER (matches actual structure)
repository.Namespace.Should().Contain("Repositories",
    $"{repository.Name} should be in Repositories namespace");
```

### Step 5: Verify
- [ ] Run the test under the debugger
- [ ] Confirm exit code is 0 (success)
- [ ] Check for no cascading failures in other tests
- [ ] Verify the change aligns with the Copilot Instructions

---

## Functional Test Workflow

Functional tests validate code behavior and logic. Use this workflow for non-architecture tests.

### Step 1: Understand the Test
- [ ] Read the test name and arrange/act/assert sections
- [ ] Identify what functionality is being tested
- [ ] Note any expected inputs/outputs

### Step 2: Debug the Subject Code
- [ ] Set a breakpoint at the failure point
- [ ] Launch the test under the debugger
- [ ] Inspect variable state and execution flow
- [ ] Compare actual vs. expected behavior

### Step 3: Identify the Issue
Does the code match the test expectation?

- **YES** → Test is wrong. Update test to match intended behavior.
- **NO** → Code has a bug. Fix the implementation.

### Step 4: Apply Fix
- If fixing code: implement the logic correction
- If fixing test: update expectations or mock setup

### Step 5: Verify
- [ ] Run the fixed test under the debugger
- [ ] Confirm exit code is 0 (success)
- [ ] Run related tests to ensure no regressions
- [ ] Follow code style from Copilot Instructions

---

## Pre-Fix Verification Checklist

**Use this checklist before modifying any production code:**

- [ ] I have examined the ACTUAL code in the subject files
- [ ] I have confirmed the test expectation contradicts the implementation
- [ ] I have NOT found evidence this is an intentional pattern in the codebase
- [ ] I have confirmed no other tests/features depend on the current structure
- [ ] I have verified this is a genuine bug, not a test problem

**Outcome:**
- If ≥4 boxes checked: Proceed to modify production code
- If <4 boxes checked: Update the test instead

---

## Common Mistakes to Avoid

### ❌ Mistake 1: Refactoring Code to Match Test
```csharp
// DON'T DO THIS:
// Test expects namespace "Data"
// So you move the file to Data namespace
// THIS IS BACKWARDS!

// DO THIS INSTEAD:
// Check where the file actually is
// Update the test to match that location
```

### ❌ Mistake 2: Ignoring Cascading Changes
When you move a file, update using statements:
```csharp
// If you must move code, also update:
- namespace declarations
- using statements in dependent files
- imports/references
- related configuration
```

### ❌ Mistake 3: Not Verifying the Fix
```csharp
// After making changes, ALWAYS:
debugger_launch_unit_test();        // ✅ Verify the test passes
// Check for build errors             // ✅ Ensure nothing broke
// Run related tests                   // ✅ Check for regressions
```

---

## Example Scenarios

### Scenario 1: Repository Namespace Test Fails

```
Test: Repositories_ShouldBeInDataNamespace
Error: Expected namespace "Web.Components.Features.Categories.Repositories" 
       to contain "Data"

Step 1: Test expects repositories in "Data" namespace
Step 2: Actual repositories are in "Repositories" namespace
Step 3: Mismatch detected → Test is wrong
Step 4: Update test to expect "Repositories"
Step 5: Verify test passes
```

### Scenario 2: Entity Validation Test Fails

```
Test: Entities_ShouldHaveParameterlessConstructor
Error: Entity "Article" does not have parameterless constructor

Step 1: Test expects parameterless constructor
Step 2: Article has only primary constructor
Step 3: Check Copilot Instructions - primary constructors are allowed
Step 4: Either:
        - Add parameterless constructor to Article, OR
        - Exclude Article from this test (if intentional)
Step 5: Verify fix and check dependencies
```

---

## Tools Reference

| Tool | When to Use | Example |
|------|------------|---------|
| `get_symbols_by_name` | Find a class definition | Find all `Repository` classes |
| `get_file` | Inspect current code structure | View actual namespace of a file |
| `code_search` | Find namespace/pattern usage | Search for "Repositories" references |
| `debugger_set_breakpoints` | Stop at a specific line | Debug test execution |
| `debugger_launch_unit_test` | Run test under debugger | Verify fix works |
| `replace_string_in_file` | Update using statements | Fix namespace imports |

---

## Copilot Instructions Reference

Before modifying production code, check the [Copilot Instructions](.github/copilot-instructions.md):

- **Architecture**: Vertical Slice Architecture (each feature has its own slice)
- **Repositories**: Should be in feature folders, typically named `Repositories`
- **Entities**: Should be sealed or abstract (with exceptions documented)
- **DTOs**: Should be in `Models` namespace
- **Naming**: Follow the established patterns in the codebase

If your change contradicts these instructions, you're probably modifying the wrong file.

---

## Quick Reference: When to Do What

| Situation | Action |
|-----------|--------|
| Architecture test fails, code structure doesn't match expectation | Update the test |
| Functional test fails, code has a bug | Fix the code |
| Test passes locally but fails in CI | Check file paths and case sensitivity |
| Multiple tests fail after one change | You likely broke a dependency |
| Test expects pattern not in Copilot Instructions | Update test to match actual pattern |
| Code violates Copilot Instructions | Fix the code structure |

---

## Workflow Diagram

```
Test Fails
    ↓
[Architecture Test?]
   ↙            ↘
 YES            NO
  ↓              ↓
Check actual    Debug code
code structure  behavior
  ↓              ↓
Does test       Found
match code?     bug?
 ↙   ↘          ↙   ↘
YES   NO       YES   NO
 ↓    ↓         ↓    ↓
Fix  Update   Fix  Update
code test     code test
 ↓    ↓         ↓    ↓
Verify Verify  Verify Verify
```

---

## Template Usage Instructions

1. **Copy this template** when investigating a test failure
2. **Follow the Decision Tree** at the top
3. **Use the appropriate workflow** (Architecture or Functional)
4. **Complete the verification checklist** before making changes
5. **Reference the examples** if you encounter similar scenarios
6. **Document your findings** for team knowledge

---

**Remember**: The codebase is the specification. Fix tests to match reality, not the reverse.
