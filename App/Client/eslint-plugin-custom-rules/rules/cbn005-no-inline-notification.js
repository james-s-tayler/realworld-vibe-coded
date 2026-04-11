/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban <InlineNotification> — use <ToastNotification> or <ActionableNotification> instead (CBN005)',
    },
    schema: [],
    messages: {
      noInlineNotification:
        'Use <ToastNotification> or <ActionableNotification> from @carbon/react instead of <InlineNotification>.',
    },
  },
  create(context) {
    return {
      JSXOpeningElement(node) {
        if (node.name.type !== 'JSXIdentifier') return;
        if (node.name.name !== 'InlineNotification') return;
        context.report({ node, messageId: 'noInlineNotification' });
      },
    };
  },
};
