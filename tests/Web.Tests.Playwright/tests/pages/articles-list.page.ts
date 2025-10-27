import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Articles List Page Object Model
 */
export class ArticlesListPage extends BasePage {
  readonly pageHeading: Locator;
  readonly articlesList: Locator;
  readonly articleItems: Locator;
  readonly createButton: Locator;
  readonly searchInput: Locator;
  readonly filterDropdown: Locator;
  readonly noArticlesMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1, h2').first();
    this.articlesList = page.locator('[data-testid="articles-list"], .articles-list, .article-grid');
    this.articleItems = page.locator('[data-testid="article-item"], .article-item, .article-card');
    this.createButton = page.locator('a[href*="/articles/create"], button:has-text("Create"), button:has-text("New Article")');
    this.searchInput = page.locator('input[type="search"], input[placeholder*="Search"]');
    this.filterDropdown = page.locator('select[name="filter"], select[id*="filter"]');
    this.noArticlesMessage = page.locator('text=/no articles/i, .empty-state');
  }

  /**
   * Navigate to articles list page
   */
  async goto(): Promise<void> {
    await super.goto('/articles');
  }

  /**
   * Get the page heading text
   */
  async getHeadingText(): Promise<string> {
    return await this.pageHeading.textContent() || '';
  }

  /**
   * Get the count of articles displayed
   */
  async getArticlesCount(): Promise<number> {
    return await this.articleItems.count();
  }

  /**
   * Check if articles list is visible
   */
  async hasArticlesList(): Promise<boolean> {
    try {
      return await this.articlesList.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Check if create button is visible
   */
  async hasCreateButton(): Promise<boolean> {
    try {
      return await this.createButton.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Click on create new article button
   */
  async clickCreateButton(): Promise<void> {
    await this.createButton.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Click on a specific article by index
   */
  async clickArticle(index: number = 0): Promise<void> {
    await this.articleItems.nth(index).click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Search for articles
   */
  async searchArticles(searchTerm: string): Promise<void> {
    if (await this.searchInput.isVisible({ timeout: 2000 })) {
      await this.searchInput.fill(searchTerm);
      await this.page.waitForLoadState('networkidle');
    }
  }

  /**
   * Check if no articles message is displayed
   */
  async hasNoArticlesMessage(): Promise<boolean> {
    try {
      return await this.noArticlesMessage.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }
}
