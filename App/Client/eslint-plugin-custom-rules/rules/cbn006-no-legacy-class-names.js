/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description:
        'Ban Bootstrap/legacy CSS class names — use Carbon Grid and layout components instead (CBN006)',
    },
    schema: [],
    messages: {
      noLegacyClassName:
        'Legacy class name "{{className}}" detected. Use Carbon Grid/Column components and Carbon spacing tokens instead.',
    },
  },
  create(context) {
    const LEGACY_PATTERN =
      /\b(col-(md|xs)-\d+|offset-md-\d+|pull-xs-\w+|text-xs-\w+)\b/g;

    function checkString(node, value) {
      LEGACY_PATTERN.lastIndex = 0;
      let match;
      while ((match = LEGACY_PATTERN.exec(value)) !== null) {
        context.report({
          node,
          messageId: 'noLegacyClassName',
          data: { className: match[0] },
        });
      }
    }

    return {
      Literal(node) {
        if (typeof node.value === 'string') {
          checkString(node, node.value);
        }
      },
      TemplateLiteral(node) {
        for (const quasi of node.quasis) {
          checkString(node, quasi.value.raw);
        }
      },
    };
  },
};
