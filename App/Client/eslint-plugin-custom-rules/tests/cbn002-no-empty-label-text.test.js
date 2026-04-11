import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn002-no-empty-label-text.js';

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

ruleTester.run('cbn002-no-empty-label-text', rule, {
  valid: [
    { code: '<TextInput labelText="Name" />' },
    { code: '<TextInput labelText={t("label.name")} />' },
    { code: '<TextArea labelText="Bio" hideLabel />' },
  ],
  invalid: [
    {
      code: '<TextInput labelText="" />',
      errors: [{ messageId: 'noEmptyLabelText' }],
    },
    {
      code: '<TextInput labelText={""} />',
      errors: [{ messageId: 'noEmptyLabelText' }],
    },
    {
      code: '<TextArea labelText="" />',
      errors: [{ messageId: 'noEmptyLabelText' }],
    },
  ],
});
