# Web.Tests.Playwright

End-to-end (E2E) tests for the ArticlesSite Web project using Playwright.

## Overview

This test project provides comprehensive E2E testing coverage for the Blazor Server web application. Tests are written in TypeScript using the Playwright test framework and follow the Page Object Model (POM) pattern for maintainability.

## Prerequisites

- **Node.js** (v18 or higher)
- **npm** (v9 or higher)
- **Playwright browsers** (installed automatically or via `npm run install-browsers`)
- **Running Web Application** - The application must be running before executing tests

## Getting Started

### 1. Install Dependencies

```bash
cd tests/Web.Tests.Playwright
npm install
```

### 2. Install Playwright Browsers

```bash
npm run install-browsers
```

This will download Chromium, Firefox, and WebKit browsers along with system dependencies.

### 3. Start the Application

Before running tests, ensure the web application is running:

```bash
# From the repository root
dotnet run --project src/AppHost
```

The application typically runs on `http://localhost:5000` by default.

### 4. Run Tests

```bash
# Run all tests
npm test

# Run tests in headed mode (see browser)
npm run test:headed

# Run tests in debug mode
npm run test:debug

# Run tests for specific browser
npm run test:chromium
npm run test:firefox
npm run test:webkit

# Run mobile tests only
npm run test:mobile
```

## Project Structure

```
Web.Tests.Playwright/
├── tests/
│   ├── fixtures/
│   │   └── test-fixtures.ts      # Custom test fixtures
│   ├── pages/                     # Page Object Models
│   │   ├── base.page.ts           # Base page class
│   │   ├── home.page.ts           # Home page POM
│   │   ├── about.page.ts          # About page POM
│   │   ├── contact.page.ts        # Contact page POM
│   │   ├── articles-list.page.ts  # Articles list POM
│   │   ├── categories-list.page.ts # Categories list POM
│   │   └── error.page.ts          # Error page POM
│   ├── utils/
│   │   └── test-utils.ts          # Test utility functions
│   ├── home.spec.ts               # Home page tests
│   ├── about.spec.ts              # About page tests
│   ├── contact.spec.ts            # Contact page tests
│   ├── articles-list.spec.ts      # Articles tests
│   ├── categories-list.spec.ts    # Categories tests
│   ├── error.spec.ts              # Error page tests
│   ├── navigation.spec.ts         # Navigation component tests
│   └── footer.spec.ts             # Footer component tests
├── playwright.config.ts           # Playwright configuration
├── tsconfig.json                  # TypeScript configuration
├── package.json                   # npm configuration
└── README.md                      # This file
```

## Test Coverage

### Pages
- ✅ Home page - Layout, navigation, content
- ✅ About page - Content, navigation
- ✅ Contact page - Form, navigation
- ✅ Articles list - Display, search, filtering
- ✅ Categories list - Display, search, filtering
- ✅ Error pages - 404, error handling

### Components
- ✅ Navigation menu - Links, responsive behavior
- ✅ Footer - Display across pages, responsive
- ✅ Page headers - Consistent display
- ✅ Error alerts - Error message handling

### User Interactions
- ✅ Navigation between pages
- ✅ Form interactions (where applicable)
- ✅ Responsive behavior (mobile, tablet, desktop)
- ✅ Error handling and recovery

## Configuration

### Base URL

The base URL for tests is configured in `playwright.config.ts`:

```typescript
use: {
  baseURL: process.env.BASE_URL || 'http://localhost:5000',
}
```

You can override this by setting the `BASE_URL` environment variable:

```bash
BASE_URL=http://localhost:8080 npm test
```

### Browsers

Tests run against multiple browsers by default:
- Chromium (Desktop)
- Firefox (Desktop)
- WebKit (Safari)
- Mobile Chrome (Pixel 5)
- Mobile Safari (iPhone 12)

You can run tests for specific browsers using the project flags:

```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### Reporters

Test results are generated in multiple formats:
- **HTML Report** - Interactive HTML report in `playwright-report/`
- **JSON Report** - Machine-readable results in `test-results/test-results.json`
- **JUnit XML** - CI/CD compatible format in `test-results/junit.xml`
- **Console** - Real-time list output during test execution

View the HTML report:

```bash
npm run report
```

## Writing Tests

### Page Object Model Pattern

Tests use the Page Object Model pattern to separate page structure from test logic:

```typescript
import { test, expect } from '../fixtures/test-fixtures';
import { HomePage } from '../pages/home.page';

test.describe('Home Page Tests', () => {
  let homePage: HomePage;

  test.beforeEach(async ({ page }) => {
    homePage = new HomePage(page);
    await homePage.goto();
  });

  test('should display welcome message', async () => {
    const hasWelcome = await homePage.hasWelcomeMessage();
    expect(hasWelcome).toBeTruthy();
  });
});
```

### Custom Fixtures

Extend the base fixtures in `tests/fixtures/test-fixtures.ts` to add custom test setup:

```typescript
export const test = base.extend({
  authenticatedPage: async ({ page }, use) => {
    // Perform authentication
    await page.goto('/login');
    // ... login logic
    await use(page);
  },
});
```

### Utility Functions

Common test utilities are available in `tests/utils/test-utils.ts`:

```typescript
import { generateArticleData, takeScreenshot } from '../utils/test-utils';

test('should create article', async ({ page }) => {
  const articleData = generateArticleData();
  // ... use article data
  await takeScreenshot(page, 'article-created');
});
```

## CI/CD Integration

### GitHub Actions

The tests are designed to run in GitHub Actions. Example workflow:

```yaml
- name: Install Playwright Browsers
  run: |
    cd tests/Web.Tests.Playwright
    npm ci
    npx playwright install --with-deps

- name: Run E2E Tests
  run: |
    cd tests/Web.Tests.Playwright
    npm test
  env:
    BASE_URL: http://localhost:5000

- name: Upload Test Results
  if: always()
  uses: actions/upload-artifact@v4
  with:
    name: playwright-report
    path: tests/Web.Tests.Playwright/playwright-report/
```

### Test Artifacts

Test results, screenshots, and videos are automatically captured:
- Screenshots on failure
- Videos on failure
- Trace files for debugging

These are stored in `test-results/` and `playwright-report/` directories.

## Debugging Tests

### Visual Debugging

Run tests in headed mode to see the browser:

```bash
npm run test:headed
```

### Debug Mode

Run tests in debug mode with Playwright Inspector:

```bash
npm run test:debug
```

### Trace Viewer

View traces for failed tests:

```bash
npx playwright show-trace test-results/traces/trace.zip
```

### Code Generation

Generate test code by recording interactions:

```bash
npm run codegen
```

## Best Practices

1. **Use Page Object Models** - Keep page structure separate from test logic
2. **Wait for Elements** - Use proper wait strategies instead of fixed timeouts
3. **Isolate Tests** - Each test should be independent and not rely on other tests
4. **Clean Up** - Reset state after tests when necessary
5. **Descriptive Names** - Use clear, descriptive test and assertion names
6. **Handle Async** - Always await async operations
7. **Test Real Scenarios** - Focus on user workflows, not implementation details

## Troubleshooting

### Tests Timeout

- Ensure the application is running before tests
- Check the `BASE_URL` configuration
- Increase timeout in `playwright.config.ts` if needed

### Browser Installation Fails

Try installing browsers manually:

```bash
npx playwright install --with-deps chromium
```

### Tests Fail Inconsistently

- Check for race conditions
- Add proper wait conditions
- Ensure test isolation

### Port Already in Use

If the application port is already in use, set a different port:

```bash
BASE_URL=http://localhost:8080 npm test
```

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Page Object Model](https://playwright.dev/docs/pom)
- [Debugging Tests](https://playwright.dev/docs/debug)
- [CI/CD Integration](https://playwright.dev/docs/ci)

## Contributing

When adding new tests:

1. Create appropriate Page Object Models in `tests/pages/`
2. Write test specifications in `tests/`
3. Use existing fixtures and utilities
4. Follow the established naming conventions
5. Ensure tests pass locally before committing
6. Update this README if adding significant functionality

## License

This project is part of ArticlesSite and follows the same MIT License.
