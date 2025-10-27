import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * About Page Object Model
 */
export class AboutPage extends BasePage {
  readonly pageHeading: Locator;
  readonly pageContent: Locator;

  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1, h2').first();
    this.pageContent = page.locator('main, article, .content');
  }

  /**
   * Navigate to about page
   */
  async goto(): Promise<void> {
    await super.goto('/about');
  }

  /**
   * Get the page heading text
   */
  async getHeadingText(): Promise<string> {
    return await this.pageHeading.textContent() || '';
  }

  /**
   * Check if page content is visible
   */
  async hasContent(): Promise<boolean> {
    return await this.pageContent.isVisible();
  }
}
