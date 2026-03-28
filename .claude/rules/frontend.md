---
paths:
  - "App/Client/**"
---

## Kiota Bridge — Backend → Generated Client → Frontend

The generated API client (`src/api/generated/`) is the **type contract** between backend and frontend. The workflow is strictly ordered:

1. **Backend first** — implement/modify endpoints in `Server.Web/`
2. **Build server** — `./build.sh BuildServer` compiles the backend AND regenerates the Kiota TypeScript client (transitive dependency)
3. **Frontend second** — only then use the generated types in React components and API modules

**Never** reference properties in frontend code that don't exist in the generated client types. If a backend endpoint returns a new field (e.g., `following` on Profile), you must regenerate before the frontend can use it. The `BuildClient` target depends on `BuildGenerateApiClient` which depends on `BuildServer`, so running `./build.sh BuildClient` always produces fresh types.

## Project Structure

- `src/api/` — API client modules, one per domain (e.g., `articlesApi`, `commentsApi`)
- `src/api/generated/` — Kiota auto-generated. NEVER edit manually. Regenerate: `./build.sh BuildGenerateApiClient`
- `src/api/clientFactory.ts` — Kiota `ConduitApiClient` singleton with cookie auth + CSRF
- `src/api/errors.ts` — `convertKiotaError()` converts Kiota errors to `ApiError`
- `src/api/client.ts` — Only `ApiError` class
- `src/components/` — Reusable UI (Carbon Design System)
- `src/context/` — React Context providers (types in `*Type.ts`)
- `src/hooks/` — Custom hooks (`useAuth`, `useApiCall`)
- `src/pages/` — Route-level page components
- `src/types/` — TypeScript type definitions
- `src/constants.ts` — Constraint constants (ARTICLE_CONSTRAINTS, TAG_CONSTRAINTS, etc.)

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
import { PageShell } from '../components/PageShell';

export const MyPage: React.FC = () => (
  <PageShell
    className="my-page"
    columnLayout="narrow"    // "narrow"|"wide"|"full"|"two-column"
    title="Page Title"       // optional h1
    sidebar={<TagSidebar />} // only with "two-column"
  >
    {/* Main content */}
  </PageShell>
);
```

Layout options: `narrow` (50% centered), `wide` (83%), `full` (100%), `two-column` (70/30 with sidebar)

## Carbon Design System — Common Imports

```typescript
import {
  Form, TextInput, TextArea, Button, Loading, InlineNotification,
  Stack, Tabs, TabList, Tab, TabPanels, TabPanel, Tag, Pagination,
  Modal,
} from '@carbon/react';
import { Add, FavoriteFilled, Favorite, UserFollow, Settings } from '@carbon/icons-react';
```

Key props:
- `Button`: `kind="primary|secondary|ghost|danger--ghost"`, `size="sm|lg"`, `renderIcon={Icon}`, `disabled`
- `InlineNotification`: `kind="error|success"`, `title`, `subtitle`, `onCloseButtonClick`
- `Loading`: `withOverlay={false}` for inline
- `Tabs`: Use `Tabs > TabList > Tab` + `TabPanels > TabPanel` for feed tabs
- `Tag`: Use `Tag` component for tags (supports `onClick`, `size="sm"`)
- `Pagination`: Carbon `Pagination` component for article list pagination

## CSS Classes (already defined in existing stylesheets)

Article components use these CSS classes (defined in ArticlePage.css, ProfilePage.css):
- `.article-preview` — Article card container
- `.article-meta`, `.author-info`, `.author-image`, `.author-details` — Author section
- `.article-title`, `.article-description`, `.article-footer` — Content
- `.tag-pill`, `.tag-list` — Tag display
- `.sidebar` — Sidebar container (used with PageShell two-column)
- `.pull-xs-right` — Float right utility

## Error Handling

```typescript
import { ErrorDisplay } from '../components/ErrorDisplay';

// In component:
{error && <ErrorDisplay error={error} onClose={clearError} />}
```

`ErrorDisplay` renders `InlineNotification` with normalized error messages.

## API Module Template

```typescript
// src/api/articles.ts
import { getApiClient } from './clientFactory';
import { convertKiotaError } from './errors';

export const articlesApi = {
  getArticles: async (params?: { limit?: number; offset?: number; tag?: string; author?: string; favorited?: string }) => {
    try {
      const result = await getApiClient().api.articles.get({
        queryParameters: params,
      });
      return result as { articles: Article[]; articlesCount: number };
    } catch (error) {
      return convertKiotaError(error);
    }
  },
  createArticle: async (data: CreateArticleData) => {
    try {
      const result = await getApiClient().api.articles.post({ article: data });
      return result as { article: Article };
    } catch (error) {
      return convertKiotaError(error);
    }
  },
};
```

## Routing

- Routes defined in `App.tsx` via `react-router`
- Use `<ProtectedRoute>` wrapper for authenticated pages
- Use `useNavigate()`, `useParams()`, `Link` from `react-router`

## Testing (Vitest)

- Mock API modules matching their export structure (object with methods)
- `vi.clearAllMocks()` in `beforeEach`
- Use `MemoryRouter` for components requiring routes
- Carbon components need: `import '@carbon/react'` (auto-available in test setup)

## State Management

- Use `useAuth` hook for auth state
- Local state (`useState`) for page-level data
- `useApiCall` handles loading/error/data lifecycle
- No global state library — each page fetches its own data
