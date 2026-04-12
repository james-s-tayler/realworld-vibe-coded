/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description:
        'Ban JS text truncation — use CSS text-overflow via Carbon\'s cds--text-truncate-end class instead (CBN007)',
    },
    schema: [],
    messages: {
      noJsTruncation:
        'Use CSS text-overflow (cds--text-truncate-end class) instead of JS truncation for display text.',
    },
  },
  create(context) {
    return {
      CallExpression(node) {
        const name =
          node.callee.type === 'Identifier' ? node.callee.name : null;
        if (name !== 'truncateText' && name !== 'truncateUsername') return;
        context.report({ node, messageId: 'noJsTruncation' });
      },
    };
  },
};
