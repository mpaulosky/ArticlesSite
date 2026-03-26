# Pippin — History

## Project Context

**Project:** ArticlesSite (TailwindBlogApp solution)
**User:** Matthew (mpaulosky)
**Date Joined:** 2026-03-26

### Tech Stack
- .NET 10 / C# 14, Blazor Server, MongoDB, Redis, .NET Aspire, Auth0
- xUnit, FluentAssertions, NSubstitute, bUnit, TestContainers
- Vertical Slice Architecture, CQRS (static outer class), Result&lt;T&gt;

### My Domain
Blog posts and article content for the ArticlesSite — a Blazor Server app that
manages articles and categories backed by MongoDB.

### Key Paths
- `src/Web/Components/Features/Articles/` — article feature vertical slice
- `src/Web/Components/Features/Categories/` — category feature
- `src/Web/Components/Features/Articles/Fakes/` — FakeArticle (Bogus, seed=621)
- `src/Web/Components/Features/Categories/Fakes/` — FakeCategory

### Key Entities
- `Article` — title, content, author, category, published state, timestamps
- `Category` — name, description
- `ArticleDto` / `CategoryDto` — transfer objects used in handlers and components

### Conventions
- File copyright headers required
- File-scoped namespaces
- Async suffix on async methods
- useSeed: true for deterministic test data

## Learnings
