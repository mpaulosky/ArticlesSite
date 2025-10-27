import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Categories List Page Object Model
 */
export class CategoriesListPage extends BasePage {
  readonly pageHeading: Locator;
  readonly categoriesList: Locator;
  readonly categoryItems: Locator;
  readonly createButton: Locator;
  readonly searchInput: Locator;
  readonly noCategoriesMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1, h2').first();
    this.categoriesList = page.locator('[data-testid="categories-list"], .categories-list, .category-grid');
    this.categoryItems = page.locator('[data-testid="category-item"], .category-item, .category-card');
    this.createButton = page.locator('a[href*="/categories/create"], button:has-text("Create"), button:has-text("New Category")');
    this.searchInput = page.locator('input[type="search"], input[placeholder*="Search"]');
    this.noCategoriesMessage = page.locator('text=/no categories/i, .empty-state');
  }

  /**
   * Navigate to categories list page
   */
  async goto(): Promise<void> {
    await super.goto('/categories');
  }

  /**
   * Get the page heading text
   */
  async getHeadingText(): Promise<string> {
    return await this.pageHeading.textContent() || '';
  }

  /**
   * Get the count of categories displayed
   */
  async getCategoriesCount(): Promise<number> {
    return await this.categoryItems.count();
  }

  /**
   * Check if categories list is visible
   */
  async hasCategoriesList(): Promise<boolean> {
    try {
      return await this.categoriesList.isVisible({ timeout: 5000 });
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
   * Click on create new category button
   */
  async clickCreateButton(): Promise<void> {
    await this.createButton.first().click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Click on a specific category by index
   */
  async clickCategory(index: number = 0): Promise<void> {
    await this.categoryItems.nth(index).click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Search for categories
   */
  async searchCategories(searchTerm: string): Promise<void> {
    if (await this.searchInput.isVisible({ timeout: 2000 })) {
      await this.searchInput.fill(searchTerm);
      await this.page.waitForLoadState('networkidle');
    }
  }

  /**
   * Check if no categories message is displayed
   */
  async hasNoCategoriesMessage(): Promise<boolean> {
    try {
      return await this.noCategoriesMessage.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }
}
