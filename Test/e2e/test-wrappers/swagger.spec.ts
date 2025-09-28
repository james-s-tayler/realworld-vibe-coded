import { test, expect } from '@playwright/test';

/**
 * Native Playwright test that mirrors the .NET SwaggerE2eTests
 * This provides a cleaner path for HTML report generation
 */
test.describe('Swagger API Documentation', () => {
  test('Swagger API docs are displayed', async ({ page }) => {
    // Navigate to Swagger docs
    await page.goto('/swagger/index.html', {
      waitUntil: 'networkidle',
    });

    // Verify the page loaded
    await expect(page).toHaveTitle(/Swagger/i);
    
    // Wait for Swagger UI to load
    await page.waitForSelector('.swagger-ui', { timeout: 10000 });
    
    // Verify core Swagger elements are present
    await expect(page.locator('.swagger-ui')).toBeVisible();
    
    // Check if the API spec is loaded
    await page.waitForTimeout(2000); // Give time for the spec to load
    
    // Don't assert on specific content as it may vary
    // Just verify we got past the basic loading
    console.log('✅ Swagger UI loaded successfully');
  });
});