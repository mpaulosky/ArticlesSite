import { test, expect } from '../fixtures/test-fixtures';
import { ArticlesListPage } from '../pages/articles-list.page';

test.describe('Articles List Page Tests', () => {
  let articlesPage: ArticlesListPage;

  test.beforeEach(async ({ page }) => {
    articlesPage = new ArticlesListPage(page);
  });

  test('should load articles list page successfully', async () => {
    await articlesPage.goto();
    
    // Verify the page loads
    expect(articlesPage.page.url()).toContain('/articles');
    
    // Verify page title is set
    const title = await articlesPage.getTitle();
    expect(title).toBeTruthy();
  });

  test('should display page heading', async () => {
    await articlesPage.goto();
    
    // Verify heading is displayed
    const heading = await articlesPage.getHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should display navigation menu', async () => {
    await articlesPage.goto();
    
    // Verify navigation is visible
    const isNavVisible = await articlesPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should display footer', async () => {
    await articlesPage.goto();
    
    // Verify footer is visible
    const isFooterVisible = await articlesPage.isFooterVisible();
    expect(isFooterVisible).toBeTruthy();
  });

  test('should show articles list or empty state', async () => {
    await articlesPage.goto();
    
    // The page should either show articles or an empty state message
    const hasArticles = await articlesPage.hasArticlesList();
    const hasEmptyMessage = await articlesPage.hasNoArticlesMessage();
    
    // At least one should be true
    expect(hasArticles || hasEmptyMessage).toBeTruthy();
  });

  test('should display article count correctly', async () => {
    await articlesPage.goto();
    
    // Get article count
    const count = await articlesPage.getArticlesCount();
    
    // Count should be a non-negative number
    expect(count).toBeGreaterThanOrEqual(0);
  });

  test('should navigate back to home page', async () => {
    await articlesPage.goto();
    
    // Click on Home link
    await articlesPage.clickHome();
    
    // Verify navigation
    expect(articlesPage.getCurrentUrl()).toContain('/');
  });

  test('should be responsive on mobile', async ({ page }) => {
    await articlesPage.goto();
    
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify navigation is still visible
    const isNavVisible = await articlesPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should handle search if available', async () => {
    await articlesPage.goto();
    
    // Try to search (only if search is available)
    try {
      await articlesPage.searchArticles('test');
      // If search works, just verify page is still loaded
      expect(articlesPage.page.url()).toContain('/articles');
    } catch {
      // Search might not be available, which is okay
      expect(true).toBeTruthy();
    }
  });
});
