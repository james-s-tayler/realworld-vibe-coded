import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import i18next from 'eslint-plugin-i18next'
import tseslint from 'typescript-eslint'
import { defineConfig, globalIgnores } from 'eslint/config'
import customRules from './eslint-plugin-custom-rules/index.js'

export default defineConfig([
  globalIgnores(['dist', 'src/api/generated']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs['recommended-latest'],
      reactRefresh.configs.vite,
    ],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
  },
  {
    files: ['**/*.tsx'],
    ignores: ['**/*.test.tsx'],
    plugins: { i18next },
    rules: {
      'i18next/no-literal-string': ['error', {
        mode: 'jsx-text-only',
        'jsx-attributes': {
          include: ['title', 'aria-label', 'alt', 'placeholder'],
        },
      }],
    },
  },

  // CBN001, CBN002, CBN004, CBN005, CBN006: Carbon component rules (all source, not tests)
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/**/*.test.{ts,tsx}', 'src/test/**'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/cbn001-no-password-textinput': 'error',
      'custom-rules/cbn002-no-empty-label-text': 'error',
      'custom-rules/cbn004-no-on-key-press': 'error',
      'custom-rules/cbn005-no-inline-notification': 'error',
      'custom-rules/cbn006-no-raw-form': 'error',
    },
  },

  // CBN003: No inline styles (pages + components only, excluding ErrorDisplay)
  {
    files: ['src/pages/**/*.{ts,tsx}', 'src/components/**/*.{ts,tsx}'],
    ignores: ['src/**/*.test.{ts,tsx}', 'src/components/ErrorDisplay.tsx'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/cbn003-no-inline-styles': 'error',
    },
  },

  // no-console: Ban console.* in production code
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/**/*.test.{ts,tsx}', 'src/test/**'],
    rules: {
      'no-console': 'error',
    },
  },

  // TST001: No userEvent.type() in tests (causes CI flakiness)
  {
    files: ['src/**/*.test.{ts,tsx}'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/tst001-no-user-event-type': 'error',
    },
  },

  // ARCH001: No direct useContext in pages/components
  {
    files: ['src/pages/**/*.{ts,tsx}', 'src/components/**/*.{ts,tsx}'],
    ignores: ['src/**/*.test.{ts,tsx}'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/arch001-no-direct-use-context': 'error',
    },
  },

  // ARCH002: No generated imports outside api/
  {
    files: ['src/**/*.{ts,tsx}'],
    ignores: ['src/api/**', 'src/**/*.test.{ts,tsx}', 'src/test/**'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/arch002-no-generated-imports': 'error',
    },
  },

  // ARCH003: No API imports in components
  {
    files: ['src/components/**/*.{ts,tsx}'],
    ignores: ['src/**/*.test.{ts,tsx}'],
    plugins: { 'custom-rules': customRules },
    rules: {
      'custom-rules/arch003-no-api-in-components': 'error',
    },
  },
])
