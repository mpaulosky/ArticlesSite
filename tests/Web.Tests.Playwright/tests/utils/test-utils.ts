/**
 * Test utilities and helper functions
 */

/**
 * Wait for the page to be fully loaded
 */
export async function waitForPageLoad(page: any): Promise<void> {
  await page.waitForLoadState('networkidle');
}

/**
 * Generate a random string for test data
 */
export function generateRandomString(length: number = 10): string {
  const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
  let result = '';
  for (let i = 0; i < length; i++) {
    result += characters.charAt(Math.floor(Math.random() * characters.length));
  }
  return result;
}

/**
 * Generate test data for an article
 */
export function generateArticleData() {
  const timestamp = Date.now();
  return {
    title: `Test Article ${timestamp}`,
    description: `Test article description for automated testing ${timestamp}`,
    content: `This is test content for article created at ${timestamp}. It contains multiple paragraphs and rich content.`,
    author: 'Test Author',
  };
}

/**
 * Generate test data for a category
 */
export function generateCategoryData() {
  const timestamp = Date.now();
  return {
    name: `Test Category ${timestamp}`,
    description: `Test category description for automated testing ${timestamp}`,
  };
}

/**
 * Take a screenshot with a descriptive name
 */
export async function takeScreenshot(page: any, name: string): Promise<void> {
  await page.screenshot({ path: `test-results/screenshots/${name}-${Date.now()}.png`, fullPage: true });
}
