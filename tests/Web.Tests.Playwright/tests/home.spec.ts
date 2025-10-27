import { test, expect } from '@playwright/test';
import { HomePage } from './pages/home.page';

test.describe('Home Page Tests', () => {
  let homePage: HomePage;

  test.beforeEach(async ({ page }) => {
    homePage = new HomePage(page);
    await homePage.goto();
  });

  test('should load home page successfully', async () => {
    // Verify the page loads
    expect(homePage.page.url()).toContain('/');
    
    // Verify page title is set
    const title = await homePage.getTitle();
    expect(title).toBeTruthy();
  });

  test('should display navigation menu', async () => {
    // Verify navigation is visible
    const isNavVisible = await homePage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should display footer', async () => {
    // Verify footer is visible
    const isFooterVisible = await homePage.isFooterVisible();
    expect(isFooterVisible).toBeTruthy();
  });

  test('should have main heading', async () => {
    // Verify heading is displayed
    const heading = await homePage.getHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should navigate to About page from home', async () => {
    // Click on About link
    await homePage.clickAbout();
    
    // Verify navigation
    expect(homePage.getCurrentUrl()).toContain('/about');
  });

  test('should navigate to Contact page from home', async () => {
    // Click on Contact link
    await homePage.clickContact();
    
    // Verify navigation
    expect(homePage.getCurrentUrl()).toContain('/contact');
  });

  test('should be responsive', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify navigation still works
    const isNavVisible = await homePage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });
});
