/** @type {import('eslint').Rule.RuleModule} */
export default {
  meta: {
    type: 'problem',
    docs: {
      description: 'Ban <TextInput type="password"> — use <PasswordInput> instead (CBN001)',
    },
    schema: [],
    messages: {
      noPasswordTextInput:
        'Use <PasswordInput> from @carbon/react instead of <TextInput type="password">. PasswordInput provides a built-in visibility toggle.',
    },
  },
  create(context) {
    return {
      JSXOpeningElement(node) {
        if (node.name.type !== 'JSXIdentifier' || node.name.name !== 'TextInput') return;

        const typeAttr = node.attributes.find(
          (attr) =>
            attr.type === 'JSXAttribute' &&
            attr.name.name === 'type' &&
            getStringValue(attr.value) === 'password',
        );

        if (typeAttr) {
          context.report({ node, messageId: 'noPasswordTextInput' });
        }
      },
    };
  },
};

function getStringValue(value) {
  if (!value) return null;
  if (value.type === 'Literal') return value.value;
  if (
    value.type === 'JSXExpressionContainer' &&
    value.expression.type === 'Literal'
  ) {
    return value.expression.value;
  }
  return null;
}
