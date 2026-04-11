import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn005-no-inline-notification.js';

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

ruleTester.run('cbn005-no-inline-notification', rule, {
  valid: [
    { code: '<ToastNotification kind="error" title="Error" />' },
    { code: '<ActionableNotification kind="error" title="Error" />' },
    { code: '<div className="notification" />' },
  ],
  invalid: [
    {
      code: '<InlineNotification kind="error" title="Error" />',
      errors: [{ messageId: 'noInlineNotification' }],
    },
    {
      code: '<InlineNotification kind="success" title="Done" subtitle="Saved" />',
      errors: [{ messageId: 'noInlineNotification' }],
    },
  ],
});
