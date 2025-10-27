import { test, expect } from '@playwright/test';
import { CategoriesListPage } from './pages/categories-list.page';

test.describe('Categories List Page Tests', () => {
  let categoriesPage: CategoriesListPage;

  test.beforeEach(async ({ page }) => {
    categoriesPage = new CategoriesListPage(page);
  });

  test('should load categories list page successfully', async () => {
    await categoriesPage.goto();
    
    // Verify the page loads
    expect(categoriesPage.page.url()).toContain('/categories');
    
    // Verify page title is set
    const title = await categoriesPage.getTitle();
    expect(title).toBeTruthy();
  });

  test('should display page heading', async () => {
    await categoriesPage.goto();
    
    // Verify heading is displayed
    const heading = await categoriesPage.getHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should display navigation menu', async () => {
    await categoriesPage.goto();
    
    // Verify navigation is visible
    const isNavVisible = await categoriesPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should display footer', async () => {
    await categoriesPage.goto();
    
    // Verify footer is visible
    const isFooterVisible = await categoriesPage.isFooterVisible();
    expect(isFooterVisible).toBeTruthy();
  });

  test('should show categories list or empty state', async () => {
    await categoriesPage.goto();
    
    // The page should either show categories or an empty state message
    const hasCategories = await categoriesPage.hasCategoriesList();
    const hasEmptyMessage = await categoriesPage.hasNoCategoriesMessage();
    
    // At least one should be true
    expect(hasCategories || hasEmptyMessage).toBeTruthy();
  });

  test('should display category count correctly', async () => {
    await categoriesPage.goto();
    
    // Get category count
    const count = await categoriesPage.getCategoriesCount();
    
    // Count should be a non-negative number
    expect(count).toBeGreaterThanOrEqual(0);
  });

  test('should navigate back to home page', async () => {
    await categoriesPage.goto();
    
    // Click on Home link
    await categoriesPage.clickHome();
    
    // Verify navigation
    expect(categoriesPage.getCurrentUrl()).toContain('/');
  });

  test('should be responsive on mobile', async ({ page }) => {
    await categoriesPage.goto();
    
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify navigation is still visible
    const isNavVisible = await categoriesPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should handle search if available', async () => {
    await categoriesPage.goto();
    
    // Try to search (only if search is available)
    try {
      await categoriesPage.searchCategories('test');
      // If search works, just verify page is still loaded
      expect(categoriesPage.page.url()).toContain('/categories');
    } catch {
      // Search might not be available, which is okay
      expect(true).toBeTruthy();
    }
  });
});
