import cbn001 from './rules/cbn001-no-password-textinput.js';
import cbn002 from './rules/cbn002-no-empty-label-text.js';
import cbn003 from './rules/cbn003-no-inline-styles.js';
import cbn004 from './rules/cbn004-no-on-key-press.js';
import cbn005 from './rules/cbn005-no-inline-notification.js';
import cbn006 from './rules/cbn006-no-raw-form.js';
import arch001 from './rules/arch001-no-direct-use-context.js';
import arch002 from './rules/arch002-no-generated-imports.js';
import arch003 from './rules/arch003-no-api-in-components.js';
import cbn007 from './rules/cbn007-no-js-text-truncation.js';
import tst001 from './rules/tst001-no-user-event-type.js';

const plugin = {
  meta: {
    name: 'eslint-plugin-custom-rules',
    version: '1.0.0',
  },
  rules: {
    'cbn001-no-password-textinput': cbn001,
    'cbn002-no-empty-label-text': cbn002,
    'cbn003-no-inline-styles': cbn003,
    'cbn004-no-on-key-press': cbn004,
    'cbn005-no-inline-notification': cbn005,
    'cbn006-no-raw-form': cbn006,
    'cbn007-no-js-text-truncation': cbn007,
    'arch001-no-direct-use-context': arch001,
    'arch002-no-generated-imports': arch002,
    'arch003-no-api-in-components': arch003,
    'tst001-no-user-event-type': tst001,
  },
};

export default plugin;
