/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban deprecated onKeyPress — use onKeyDown instead (CBN004)',
    },
    schema: [],
    messages: {
      noOnKeyPress:
        'onKeyPress is deprecated. Use onKeyDown instead.',
    },
  },
  create(context) {
    return {
      JSXAttribute(node) {
        if (node.name.name !== 'onKeyPress') return;
        context.report({ node, messageId: 'noOnKeyPress' });
      },
    };
  },
};
