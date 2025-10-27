# End-to-End Testing with Playwright

This document provides comprehensive information about the Playwright E2E testing framework for ArticlesSite.

## Overview

The ArticlesSite project uses [Playwright](https://playwright.dev/) for end-to-end testing. Playwright is a modern browser automation framework that supports Chromium, Firefox, and WebKit, providing reliable cross-browser testing capabilities.

## What's Included

The E2E test suite covers:

### Pages Tested
- **Home Page** - Landing page, navigation, layout
- **About Page** - Static content, navigation
- **Contact Page** - Contact form, validation
- **Articles List** - Article browsing, search, filtering
- **Categories List** - Category browsing, search
- **Error Pages** - 404 and error handling

### Components Tested
- **Navigation Menu** - Links, responsive behavior, accessibility
- **Footer** - Display consistency across pages
- **Page Headers** - Consistent heading structure
- **Error Alerts** - Error message handling

### User Interactions
- Page navigation
- Form submissions
- Search and filtering
- Responsive behavior (mobile, tablet, desktop)
- Error handling and recovery

## Getting Started

### Prerequisites

Before running E2E tests, ensure you have:

1. **Node.js** (v18 or higher)
2. **npm** (v9 or higher)
3. **Running Application** - The web application must be running

### Installation

1. Navigate to the test project:
   ```bash
   cd tests/Web.Tests.Playwright
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Install Playwright browsers:
   ```bash
   npm run install-browsers
   ```

### Running Tests

#### Start the Application

Before running tests, start the application:

```bash
# From repository root
dotnet run --project src/AppHost
```

The application will start on `http://localhost:5000` (default).

#### Run All Tests

```bash
cd tests/Web.Tests.Playwright
npm test
```

#### Run Specific Tests

```bash
# Run specific test file
npx playwright test tests/home.spec.ts

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

#### View Test Reports

After running tests, view the HTML report:

```bash
npm run report
```

This opens an interactive HTML report showing:
- Test results (passed/failed)
- Screenshots of failures
- Video recordings
- Execution traces

## Test Architecture

### Page Object Model (POM)

The test suite uses the Page Object Model pattern to separate page structure from test logic:

```
tests/
├── pages/              # Page Object Models
│   ├── base.page.ts    # Base class with common functionality
│   ├── home.page.ts    # Home page specific methods
│   └── ...
├── fixtures/           # Custom test fixtures
│   └── test-fixtures.ts
├── utils/              # Utility functions
│   └── test-utils.ts
└── *.spec.ts           # Test specifications
```

### Benefits of POM

1. **Maintainability** - Page changes require updates in one place
2. **Reusability** - Page objects can be reused across multiple tests
3. **Readability** - Tests read like user stories
4. **Separation of Concerns** - Test logic separate from page structure

### Example Test

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

## Configuration

### playwright.config.ts

The Playwright configuration file controls:

- **Test directory** - Where tests are located
- **Base URL** - Application URL (configurable via `BASE_URL` env var)
- **Browsers** - Which browsers to test (Chromium, Firefox, WebKit)
- **Reporters** - Test result formats (HTML, JSON, JUnit)
- **Timeouts** - Global and action timeouts
- **Screenshots/Videos** - When to capture media

### Environment Variables

- `BASE_URL` - Override the application URL (default: `http://localhost:5000`)
- `CI` - Set to `true` for CI/CD optimizations

Example:
```bash
BASE_URL=http://localhost:8080 npm test
```

## CI/CD Integration

### GitHub Actions

An example workflow is provided in `.github-workflow-example.yml`. To enable:

1. Copy to `.github/workflows/playwright-tests.yml`
2. Adjust as needed for your CI/CD pipeline
3. Commit and push

The workflow:
1. Sets up .NET and Node.js
2. Builds the application
3. Installs Playwright browsers
4. Starts the application
5. Runs E2E tests
6. Uploads test results as artifacts

### Test Artifacts

Test results are automatically captured:
- **HTML Report** - Interactive test results browser
- **Screenshots** - Captured on test failure
- **Videos** - Recorded for failed tests
- **Traces** - Full execution traces for debugging
- **JUnit XML** - CI/CD compatible test results

## Debugging Tests

### Visual Debugging

Run tests in headed mode to watch execution:

```bash
npm run test:headed
```

### Debug Mode

Use Playwright Inspector for step-by-step debugging:

```bash
npm run test:debug
```

Features:
- Step through test execution
- Inspect page elements
- View console logs
- Record and replay actions

### Trace Viewer

View detailed execution traces:

```bash
npx playwright show-trace test-results/traces/trace.zip
```

Traces show:
- Screenshots at each step
- DOM snapshots
- Network requests
- Console logs
- Action timeline

### Code Generation

Generate test code by recording browser interactions:

```bash
npm run codegen
```

This opens a browser where you can:
1. Navigate and interact with the application
2. Playwright records your actions
3. Generated code is displayed in real-time
4. Copy code into your test files

## Best Practices

### 1. Use Proper Wait Strategies

❌ Don't use fixed timeouts:
```typescript
await page.waitForTimeout(5000); // Bad
```

✅ Wait for specific conditions:
```typescript
await page.waitForSelector('button[type="submit"]');
await page.waitForLoadState('networkidle');
```

### 2. Keep Tests Independent

Each test should:
- Set up its own state
- Not depend on other tests
- Clean up after itself

### 3. Use Descriptive Names

✅ Good:
```typescript
test('should display error message when login fails with invalid credentials', ...);
```

❌ Bad:
```typescript
test('test1', ...);
```

### 4. Test User Workflows

Focus on real user scenarios rather than implementation details.

✅ Good:
```typescript
test('user can create and publish an article', async () => {
  await articlesPage.clickCreateButton();
  await createPage.fillForm({ title: 'Test', content: '...' });
  await createPage.submitForm();
  expect(await detailsPage.isPublished()).toBeTruthy();
});
```

### 5. Handle Flakiness

- Use proper wait conditions
- Avoid hardcoded timeouts
- Test for visible elements before interacting
- Use retry logic for unstable operations

### 6. Keep Page Objects Simple

Page objects should:
- Represent page structure
- Provide high-level actions
- Not contain test assertions
- Be reusable across tests

## Common Issues and Solutions

### Tests Timeout

**Problem**: Tests fail with timeout errors

**Solutions**:
- Ensure application is running before tests
- Check `BASE_URL` is correct
- Increase timeout in `playwright.config.ts`
- Use proper wait conditions instead of fixed timeouts

### Browser Installation Fails

**Problem**: Playwright cannot download browsers

**Solutions**:
```bash
# Install specific browser
npx playwright install chromium

# Install with system dependencies
npx playwright install --with-deps chromium

# Set proxy if needed
HTTPS_PROXY=http://proxy:8080 npx playwright install
```

### Tests Pass Locally but Fail in CI

**Problem**: Tests work on local machine but fail in CI/CD

**Solutions**:
- Check viewport sizes
- Ensure proper wait conditions
- Verify environment variables
- Check for race conditions
- Use `CI` environment variable for CI-specific logic

### Flaky Tests

**Problem**: Tests sometimes pass, sometimes fail

**Solutions**:
- Add proper wait conditions
- Increase timeouts for slow operations
- Check for animations that might cause timing issues
- Use `test.describe.configure({ retries: 2 })` for specific suites
- Verify test isolation (each test should be independent)

## Writing New Tests

### 1. Create Page Object (if needed)

```typescript
// tests/pages/new-page.page.ts
import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

export class NewPage extends BasePage {
  readonly pageHeading: Locator;
  
  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1');
  }
  
  async goto(): Promise<void> {
    await super.goto('/new-page');
  }
}
```

### 2. Create Test File

```typescript
// tests/new-page.spec.ts
import { test, expect } from '../fixtures/test-fixtures';
import { NewPage } from '../pages/new-page.page';

test.describe('New Page Tests', () => {
  let newPage: NewPage;

  test.beforeEach(async ({ page }) => {
    newPage = new NewPage(page);
    await newPage.goto();
  });

  test('should load successfully', async () => {
    expect(newPage.page.url()).toContain('/new-page');
  });
});
```

### 3. Run and Verify

```bash
npx playwright test tests/new-page.spec.ts
```

## Performance Optimization

### Parallel Execution

Tests run in parallel by default:

```typescript
// playwright.config.ts
fullyParallel: true,
workers: process.env.CI ? 1 : undefined,
```

### Selective Test Execution

Run only affected tests:

```bash
# Run specific suite
npx playwright test --grep "Home Page"

# Skip specific tests
npx playwright test --grep-invert "slow"
```

### Reuse Browser Context

Use `test.describe.configure()` to optimize:

```typescript
test.describe.configure({ mode: 'parallel' });
test.describe.configure({ mode: 'serial' }); // For dependent tests
```

## Resources

- [Playwright Documentation](https://playwright.dev/)
- [Playwright API Reference](https://playwright.dev/docs/api/class-playwright)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Playwright Test Configuration](https://playwright.dev/docs/test-configuration)
- [Page Object Model Pattern](https://playwright.dev/docs/pom)

## Contributing

When adding new E2E tests:

1. Follow the Page Object Model pattern
2. Create page objects for new pages
3. Write descriptive test names
4. Ensure tests are independent
5. Add proper wait conditions
6. Test across multiple browsers
7. Update documentation
8. Run tests locally before committing

## License

This project is part of ArticlesSite and follows the same MIT License.
