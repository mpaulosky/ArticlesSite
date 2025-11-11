# Migration Plan: Newtonsoft.Json to System.Text.Json

## 1. Executive Summary

**Scenario**: Migrate all usages of `Newtonsoft.Json` to `System.Text.Json` in the solution, ensuring compatibility and leveraging modern .NET serialization features.

**Scope**:  
- All projects referencing `Newtonsoft.Json`, including `Shared`, `ServiceDefaults`, `Web`, test projects, and supporting tools.
- Projects currently target `.NET 10`.
- Includes a Blazor project; Blazor-specific serialization patterns will be prioritized.

**Target State**:  
- All serialization and deserialization logic uses `System.Text.Json`.
- No references to `Newtonsoft.Json` remain.
- All projects build and pass tests.
- Serialization behavior is validated and documented.

**Selected Strategy**:  
- **Bottom-Up Strategy**: Projects are migrated tier-by-tier, starting from leaf nodes (libraries with no dependencies) and progressing upward to applications and test projects.

**Complexity Assessment**:  
- **Medium**: Multiple projects, some with advanced serialization logic and custom converters.  
- **Critical Issues**:  
  - Potential breaking changes in serialization behavior.
  - Custom converters and settings may require manual migration.
  - Test coverage must be validated.

**Recommended Approach**:  
- **Incremental Migration**: Ensures stability and allows for phased validation.

---

## 2. Migration Strategy

### 2.1 Approach Selection

- **Chosen Strategy**: Bottom-Up Strategy
- **Rationale**:  
  - Multiple projects with clear dependency hierarchy.
  - Allows staged migration and validation.
  - Reduces risk of breaking changes propagating upward.

- **Strategy-Specific Considerations**:  
  - Each tier is validated before proceeding.
  - Package updates and code changes are batched per tier.

### 2.2 Dependency-Based Ordering

- Migration order is dictated by project dependencies:
  - Leaf libraries first (no internal dependencies).
  - Mid-tier libraries next (depend on leaf libraries).
  - Applications and entry points last.
  - Test projects after all main code is migrated.

### 2.3 Parallel vs Sequential Execution

- Projects within the same tier can be migrated in parallel.
- Tiers must be migrated sequentially.
- Testing and validation are performed after each tier.

---

## 3. Detailed Dependency Analysis

### 3.1 Dependency Graph Summary

```
Tier 4: [Web] [AppHost]
         ↓      ↓
Tier 3: [ServiceDefaults]
         ↓
Tier 2: [Shared]
         ↓
Tier 1: [No internal dependencies]
```
Test projects depend on main projects and are migrated last.

### 3.2 Project Groupings

- **Phase 1 (Tier 1)**: Foundation libraries (leaf nodes)
  - `Shared`
- **Phase 2 (Tier 2)**: Libraries depending on Tier 1
  - `ServiceDefaults`
- **Phase 3 (Tier 3)**: Applications
  - `Web`, `AppHost`
- **Phase 4 (Tier 4)**: Test projects
  - `Web.Tests.Unit`, `Web.Tests.Integration`, `Shared.Tests.Unit`, `Architecture.Tests`, `Web.Tests.Playwright`

---

## 4. Project-by-Project Migration Plans

### Project: Shared

**Current State**
- Dependencies: None (leaf node)
- Dependants: ServiceDefaults, Web, AppHost, test projects
- Package Count: 1 (`Newtonsoft.Json`)
- LOC: [from assessment]

**Target State**
- Target Framework: net10.0
- Updated Packages: Remove `Newtonsoft.Json`, add `System.Text.Json`

**Migration Steps**
1. **Prerequisites**
   - None; can start immediately.

2. **Framework Update**
   - No change needed (already net10.0).

3. **Package Updates**
   | Package           | Current Version | Target Version | Reason                        |
   |-------------------|----------------|---------------|-------------------------------|
   | Newtonsoft.Json   | [current]      | Remove        | Migrate to System.Text.Json   |
   | System.Text.Json  | N/A            | [latest]      | Required for serialization    |

4. **Expected Breaking Changes**
   - Differences in default property naming (PascalCase vs camelCase).
   - Handling of nulls and missing members.
   - Custom converters and settings may need rewriting.
   - Attributes (`JsonProperty`, etc.) must be replaced with `System.Text.Json` equivalents.

5. **Code Modifications**
   - Replace all usages of `JsonConvert`, `JsonSerializer`, and related attributes.
   - Update custom converters to inherit from `System.Text.Json` types.
   - Review and update serialization settings.

6. **Testing Strategy**
   - Run all unit tests.
   - Validate serialization/deserialization of key models.
   - Manual review of edge cases.

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass
   - [ ] No security warnings

---

### Project: ServiceDefaults

**Current State**
- Dependencies: Shared
- Dependants: Web, AppHost, test projects
- Package Count: 1 (`Newtonsoft.Json`)
- LOC: [from assessment]

**Target State**
- Target Framework: net10.0
- Updated Packages: Remove `Newtonsoft.Json`, add `System.Text.Json`

**Migration Steps**
1. **Prerequisites**
   - Shared project migration complete.

2. **Framework Update**
   - No change needed.

3. **Package Updates**
   | Package           | Current Version | Target Version | Reason                        |
   |-------------------|----------------|---------------|-------------------------------|
   | Newtonsoft.Json   | [current]      | Remove        | Migrate to System.Text.Json   |
   | System.Text.Json  | N/A            | [latest]      | Required for serialization    |

4. **Expected Breaking Changes**
   - Same as Shared; review for custom serialization logic.

5. **Code Modifications**
   - Replace usages of `Newtonsoft.Json` APIs.
   - Update attributes and settings.

6. **Testing Strategy**
   - Run all unit tests.
   - Validate integration with Shared.

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit tests pass
   - [ ] No security warnings

---

### Project: Web

**Current State**
- Dependencies: Shared, ServiceDefaults
- Dependants: Test projects
- Package Count: 1 (`Newtonsoft.Json`)
- LOC: [from assessment]

**Target State**
- Target Framework: net10.0
- Updated Packages: Remove `Newtonsoft.Json`, add `System.Text.Json`

**Migration Steps**
1. **Prerequisites**
   - Shared and ServiceDefaults migration complete.

2. **Framework Update**
   - No change needed.

3. **Package Updates**
   | Package           | Current Version | Target Version | Reason                        |
   |-------------------|----------------|---------------|-------------------------------|
   | Newtonsoft.Json   | [current]      | Remove        | Migrate to System.Text.Json   |
   | System.Text.Json  | N/A            | [latest]      | Required for serialization    |

4. **Expected Breaking Changes**
   - Blazor-specific serialization patterns.
   - API controller serialization behavior.
   - Custom converters and settings.

5. **Code Modifications**
   - Replace usages of `Newtonsoft.Json` APIs.
   - Update attributes and settings.
   - Review Blazor serialization logic.

6. **Testing Strategy**
   - Run all unit and integration tests.
   - Validate Blazor component serialization.
   - Manual review of API responses.

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit and integration tests pass
   - [ ] No security warnings

---

### Project: AppHost

**Current State**
- Dependencies: Shared, ServiceDefaults
- Dependants: Test projects
- Package Count: 1 (`Newtonsoft.Json`)
- LOC: [from assessment]

**Target State**
- Target Framework: net10.0
- Updated Packages: Remove `Newtonsoft.Json`, add `System.Text.Json`

**Migration Steps**
1. **Prerequisites**
   - Shared and ServiceDefaults migration complete.

2. **Framework Update**
   - No change needed.

3. **Package Updates**
   | Package           | Current Version | Target Version | Reason                        |
   |-------------------|----------------|---------------|-------------------------------|
   | Newtonsoft.Json   | [current]      | Remove        | Migrate to System.Text.Json   |
   | System.Text.Json  | N/A            | [latest]      | Required for serialization    |

4. **Expected Breaking Changes**
   - Review for custom serialization logic.

5. **Code Modifications**
   - Replace usages of `Newtonsoft.Json` APIs.
   - Update attributes and settings.

6. **Testing Strategy**
   - Run all unit and integration tests.
   - Validate application startup and core workflows.

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit and integration tests pass
   - [ ] No security warnings

---

### Project: Test Projects

**Projects**:  
- Web.Tests.Unit  
- Web.Tests.Integration  
- Shared.Tests.Unit  
- Architecture.Tests  
- Web.Tests.Playwright

**Current State**
- Dependencies: Main projects (Shared, ServiceDefaults, Web, AppHost)
- Package Count: 1 (`Newtonsoft.Json`)
- LOC: [from assessment]

**Target State**
- Target Framework: net10.0
- Updated Packages: Remove `Newtonsoft.Json`, add `System.Text.Json`

**Migration Steps**
1. **Prerequisites**
   - All main projects migrated.

2. **Framework Update**
   - No change needed.

3. **Package Updates**
   | Package           | Current Version | Target Version | Reason                        |
   |-------------------|----------------|---------------|-------------------------------|
   | Newtonsoft.Json   | [current]      | Remove        | Migrate to System.Text.Json   |
   | System.Text.Json  | N/A            | [latest]      | Required for serialization    |

4. **Expected Breaking Changes**
   - Test data serialization/deserialization may change.
   - Update test helpers and mocks.

5. **Code Modifications**
   - Replace usages of `Newtonsoft.Json` APIs.
   - Update test data and helpers.

6. **Testing Strategy**
   - Run all unit and integration tests.
   - Validate test coverage and results.

7. **Validation Checklist**
   - [ ] Dependencies resolve correctly
   - [ ] Project builds without errors
   - [ ] Project builds without warnings
   - [ ] All unit and integration tests pass
   - [ ] No security warnings

---

## 5. Risk Management

### 5.1 High-Risk Changes

| Project      | Risk Description                                 | Mitigation                        |
|--------------|--------------------------------------------------|-----------------------------------|
| Shared       | Custom converters/settings may break             | Manual migration, extra testing   |
| Web          | Blazor serialization, API response changes       | Validate with integration tests   |
| Test Projects| Test data compatibility                          | Update helpers, review coverage   |

### 5.2 Contingency Plans

- If breaking changes block migration, consider:
  - Isolating problematic code and migrating incrementally.
  - Retaining `Newtonsoft.Json` for legacy scenarios (temporary).
  - Adding custom converters to bridge gaps.

---

## 6. Testing and Validation Strategy

### 6.1 Phase-by-Phase Testing

- After each tier migration:
  - Run all unit and integration tests.
  - Validate serialization logic.
  - Manual review of critical workflows.

### 6.2 Smoke Tests

- Build succeeds.
- Tests pass.
- Application starts.
- Core functionality works.

### 6.3 Comprehensive Validation

- All automated tests pass.
- No new warnings or errors.
- Performance metrics within acceptable range.
- Security scan clean.

---

## 7. Timeline and Effort Estimates

| Project      | Complexity | Estimated Time | Dependencies      | Risk Level |
|--------------|------------|---------------|-------------------|------------|
| Shared       | Medium     | 1 day         | None              | Medium     |
| ServiceDefaults| Medium   | 1 day         | Shared            | Medium     |
| Web          | High       | 2 days        | Shared, ServiceDefaults | High      |
| AppHost      | Medium     | 1 day         | Shared, ServiceDefaults | Medium     |
| Test Projects| Medium     | 1 day         | All main projects | Medium     |

- **Phase Durations**:  
  - Phase 1: 1 day  
  - Phase 2: 1 day  
  - Phase 3: 2 days  
  - Phase 4: 1 day  
  - **Total**: ~5 days (plus buffer for testing and review)

- **Resource Requirements**:
  - Developers familiar with serialization and .NET
  - Testers for validation
  - Code reviewers

---

## 8. Source Control Strategy

### 8.1 Branching Strategy

- Main upgrade branch: `upgrade-to-NET10`
- Feature branches per tier (optional)
- Merge after each tier migration and validation

### 8.2 Commit Strategy

- Commit after each completed tier
- Commit message format:  
  `Migrate [Tier/Project] from Newtonsoft.Json to System.Text.Json`
- Create checkpoints after major changes

### 8.3 Review and Merge Process

- PR required for each tier
- Review checklist:
  - All code changes documented
  - Tests pass
  - No warnings/errors
- Merge after validation

---

## 9. Success Criteria

### 9.1 Strategy-Specific Success Criteria

- All projects migrated in bottom-up order
- No references to `Newtonsoft.Json` remain
- All serialization logic uses `System.Text.Json`
- All builds and tests succeed

### 9.2 Technical Success Criteria

- [ ] All projects migrated to `System.Text.Json`
- [ ] All packages updated
- [ ] Zero security vulnerabilities
- [ ] All builds succeed without errors/warnings
- [ ] All automated tests pass
- [ ] Performance within acceptable thresholds

### 9.3 Quality Criteria

- [ ] Code quality maintained or improved
- [ ] Test coverage maintained or improved
- [ ] Documentation updated
- [ ] No known regressions

### 9.4 Process Criteria

- [ ] Bottom-Up Strategy followed throughout migration
- [ ] Source control strategy followed
- [ ] All steps documented

---

## Planning Principles

- **Dependency-First Ordering**: Migrate leaf projects first, applications next, tests last.
- **Data-Driven Decisions**: Use assessment findings for all recommendations.
- **Completeness**: Address all usages of `Newtonsoft.Json`.
- **Specificity**: Reference exact projects and packages.
- **Actionability**: Each step is clear and executable.
- **Risk Awareness**: High-risk areas flagged and mitigated.
- **Incremental Progress**: Validate after each tier.
- **Source Control Integration**: Commit and review after each tier.
- **Planning vs Execution Separation**: This plan documents what to do; execution is handled separately.

---

This plan enables a safe, staged migration from `Newtonsoft.Json` to `System.Text.Json` across your solution, following best practices and the Bottom-Up Strategy.

---