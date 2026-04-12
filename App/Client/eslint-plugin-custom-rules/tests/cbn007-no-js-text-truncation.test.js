import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn007-no-js-text-truncation.js';

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

ruleTester.run('cbn007-no-js-text-truncation', rule, {
  valid: [
    { code: '<span className="cds--text-truncate-end">{username}</span>' },
    { code: 'const x = someOtherFunction(text, 50)' },
    { code: '<span>{formatDate(date)}</span>' },
  ],
  invalid: [
    {
      code: '<span>{truncateText(name, 50)}</span>',
      errors: [{ messageId: 'noJsTruncation' }],
    },
    {
      code: '<span>{truncateUsername(name)}</span>',
      errors: [{ messageId: 'noJsTruncation' }],
    },
    {
      code: 'const label = truncateText(username, 50)',
      errors: [{ messageId: 'noJsTruncation' }],
    },
  ],
});
