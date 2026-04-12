/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description:
        'Ban userEvent.type() in tests — use paste() to avoid per-keystroke flakiness under CI load (TST001)',
    },
    schema: [],
    messages: {
      noUserEventType:
        'userEvent.type() fires per-character DOM events and causes CI timeouts. Use user.click(input) + user.paste(value) instead.',
    },
  },
  create(context) {
    return {
      CallExpression(node) {
        const callee = node.callee;
        if (
          callee.type === 'MemberExpression' &&
          callee.property.type === 'Identifier' &&
          callee.property.name === 'type' &&
          callee.object.type === 'Identifier' &&
          callee.object.name === 'user'
        ) {
          context.report({ node, messageId: 'noUserEventType' });
        }
      },
    };
  },
};
