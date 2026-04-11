import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/arch003-no-api-in-components.js';

RuleTester.afterAll = afterAll;
RuleTester.describe = describe;
RuleTester.it = it;

const ruleTester = new RuleTester({
  languageOptions: {
    ecmaVersion: 2020,
    sourceType: 'module',
  },
});

ruleTester.run('arch003-no-api-in-components', rule, {
  valid: [
    { code: "import { PageShell } from '../components/PageShell';" },
    { code: "import { useAuth } from '../hooks/useAuth';" },
    { code: "import { Article } from '../types/article';" },
  ],
  invalid: [
    {
      code: "import { articlesApi } from '../api/articles';",
      errors: [{ messageId: 'noApiInComponents' }],
    },
    {
      code: "import { ApiError } from '../api/client';",
      errors: [{ messageId: 'noApiInComponents' }],
    },
  ],
});
