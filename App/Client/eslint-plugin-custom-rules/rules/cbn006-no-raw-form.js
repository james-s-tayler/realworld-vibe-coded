/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban <form> — use Carbon <Form> instead (CBN006)',
    },
    schema: [],
    messages: {
      noRawForm:
        'Use <Form> from @carbon/react instead of raw <form>. Carbon Form provides consistent styling and accessibility.',
    },
  },
  create(context) {
    return {
      JSXOpeningElement(node) {
        if (node.name.type !== 'JSXIdentifier') return;
        if (node.name.name !== 'form') return;
        context.report({ node, messageId: 'noRawForm' });
      },
    };
  },
};
