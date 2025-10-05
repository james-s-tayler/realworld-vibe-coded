import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'
import TrxReporter from 'vitest-trx-results-processor'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    reporters: [
      'default',
      new TrxReporter({
        outputFile: '../../Reports/Client/Results/test-results.trx',
      }),
    ],
    coverage: {
      provider: 'v8',
      reporter: ['cobertura', 'html', 'text'],
      reportsDirectory: '../../Reports/Client/Results/coverage',
    },
  },
})