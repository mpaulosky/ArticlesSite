import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Error Page Object Model
 */
export class ErrorPage extends BasePage {
  readonly errorHeading: Locator;
  readonly errorMessage: Locator;
  readonly errorCode: Locator;
  readonly homeLink: Locator;
  readonly backButton: Locator;

  constructor(page: Page) {
    super(page);
    this.errorHeading = page.locator('h1:has-text("Error"), h1:has-text("Not Found"), h1:has-text("404")');
    this.errorMessage = page.locator('.error-message, .error-description, p:below(h1)').first();
    this.errorCode = page.locator('text=/404|500|error/i');
    this.homeLink = page.locator('a[href="/"]');
    this.backButton = page.locator('button:has-text("Back"), a:has-text("Go Back")');
  }

  /**
   * Navigate to a non-existent page to trigger 404
   */
  async gotoNonExistentPage(): Promise<void> {
    await super.goto('/this-page-does-not-exist-' + Date.now());
  }

  /**
   * Get the error heading text
   */
  async getErrorHeadingText(): Promise<string> {
    try {
      return await this.errorHeading.textContent() || '';
    } catch {
      return '';
    }
  }

  /**
   * Check if error page is displayed
   */
  async isErrorPageDisplayed(): Promise<boolean> {
    try {
      return await this.errorHeading.isVisible({ timeout: 5000 }) || 
             await this.errorCode.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Check if home link is visible
   */
  async hasHomeLink(): Promise<boolean> {
    try {
      return await this.homeLink.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Click on home link to return to home page
   */
  async clickHomeLink(): Promise<void> {
    await this.homeLink.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Click on back button
   */
  async clickBackButton(): Promise<void> {
    if (await this.backButton.isVisible({ timeout: 2000 })) {
      await this.backButton.click();
      await this.page.waitForLoadState('networkidle');
    }
  }
}
