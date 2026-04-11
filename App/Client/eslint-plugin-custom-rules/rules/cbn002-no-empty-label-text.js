/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban empty labelText="" on form components — accessibility violation (CBN002)',
    },
    schema: [],
    messages: {
      noEmptyLabelText:
        'Empty labelText is an accessibility violation (WCAG). Provide a translated label and use hideLabel to visually hide it.',
    },
  },
  create(context) {
    return {
      JSXAttribute(node) {
        if (node.name.name !== 'labelText') return;

        const { value } = node;
        if (!value) return;

        const isEmpty =
          (value.type === 'Literal' && value.value === '') ||
          (value.type === 'JSXExpressionContainer' &&
            value.expression.type === 'Literal' &&
            value.expression.value === '');

        if (isEmpty) {
          context.report({ node, messageId: 'noEmptyLabelText' });
        }
      },
    };
  },
};
