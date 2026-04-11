import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn004-no-on-key-press.js';

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

ruleTester.run('cbn004-no-on-key-press', rule, {
  valid: [
    { code: '<input onKeyDown={handleKey} />' },
    { code: '<div onClick={handleClick} />' },
    { code: '<input onKeyUp={handleKey} />' },
  ],
  invalid: [
    {
      code: '<input onKeyPress={handleKey} />',
      errors: [{ messageId: 'noOnKeyPress' }],
    },
    {
      code: '<div onKeyPress={fn} />',
      errors: [{ messageId: 'noOnKeyPress' }],
    },
  ],
});
