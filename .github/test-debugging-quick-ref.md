# Test Debugging Quick Reference

**Location in Workspace:** `.github/test-debugging-template.md`

**Quick Links:**
- 🔧 [Full Debugging Template](./test-debugging-template.md)
- 📋 [Copilot Instructions](./copilot-instructions.md#testing--debugging-required)

---

## When You See a Failing Test

### Step 1: Identify Test Type
- **Architecture Test** (checks structure/organization) → Codebase is correct
- **Functional Test** (checks behavior) → Codebase may have a bug

### Step 2: Make Your Decision
```
Architecture Test Fails?
├─ YES: Check if code structure matches test expectation
│        └─ NO MATCH? → Update the test to match reality ✅
│        └─ MATCHES? → Debug why test is failing
│
└─ NO: Check if code behavior matches test expectation  
         └─ MISMATCH? → Fix the code logic ✅
         └─ MATCHES? → Update test expectations
```

---

## The Golden Rule

> **The production code IS the specification.** Fix tests to match code, not the reverse.

---

## Before Touching Production Code

- [ ] Examined actual code structure
- [ ] Confirmed test expectation contradicts implementation
- [ ] Verified this is intentional code, not a bug
- [ ] Checked no other tests depend on current structure

**Result:**
- ✅ 4+ boxes checked? Fix the code
- ⚠️ <4 boxes checked? Fix the test instead

---

## Common Patterns

| Test Failure | Cause | Fix |
|---|---|---|
| Architecture test expects "Data" namespace | Code is in "Repositories" namespace | Update test expectation |
| Entity test expects parameterless constructor | Entity uses primary constructor | Update test or add exclusion |
| Functional test fails on logic | Code has a bug | Fix the code |
| Multiple tests fail after change | Cascading dependency issue | Check all affected files |

---

## Tools You'll Use

```powershell
# Find a class
get_symbols_by_name "ClassName"

# View actual namespace
get_file "src/path/to/file.cs" 1 50

# Find all usages
code_search "namespace or pattern"

# Debug the test
debugger_launch_unit_test()
```

---

## Need the Full Workflow?

👉 Read **[Test Debugging Instructions](./test-debugging-template.md)**

- Detailed Architecture Test Workflow
- Detailed Functional Test Workflow
- Example scenarios with real examples
- Decision trees and diagrams
