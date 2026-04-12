import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn006-no-raw-form.js';

RuleTester.afterAll = afterAll;
RuleTester.describe = describe;
RuleTester.it = it;

const ruleTester = new RuleTester({
  languageOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
    parserOptions: { ecmaFeatures: { jsx: true } },
  },
});

ruleTester.run('cbn006-no-raw-form', rule, {
  valid: [
    { code: '<Form onSubmit={handleSubmit} />' },
    { code: '<div />' },
    { code: '<section />' },
  ],
  invalid: [
    {
      code: '<form onSubmit={handleSubmit} />',
      errors: [{ messageId: 'noRawForm' }],
    },
    {
      code: '<form />',
      errors: [{ messageId: 'noRawForm' }],
    },
  ],
});
