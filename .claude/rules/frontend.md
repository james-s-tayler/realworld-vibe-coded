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

**All custom styles use SCSS (`.scss` files), not plain CSS.** This enables direct use of Carbon's SCSS tokens.

- **Spacing:** `@use '@carbon/react/scss/spacing' as *;` then use `$spacing-05`, `$spacing-07`, etc. These are SCSS-only — Carbon does NOT emit `--cds-spacing-*` CSS custom properties.
- **Colors:** Use `var(--cds-text-primary)`, `var(--cds-layer-01)`, etc. These ARE real CSS custom properties emitted by Carbon's theme system.
- **Typography:** `@use '@carbon/react/scss/type' as type;` then use `@include type.type-style('heading-03');`. SCSS mixins only — sets font-size, weight, line-height, letter-spacing together. Never set font-size/font-weight individually.
- **Font:** IBM Plex Sans globally (from Carbon). No custom fonts.
- **Philosophy:** Express all visual decisions through Carbon tokens and mixins, not raw CSS values. Tokens are the API contract; raw values bypass the design system.
- **Never use hard-coded hex colors, arbitrary rem/px spacing, or arbitrary font-size values.** `stylelint-plugin-carbon-tokens` enforces all three (`carbon/theme-use`, `carbon/layout-use`, `carbon/type-use`) via `LintClientStylelintVerify`.

**Never write direct CSS overrides for Carbon components.** Before adding custom CSS for colors, theming, hover states, or layout of any Carbon component:
1. Research the Carbon-native approach first (e.g., `Theme` component for scoped theming, component props like `kind`, `size`, design tokens)
2. Use Carbon's `Theme` component (`theme="g100"`, `"g90"`, `"g10"`, `"white"`) for dark/light zones — not manual background/color overrides
3. Only add custom CSS for layout concerns (positioning, sizing) that Carbon doesn't handle

## Common Gotchas

- **Carbon Tabs:** `selectedIndex={-1}` doesn't work — compute the proper tab index for dynamic tab sets
- **Kiota body-less PUT/POST:** Kiota sends no body and no `Content-Type` header for endpoints without a request body defined in the OpenAPI spec. FastEndpoints rejects these with 415. The fix is on the backend — annotate all route-bound properties with `[RouteParam]` so FastEndpoints accepts `*/*`. Never bypass Kiota with raw `fetch` for this — fix the root cause.

## Frontend Implementation Checklist

Before implementing a frontend page/component:
1. Check generated types in `src/api/generated/models/` — know what fields are available
2. Check existing components in `src/components/` — reuse before creating new ones
3. Check existing API modules in `src/api/` — extend before creating new ones
4. Check existing SCSS classes in stylesheets — use before adding new ones

After implementing:
1. Verify with `./build.sh BuildClient` (auto-regenerates Kiota types)
2. Run `./build.sh TestE2e` to validate against E2E expectations
