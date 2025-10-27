import { Page, Locator } from '@playwright/test';

/**
 * Base Page Object Model
 * Contains common methods and locators shared across all pages
 */
export class BasePage {
  readonly page: Page;
  readonly navigation: Locator;
  readonly footer: Locator;
  readonly homeLink: Locator;
  readonly aboutLink: Locator;
  readonly contactLink: Locator;
  readonly articlesLink: Locator;
  readonly categoriesLink: Locator;
  readonly profileLink: Locator;
  readonly adminLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.navigation = page.locator('nav');
    this.footer = page.locator('footer');
    
    // Navigation links
    this.homeLink = page.locator('a[href="/"]');
    this.aboutLink = page.locator('a[href="/about"]');
    this.contactLink = page.locator('a[href="/contact"]');
    this.articlesLink = page.locator('a[href*="/articles"]');
    this.categoriesLink = page.locator('a[href*="/categories"]');
    this.profileLink = page.locator('a[href="/profile"]');
    this.adminLink = page.locator('a[href="/admin"]');
  }

  /**
   * Navigate to a specific URL
   */
  async goto(path: string = '/'): Promise<void> {
    await this.page.goto(path);
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Get the page title
   */
  async getTitle(): Promise<string> {
    return await this.page.title();
  }

  /**
   * Check if the navigation is visible
   */
  async isNavigationVisible(): Promise<boolean> {
    return await this.navigation.isVisible();
  }

  /**
   * Check if the footer is visible
   */
  async isFooterVisible(): Promise<boolean> {
    return await this.footer.isVisible();
  }

  /**
   * Click on Home link
   */
  async clickHome(): Promise<void> {
    await this.homeLink.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Click on About link
   */
  async clickAbout(): Promise<void> {
    await this.aboutLink.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Click on Contact link
   */
  async clickContact(): Promise<void> {
    await this.contactLink.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Get the current URL
   */
  getCurrentUrl(): string {
    return this.page.url();
  }

  /**
   * Wait for specific text to appear
   */
  async waitForText(text: string): Promise<void> {
    await this.page.waitForSelector(`text=${text}`, { timeout: 10000 });
  }

  /**
   * Check if error message is displayed
   */
  async hasErrorMessage(): Promise<boolean> {
    const errorLocator = this.page.locator('[role="alert"], .alert-danger, .error-message');
    return await errorLocator.count() > 0;
  }

  /**
   * Get error message text
   */
  async getErrorMessage(): Promise<string> {
    const errorLocator = this.page.locator('[role="alert"], .alert-danger, .error-message');
    return await errorLocator.first().textContent() || '';
  }
}
