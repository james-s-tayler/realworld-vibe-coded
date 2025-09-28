// @ts-check
import { defineConfig, devices } from '@playwright/test';

/**
 * Native Playwright configuration for HTML report generation
 * This replaces the complex Node.js + .NET hybrid approach with a native Playwright solution
 */
export default defineConfig({
  testDir: './test-wrappers',
  /* Run tests in files in parallel */
  fullyParallel: true,
  /* Fail the build on CI if you accidentally left test.only in the source code. */
  forbidOnly: !!process.env.CI,
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  /* Opt out of parallel tests on CI. */
  workers: process.env.CI ? 1 : undefined,
  
  /* Native HTML reporter configuration */
  reporter: [
    ['html', { 
      outputFolder: '../../reports/e2e/playwright-report',
      open: 'never'
    }],
    ['json', { 
      outputFile: '../../reports/e2e/test-results.json' 
    }]
  ],
  
  /* Shared settings for all projects */
  use: {
    /* Base URL */
    baseURL: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:5000',
    /* Collect trace when retrying failed tests */
    trace: 'on-first-retry',
    /* Take screenshot on failure */
    screenshot: 'only-on-failure',
    /* Save video for failed tests */
    video: 'retain-on-failure',
  },

  /* Configure projects for major browsers */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  /* Output directory for test artifacts */
  outputDir: '../../reports/e2e/test-results/',
});