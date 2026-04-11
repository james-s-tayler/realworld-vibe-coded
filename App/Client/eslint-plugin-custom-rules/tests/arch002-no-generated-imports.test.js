import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/arch002-no-generated-imports.js';

RuleTester.afterAll = afterAll;
RuleTester.describe = describe;
RuleTester.it = it;

const ruleTester = new RuleTester({
  languageOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
  },
});

ruleTester.run('arch002-no-generated-imports', rule, {
  valid: [
    { code: "import { articlesApi } from '../api/articles';" },
    { code: "import { useAuth } from '../hooks/useAuth';" },
    { code: "import { PageShell } from '../components/PageShell';" },
  ],
  invalid: [
    {
      code: "import { SomeModel } from '../api/generated/models';",
      errors: [{ messageId: 'noGeneratedImports' }],
    },
    {
      code: "import { createClient } from '../../api/generated/conduitApiClient';",
      errors: [{ messageId: 'noGeneratedImports' }],
    },
  ],
});
