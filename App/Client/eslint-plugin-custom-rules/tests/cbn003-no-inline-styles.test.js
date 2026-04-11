import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn003-no-inline-styles.js';

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

ruleTester.run('cbn003-no-inline-styles', rule, {
  valid: [
    { code: '<div className="my-class" />' },
    { code: '<Button kind="primary" />' },
  ],
  invalid: [
    {
      code: '<div style={{ color: "red" }} />',
      errors: [{ messageId: 'noInlineStyles' }],
    },
    {
      code: '<Button style={buttonStyles} />',
      errors: [{ messageId: 'noInlineStyles' }],
    },
  ],
});
