/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban direct useContext() in pages/components — use typed hook wrappers (ARCH001)',
    },
    schema: [],
    messages: {
      noDirectUseContext:
        'Don\'t use useContext() directly in pages or components. Use a typed hook wrapper (e.g., useAuth()) instead.',
    },
  },
  create(context) {
    return {
      CallExpression(node) {
        if (node.callee.type !== 'Identifier' || node.callee.name !== 'useContext') return;
        context.report({ node, messageId: 'noDirectUseContext' });
      },
    };
  },
};
