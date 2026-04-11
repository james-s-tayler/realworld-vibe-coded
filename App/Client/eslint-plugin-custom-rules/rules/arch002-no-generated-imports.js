/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban imports from api/generated/ outside src/api/ — use wrapper modules (ARCH002)',
    },
    schema: [],
    messages: {
      noGeneratedImports:
        'Don\'t import from api/generated/ directly. Use the typed API wrapper modules in src/api/ instead.',
    },
  },
  create(context) {
    return {
      ImportDeclaration(node) {
        if (node.source.value.includes('api/generated')) {
          context.report({ node, messageId: 'noGeneratedImports' });
        }
      },
    };
  },
};
