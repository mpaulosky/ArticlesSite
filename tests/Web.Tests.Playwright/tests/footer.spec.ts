import { test, expect } from '@playwright/test';
import { BasePage } from './pages/base.page';

test.describe('Footer Component Tests', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: playwrightPage }) => {
    page = new BasePage(playwrightPage);
  });

  test('should display footer on all pages', async () => {
    // Test home page
    await page.goto('/');
    expect(await page.isFooterVisible()).toBeTruthy();
    
    // Test about page
    await page.goto('/about');
    expect(await page.isFooterVisible()).toBeTruthy();
    
    // Test contact page
    await page.goto('/contact');
    expect(await page.isFooterVisible()).toBeTruthy();
  });

  test('should be visible on mobile devices', async ({ page: playwrightPage }) => {
    // Test mobile viewport
    await playwrightPage.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');
    await playwrightPage.waitForTimeout(500);
    
    // Scroll to footer
    await playwrightPage.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
    await playwrightPage.waitForTimeout(500);
    
    // Verify footer is visible
    expect(await page.isFooterVisible()).toBeTruthy();
  });

  test('should be visible on desktop', async ({ page: playwrightPage }) => {
    // Test desktop viewport
    await playwrightPage.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/');
    await playwrightPage.waitForTimeout(500);
    
    // Verify footer is visible
    expect(await page.isFooterVisible()).toBeTruthy();
  });
});
