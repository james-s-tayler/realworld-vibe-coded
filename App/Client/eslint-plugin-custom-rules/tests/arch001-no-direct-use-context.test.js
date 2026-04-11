import { RuleTester } from 'eslint';
import { describe, it, afterAll } from 'vitest';
import rule from '../rules/arch001-no-direct-use-context.js';

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

ruleTester.run('arch001-no-direct-use-context', rule, {
  valid: [
    { code: 'const auth = useAuth();' },
    { code: 'const [state, setState] = useState(null);' },
    { code: 'const memo = useMemo(() => value, [value]);' },
  ],
  invalid: [
    {
      code: 'const ctx = useContext(AuthContext);',
      errors: [{ messageId: 'noDirectUseContext' }],
    },
    {
      code: 'const flags = useContext(FeatureFlagContext);',
      errors: [{ messageId: 'noDirectUseContext' }],
    },
  ],
});
