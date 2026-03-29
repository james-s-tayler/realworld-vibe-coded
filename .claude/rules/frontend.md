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

## Routing

- Routes defined in `App.tsx` via `react-router`
- Use `<ProtectedRoute>` wrapper for authenticated pages
- Use `useNavigate()`, `useParams()`, `Link` from `react-router`

## State Management

- Use `useAuth` hook for auth state
- Local state (`useState`) for page-level data
- `useApiCall` handles loading/error/data lifecycle
- No global state library — each page fetches its own data

## Constraint Constants Sync

`src/constants.ts` mirrors backend entity constraint constants. Use these for `maxLength` on `TextInput`/`TextArea` and client-side validation. The values must match `SPEC-REFERENCE.md` entity definitions. When creating forms, always apply `maxCount` on `TextArea` and `maxLength` on `TextInput` using these constants.

## Carbon Design System — Styling Rules

**Never write direct CSS overrides for Carbon components.** Before adding custom CSS for colors, theming, hover states, or layout of any Carbon component:
1. Research the Carbon-native approach first (e.g., `Theme` component for scoped theming, component props like `kind`, `size`, design tokens)
2. Use Carbon's `Theme` component (`theme="g100"`, `"g90"`, `"g10"`, `"white"`) for dark/light zones — not manual background/color overrides
3. Only add custom CSS for layout concerns (positioning, sizing, spacing) that Carbon doesn't handle

## Common Gotchas

- **Carbon Tabs:** `selectedIndex={-1}` doesn't work — compute the proper tab index for dynamic tab sets

## Frontend Implementation Checklist

Before implementing a frontend page/component:
1. Check generated types in `src/api/generated/models/` — know what fields are available
2. Check existing components in `src/components/` — reuse before creating new ones
3. Check existing API modules in `src/api/` — extend before creating new ones
4. Check existing CSS classes in stylesheets — use before adding new ones

After implementing:
1. Verify with `./build.sh BuildClient` (auto-regenerates Kiota types)
2. Run `./build.sh TestE2e` to validate against E2E expectations
