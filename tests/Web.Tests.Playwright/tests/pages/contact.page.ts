import { Page, Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Contact Page Object Model
 */
export class ContactPage extends BasePage {
  readonly pageHeading: Locator;
  readonly contactForm: Locator;
  readonly nameInput: Locator;
  readonly emailInput: Locator;
  readonly messageInput: Locator;
  readonly submitButton: Locator;
  readonly successMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.pageHeading = page.locator('h1, h2').first();
    this.contactForm = page.locator('form');
    this.nameInput = page.locator('input[name="name"], input[id*="name"]');
    this.emailInput = page.locator('input[name="email"], input[id*="email"], input[type="email"]');
    this.messageInput = page.locator('textarea[name="message"], textarea[id*="message"]');
    this.submitButton = page.locator('button[type="submit"], input[type="submit"]');
    this.successMessage = page.locator('.alert-success, .success-message');
  }

  /**
   * Navigate to contact page
   */
  async goto(): Promise<void> {
    await super.goto('/contact');
  }

  /**
   * Get the page heading text
   */
  async getHeadingText(): Promise<string> {
    return await this.pageHeading.textContent() || '';
  }

  /**
   * Check if contact form is visible
   */
  async hasContactForm(): Promise<boolean> {
    try {
      return await this.contactForm.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }

  /**
   * Fill and submit contact form
   */
  async submitContactForm(name: string, email: string, message: string): Promise<void> {
    if (await this.nameInput.isVisible()) {
      await this.nameInput.fill(name);
    }
    if (await this.emailInput.isVisible()) {
      await this.emailInput.fill(email);
    }
    if (await this.messageInput.isVisible()) {
      await this.messageInput.fill(message);
    }
    await this.submitButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Check if success message is displayed
   */
  async hasSuccessMessage(): Promise<boolean> {
    try {
      return await this.successMessage.isVisible({ timeout: 5000 });
    } catch {
      return false;
    }
  }
}
