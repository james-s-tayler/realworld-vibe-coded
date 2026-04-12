import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/tst001-no-user-event-type.js';

RuleTester.afterAll = afterAll;
RuleTester.describe = describe;
RuleTester.it = it;

const ruleTester = new RuleTester({
  languageOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
  },
});

ruleTester.run('tst001-no-user-event-type', rule, {
  valid: [
    { code: 'user.click(input)' },
    { code: 'user.paste("hello")' },
    { code: 'user.clear(input)' },
    { code: 'user.keyboard("{Enter}")' },
    { code: 'someObject.type("not userEvent")' },
  ],
  invalid: [
    {
      code: 'user.type(emailInput, "user@test.com")',
      errors: [{ messageId: 'noUserEventType' }],
    },
    {
      code: 'user.type(input, "text")',
      errors: [{ messageId: 'noUserEventType' }],
    },
  ],
});
