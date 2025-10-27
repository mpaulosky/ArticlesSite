import { test as base, expect as baseExpect, Page } from '@playwright/test';

/**
 * Custom test fixtures for ArticlesSite E2E tests
 * Extend this to add custom fixtures like authenticated users, database setup, etc.
 */
export const test = base.extend({
  // Add custom fixtures here if needed
  // Example:
  // authenticatedPage: async ({ page }, use) => {
  //   // Perform authentication
  //   await page.goto('/login');
  //   // ... login logic
  //   await use(page);
  // },
});

export const expect = baseExpect;
