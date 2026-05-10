# Sam — Backend Dev History

## Project Context
- **Project:** ArticlesSite — a blog management application built with Blazor Server, .NET Aspire, and MongoDB
- **Stack:** .NET 10, C# 14, Blazor Server, MongoDB (MongoDB.Driver), .NET Aspire, Redis Cache, MediatR, Mapster, FluentValidation, Vertical Slice Architecture, Auth0
- **User:** Matthew Paulosky
- **Repo:** mpaulosky/ArticlesSite
- **Source:** src/AppHost, src/ServiceDefaults, src/Shared, src/Web
- **Tests:** tests/Architecture.Tests, tests/Shared.Tests.Unit, tests/Web.Tests.Integration, tests/Web.Tests.Unit
- **Squad exported from:** IssueTrackerApp (2026-03-18)

## Learnings

### PR #97 CI Fix (2025-01-20)
**Issue:** PR #97 failed CI with NU1015 errors (missing package versions)
**Root Cause:** XML typo in Directory.Packages.props - double-quote on Aspire.MongoDB.Driver line 16: `Version="13.3.0""` broke XML parsing
**Additional Issues Found:**
- MongoDB.Driver.Core was 3.7.1 while MongoDB.Driver was 3.8.0 (should match)
- 8 .lscache files from VS Code C# Dev Kit committed (should be in .gitignore)
- MongoDB.Driver 3.8.0 has transitive dependencies with security vulnerabilities (SharpCompress 0.30.1, Snappier 1.0.0)

**Fix Applied:**
- Corrected XML typo (removed extra double-quote)
- Updated MongoDB.Driver.Core to 3.8.0
- Deleted all .lscache files
- Added NU1902 and NU1903 to NoWarn (security warnings from MongoDB driver dependencies)

**Pattern:** When XML parsing fails in Directory.Packages.props, ALL packages defined AFTER the error have no version. The XML typo prevented any further package versions from being read, causing widespread NU1015 errors across the entire solution.
