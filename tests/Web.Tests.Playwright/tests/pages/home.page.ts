import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Home Page Object Model
 */
export class HomePage extends BasePage {
  readonly pageHeading: Locator;
  readonly welcomeMessage: Locator;
  readonly recentArticles: Locator;
  readonly featuredContent: Locator;

  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1, h2').first();
    this.welcomeMessage = page.locator('text=/welcome/i').first();
    this.recentArticles = page.locator('[data-testid="recent-articles"], .recent-articles');
    this.featuredContent = page.locator('[data-testid="featured-content"], .featured-content');
  }

  /**
   * Navigate to home page
   */
  async goto(): Promise<void> {
    await super.goto('/');
  }

  /**
   * Check if welcome message is visible
   */
  async hasWelcomeMessage(): Promise<boolean> {
    try {
      return await this.welcomeMessage.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Get the main heading text
   */
  async getHeadingText(): Promise<string> {
    return await this.pageHeading.textContent() || '';
  }

  /**
   * Check if recent articles section is visible
   */
  async hasRecentArticles(): Promise<boolean> {
    try {
      return await this.recentArticles.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }
}
