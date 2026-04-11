# Plan: Replace custom Bootstrap grid with Carbon Grid/Column

## Context

The frontend audit (Docs/carbon-audit.md, Finding #1) identified that `PageShell` implements a custom Bootstrap-style grid system (`col-md-*`, `offset-md-*`, `row`, `container`) instead of using Carbon's `Grid`/`Column` components. This is ~100 lines of custom CSS that Carbon provides out of the box, and it prevents the app from benefiting from Carbon's responsive breakpoint system.

**Goal:** Go fully Carbon-native. No preserved max-widths, no custom container classes, no Bootstrap-isms. Let Carbon's Grid own the layout entirely.

## Column mapping (12-col custom -> 16-col Carbon)

| Layout | Current CSS | Carbon `Column` props |
|--------|------------|----------------------|
| `narrow` | `col-md-6 offset-md-3` (50% centered) | `lg={{span:8, offset:4}} md={{span:6, offset:1}} sm={4}` |
| `wide` | `col-md-10 offset-md-1` (83%) | `lg={{span:14, offset:1}} md={8} sm={4}` |
| `full` | `col-md-12` (100%) | `lg={16} md={8} sm={4}` |
| `two-column` main | `col-md-9` (70%) | `lg={11} md={8} sm={4}` |
| `two-column` sidebar | `col-md-3` (30%) | `lg={5} md={8} sm={4}` |

## Files to change

### Core (PageShell)
- **`App/Client/src/components/PageShell.tsx`** -- Replace custom `columnClasses` with Carbon `Grid`/`Column` imports and a `columnProps` mapping. Banner stays outside Grid (full-width). Title/subtitle get semantic CSS class names instead of `text-xs-center`.
- **`App/Client/src/components/PageShell.css`** -- Gut entirely. Delete all `col-md-*`, `offset-md-*`, `row`, `container.page`, `text-xs-center` rules and the responsive media query. Keep only `.page-shell` (min-height), `.page-shell-banner`, and heading/subtitle margins. No custom max-width -- Carbon Grid handles it.
- **`App/Client/src/components/PageShell.test.tsx`** -- Rewrite 5 tests that query `.col-md-*` selectors. Replace with content/role-based assertions (the tests already have good behavioral tests; only the column layout section needs updating).

### Global CSS cleanup
- **`App/Client/src/index.css`** -- Remove `.container`, `.row`, `.col-xs-12` global rules (lines 50-66). No replacement needed -- Carbon Grid provides all of this.

### Banner components (currently use `<div className="container">`)
- **`App/Client/src/pages/HomePage.tsx`** -- HomeBanner: replace `<div className="container">` with Carbon `<Grid><Column>` for internal layout.
- **`App/Client/src/pages/HomePage.css`** -- Delete all duplicate grid rules (`.container`, `.row`, `.col-md-9`, `.col-md-3`, responsive block). The sidebar column class is also deleted -- PageShell's Carbon Grid handles the two-column layout.
- **`App/Client/src/pages/ArticlePage.tsx`** -- ArticleBanner: replace `<div className="container">` with `<Grid><Column>`. Main content: remove wrapping `<div className="container">` (PageShell Grid handles it).
- **`App/Client/src/pages/ProfilePage.tsx`** -- ProfileBanner: replace `<div className="container"><div className="row"><div className="col-xs-12 col-md-10 offset-md-1">` with Carbon `<Grid><Column lg={{span:14, offset:1}} md={8} sm={4}>`.

### `pull-xs-right` utility class
- **`App/Client/src/pages/LoginPage.tsx`** -- Replace `className="pull-xs-right"` with `className="form-submit-right"`
- **`App/Client/src/pages/RegisterPage.tsx`** -- Same
- **`App/Client/src/pages/SettingsPage.tsx`** -- Same
- **`App/Client/src/pages/EditorPage.tsx`** -- Same
- **`App/Client/src/pages/AuthPages.css`** -- Rename `.pull-xs-right` to `.form-submit-right { float: right; }`
- **`App/Client/src/pages/EditorPage.css`** -- Remove `.text-xs-center`, rename `.pull-xs-right` to `.form-submit-right`
- **`App/Client/src/pages/SettingsPage.css`** -- Remove `.text-xs-center`, rename `.pull-xs-right` to `.form-submit-right`

## Key decisions

1. **No custom max-width.** Let Carbon Grid use its native breakpoint-based max-widths. The layout will go from the current fixed 1140px to Carbon's responsive system (fluid up to 1584px). This is the correct Carbon-native approach.
2. **Banner inner layout uses Carbon Grid.** Banners are full-width (colored backgrounds). Their inner content uses `<Grid><Column>` to stay within Carbon's grid margins, matching the page body alignment.
3. **FlexGrid (default).** Use the default `Grid` component which is FlexGrid in `@carbon/react` v1.x. No need to opt into CSS Grid mode.

## Verification

1. `./build.sh BuildClient --agent` -- must compile cleanly
2. `./build.sh LintClientVerify --agent` -- must pass lint
3. `./build.sh TestClient --agent` -- must pass unit tests (PageShell.test.tsx rewrites)
4. `./build.sh TestE2e --agent` -- must pass E2E tests
5. If screenshot E2E tests fail, regenerate baselines

## Execution order

1. PageShell.tsx + PageShell.css (core migration)
2. index.css global cleanup
3. Banner components (HomePage, ArticlePage, ProfilePage)
4. HomePage.css cleanup
5. pull-xs-right to form-submit-right (4 pages + 3 CSS files)
6. PageShell.test.tsx rewrite
7. Build + lint + test
8. Commit, push from worktree, create PR
