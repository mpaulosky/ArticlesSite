import { test, expect } from '@playwright/test';
import { BasePage } from './pages/base.page';

test.describe('Navigation Component Tests', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: playwrightPage }) => {
    page = new BasePage(playwrightPage);
    await page.goto('/');
  });

  test('should display navigation menu on all pages', async () => {
    // Test home page
    await page.goto('/');
    expect(await page.isNavigationVisible()).toBeTruthy();
    
    // Test about page
    await page.goto('/about');
    expect(await page.isNavigationVisible()).toBeTruthy();
    
    // Test contact page
    await page.goto('/contact');
    expect(await page.isNavigationVisible()).toBeTruthy();
  });

  test('should navigate between pages using navigation links', async () => {
    // Start at home
    await page.goto('/');
    
    // Navigate to About
    await page.clickAbout();
    expect(page.getCurrentUrl()).toContain('/about');
    
    // Navigate to Contact
    await page.clickContact();
    expect(page.getCurrentUrl()).toContain('/contact');
    
    // Navigate back to Home
    await page.clickHome();
    expect(page.getCurrentUrl()).toContain('/');
  });

  test('should be responsive on mobile devices', async ({ page: playwrightPage }) => {
    // Test mobile viewport
    await playwrightPage.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await playwrightPage.waitForTimeout(500);
    
    // Verify navigation is still accessible
    expect(await page.isNavigationVisible()).toBeTruthy();
  });

  test('should be responsive on tablet devices', async ({ page: playwrightPage }) => {
    // Test tablet viewport
    await playwrightPage.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/');
    await playwrightPage.waitForTimeout(500);
    
    // Verify navigation is still accessible
    expect(await page.isNavigationVisible()).toBeTruthy();
  });
});
