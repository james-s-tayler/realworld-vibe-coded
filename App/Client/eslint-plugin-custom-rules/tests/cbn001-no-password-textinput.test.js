import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn001-no-password-textinput.js';

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

ruleTester.run('cbn001-no-password-textinput', rule, {
  valid: [
    { code: '<PasswordInput id="pw" labelText="Password" />' },
    { code: '<TextInput id="name" labelText="Name" />' },
    { code: '<TextInput id="email" type="email" labelText="Email" />' },
    { code: '<TextInput id="text" type="text" labelText="Text" />' },
  ],
  invalid: [
    {
      code: '<TextInput id="pw" type="password" labelText="Password" />',
      errors: [{ messageId: 'noPasswordTextInput' }],
    },
    {
      code: '<TextInput id="pw" type={"password"} labelText="Password" />',
      errors: [{ messageId: 'noPasswordTextInput' }],
    },
  ],
});
