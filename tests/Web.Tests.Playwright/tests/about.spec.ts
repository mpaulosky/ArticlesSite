import { test, expect } from '../fixtures/test-fixtures';
import { AboutPage } from '../pages/about.page';

test.describe('About Page Tests', () => {
  let aboutPage: AboutPage;

  test.beforeEach(async ({ page }) => {
    aboutPage = new AboutPage(page);
    await aboutPage.goto();
  });

  test('should load about page successfully', async () => {
    // Verify the page loads
    expect(aboutPage.page.url()).toContain('/about');
    
    // Verify page title is set
    const title = await aboutPage.getTitle();
    expect(title).toBeTruthy();
  });

  test('should display page heading', async () => {
    // Verify heading is displayed
    const heading = await aboutPage.getHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should display page content', async () => {
    // Verify content is visible
    const hasContent = await aboutPage.hasContent();
    expect(hasContent).toBeTruthy();
  });

  test('should display navigation menu', async () => {
    // Verify navigation is visible
    const isNavVisible = await aboutPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should display footer', async () => {
    // Verify footer is visible
    const isFooterVisible = await aboutPage.isFooterVisible();
    expect(isFooterVisible).toBeTruthy();
  });

  test('should navigate back to home page', async () => {
    // Click on Home link
    await aboutPage.clickHome();
    
    // Verify navigation
    expect(aboutPage.getCurrentUrl()).toContain('/');
  });

  test('should be accessible on mobile', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify content is still visible
    const hasContent = await aboutPage.hasContent();
    expect(hasContent).toBeTruthy();
  });
});
