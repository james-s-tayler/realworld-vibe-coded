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

Layout options: `narrow` (50% centered), `wide` (83%), `full` (100%), `two-column` (70/30 with sidebar)

## Carbon Design System

```typescript
import {
  Form, TextInput, TextArea, Button, Loading, InlineLoading,
  Stack, Tabs, TabList, Tab, TabPanels, TabPanel, Tag, OperationalTag, Pagination, Modal,
} from '@carbon/react';
import { Add, FavoriteFilled, Favorite, UserFollow, Settings } from '@carbon/icons-react';
```

- `Button`: `kind="primary|secondary|ghost|danger--ghost"`, `size="sm|lg"`, `renderIcon={Icon}`
- `InlineLoading`: `status="active"` during submit — conditionally render to replace Button (never text-swap ternaries). For compact forms, render beside the button instead.
- `Loading`: `withOverlay={false}` for inline
- `Tabs`: `Tabs > TabList > Tab` + `TabPanels > TabPanel`
- `Tag`: read-only display, `type="outline"`, `size="sm"`; `OperationalTag`: interactive (click-to-filter), `type="gray"`, `size="sm"`, uses `text` prop not children
- `Pagination`: for article list pagination
- `OverflowMenu`: use `iconDescription` (NOT `aria-label`) to set the trigger button's accessible name. `aria-label` on `<OverflowMenu>` does NOT propagate to the rendered button — Playwright and screen readers won't see it.

## SCSS Classes (from existing stylesheets)

- `.article-preview` — Card container; `.article-meta`, `.author-info`, `.author-image` — Author
- `.article-title`, `.article-description`, `.article-footer` — Content
- `.tag-pill`, `.tag-list` — Tags; `.sidebar` — Sidebar; `.pull-xs-right` — Float right

## Error Handling

```typescript
import { ErrorDisplay } from '../components/ErrorDisplay';
// In JSX: {error && <ErrorDisplay error={error} onClose={clearError} />}
```

`ErrorDisplay` wraps notification rendering internally — never import notification components directly (CBN005).

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

## Testing (Vitest)

- Mock API modules matching their export structure (object with methods)
- `vi.clearAllMocks()` in `beforeEach`; use `MemoryRouter` for route-dependent components
- Carbon components need: `import '@carbon/react'` (auto-available in test setup)
