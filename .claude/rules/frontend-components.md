---
paths:
  - "App/Client/**"
---

## Key Hooks

### `useAuth()` — Auth context (NEVER use `useContext(AuthContext)` directly)
Returns: `{ user, loading, login, register, logout, updateUser }`

### `useApiCall<T>()` — Generic async API call with error handling
Returns: `{ data, error, loading, execute(), clearError(), reset() }`
```typescript
const { data, error, loading, execute } = useApiCall(
  async () => articlesApi.getArticles(limit, offset),
  { onSuccess: (result) => setArticles(result.articles) }
);
```

## Page Structure — PageShell (REQUIRED for all pages)

```typescript
<PageShell className="my-page" columnLayout="narrow" title="Page Title" sidebar={<TagSidebar />}>
  {/* Main content */}
</PageShell>
```

PageShell uses Carbon `Grid`/`Column` internally. Layout options: `narrow`, `wide`, `full`, `two-column`

## Carbon Design System

```typescript
import {
  Form, TextInput, TextArea, Button, Loading, InlineNotification,
  Stack, Tabs, TabList, Tab, TabPanels, TabPanel, Tag, Pagination, Modal,
  Grid, Column,
} from '@carbon/react';
import { Add, FavoriteFilled, Favorite, UserFollow, Settings } from '@carbon/icons-react';
```

- `Button`: `kind="primary|secondary|ghost|danger--ghost"`, `size="sm|lg"`, `renderIcon={Icon}`
- `InlineNotification`: `kind="error|success"`, `title`, `subtitle`, `onCloseButtonClick`
- `Loading`: `withOverlay={false}` for inline
- `Tag`: supports `onClick`, `size="sm"`; `Pagination`: for article list pagination
- `OverflowMenu`: use `iconDescription` (NOT `aria-label`) for accessible name

## CSS — Carbon-First

Minimize custom CSS. Use Carbon components, tokens, and layout primitives before writing CSS.
- Colors: `var(--cds-text-primary)`, `var(--cds-link-primary)`, `var(--cds-border-subtle-01)`, etc.
- No hex colors or named colors in CSS (Stylelint-enforced)
- No Bootstrap/legacy class names in JSX (ESLint CBN006-enforced)
- No `float` — use flexbox `align-self` or Carbon `Grid`/`Column`

## Error Handling

```typescript
import { ErrorDisplay } from '../components/ErrorDisplay';
// In JSX: {error && <ErrorDisplay error={error} onClose={clearError} />}
```

## API Module Template

```typescript
import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export const featureApi = {
  getItems: async (params?: { limit?: number; offset?: number }) => {
    try {
      return await getApiClient().api.feature.get({ queryParameters: params });
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
```

## Carbon i18n

`translateWithId` for DataTable/Dropdown: `(id) => t('carbon.${id}')`. Pagination: pass translated text props. See `.claude/rules/i18n.md`.

## Testing (Vitest)

- Mock API modules matching export structure; `vi.clearAllMocks()` in `beforeEach`; `MemoryRouter` for routes
