/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban inline style={{}} attributes — use CSS classes with Carbon tokens (CBN003)',
    },
    schema: [],
    messages: {
      noInlineStyles:
        'Inline styles bypass Carbon design tokens. Use a CSS class with Carbon spacing variables instead.',
    },
  },
  create(context) {
    return {
      JSXAttribute(node) {
        if (node.name.name !== 'style') return;
        context.report({ node, messageId: 'noInlineStyles' });
      },
    };
  },
};
