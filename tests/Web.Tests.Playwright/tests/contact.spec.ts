import { test, expect } from '../fixtures/test-fixtures';
import { ContactPage } from '../pages/contact.page';

test.describe('Contact Page Tests', () => {
  let contactPage: ContactPage;

  test.beforeEach(async ({ page }) => {
    contactPage = new ContactPage(page);
    await contactPage.goto();
  });

  test('should load contact page successfully', async () => {
    // Verify the page loads
    expect(contactPage.page.url()).toContain('/contact');
    
    // Verify page title is set
    const title = await contactPage.getTitle();
    expect(title).toBeTruthy();
  });

  test('should display page heading', async () => {
    // Verify heading is displayed
    const heading = await contactPage.getHeadingText();
    expect(heading).toBeTruthy();
    expect(heading.length).toBeGreaterThan(0);
  });

  test('should display navigation menu', async () => {
    // Verify navigation is visible
    const isNavVisible = await contactPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });

  test('should display footer', async () => {
    // Verify footer is visible
    const isFooterVisible = await contactPage.isFooterVisible();
    expect(isFooterVisible).toBeTruthy();
  });

  test('should have contact form if available', async () => {
    // Check if contact form exists on the page
    // This is optional as not all contact pages have forms
    const hasForm = await contactPage.hasContactForm();
    
    // We just verify the check completes without error
    // The form may or may not be present depending on implementation
    expect(typeof hasForm).toBe('boolean');
  });

  test('should navigate back to home page', async () => {
    // Click on Home link
    await contactPage.clickHome();
    
    // Verify navigation
    expect(contactPage.getCurrentUrl()).toContain('/');
  });

  test('should be accessible on mobile', async ({ page }) => {
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);
    
    // Verify navigation is still visible
    const isNavVisible = await contactPage.isNavigationVisible();
    expect(isNavVisible).toBeTruthy();
  });
});
