/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban API module imports in components — receive data via props (ARCH003)',
    },
    schema: [],
    messages: {
      noApiInComponents:
        'Components should not import API modules directly. Receive data via props instead.',
    },
  },
  create(context) {
    return {
      ImportDeclaration(node) {
        const source = node.source.value;
        if (/\/api\//.test(source) && !source.includes('api/generated')) {
          context.report({ node, messageId: 'noApiInComponents' });
        }
      },
    };
  },
};
