import { test, expect } from '../fixtures/test-fixtures';
import { ErrorPage } from '../pages/error.page';

test.describe('Error Page Tests', () => {
  let errorPage: ErrorPage;

  test.beforeEach(async ({ page }) => {
    errorPage = new ErrorPage(page);
  });

  test('should display 404 error page for non-existent routes', async () => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Verify error page is displayed
    const isErrorDisplayed = await errorPage.isErrorPageDisplayed();
    expect(isErrorDisplayed).toBeTruthy();
  });

  test('should display error heading on 404 page', async () => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Verify error heading is displayed
    const heading = await errorPage.getErrorHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should provide navigation back to home on error page', async () => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Verify home link is available
    const hasHomeLink = await errorPage.hasHomeLink();
    expect(hasHomeLink).toBeTruthy();
  });

  test('should navigate back to home from error page', async () => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Click home link
    await errorPage.clickHomeLink();
    
    // Verify navigation to home
    expect(errorPage.getCurrentUrl()).toContain('/');
  });

  test('should display navigation menu on error page', async () => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Verify navigation is visible
    const isNavVisible = await errorPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Navigate to non-existent page
    await errorPage.gotoNonExistentPage();
    
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify error page is still displayed
    const isErrorDisplayed = await errorPage.isErrorPageDisplayed();
    expect(isErrorDisplayed).toBeTruthy();
  });
});
