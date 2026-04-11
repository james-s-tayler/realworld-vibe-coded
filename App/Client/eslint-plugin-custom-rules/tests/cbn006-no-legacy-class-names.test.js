import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/cbn006-no-legacy-class-names.js';

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

ruleTester.run('cbn006-no-legacy-class-names', rule, {
  valid: [
    // Carbon Grid usage
    { code: '<Grid><Column lg={4}>content</Column></Grid>' },
    // Normal class names
    { code: '<div className="my-component" />' },
    // Template literals without legacy classes
    { code: '<div className={`page-${name}`} />' },
    // Non-class-name strings
    { code: 'const label = "column header"' },
  ],
  invalid: [
    // Bootstrap column classes
    {
      code: '<div className="col-md-6" />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'col-md-6' } }],
    },
    {
      code: '<div className="col-xs-12" />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'col-xs-12' } }],
    },
    // Offset classes
    {
      code: '<div className="offset-md-3" />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'offset-md-3' } }],
    },
    // Multiple legacy classes in one string
    {
      code: "const cls = 'col-md-6 offset-md-3'",
      errors: [
        { messageId: 'noLegacyClassName', data: { className: 'col-md-6' } },
        { messageId: 'noLegacyClassName', data: { className: 'offset-md-3' } },
      ],
    },
    // Float utility
    {
      code: '<Button className="pull-xs-right" />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'pull-xs-right' } }],
    },
    // Text utility
    {
      code: '<p className="text-xs-center" />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'text-xs-center' } }],
    },
    // Template literal with legacy class
    {
      code: '<div className={`${cls} col-xs-12`} />',
      errors: [{ messageId: 'noLegacyClassName', data: { className: 'col-xs-12' } }],
    },
  ],
});
