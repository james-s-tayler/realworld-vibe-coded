# Frontend Carbon Design System Refactor

**Date:** April 11-13, 2026
**Scope:** PRs #658, #668, #672 through #692 (23 pull requests)
**Goal:** Transform the frontend from a Bootstrap-legacy React app into a fully Carbon Design System-native application with automated guardrails to prevent regression.

---

## Table of Contents

1. [Overview](#overview)
2. [Phase 0: Frontend Library Audit (#658, #668)](#phase-0-frontend-library-audit)
3. [Phase 1: Audit and Grid Migration (#672-#673)](#phase-1-audit-and-grid-migration)
4. [Phase 2: Design Token Enforcement (#674)](#phase-2-design-token-enforcement)
5. [Phase 3: Carbon Component Adoption (#675-#678)](#phase-3-carbon-component-adoption)
6. [Phase 4: Guardrail Hardening (#679-#680)](#phase-4-guardrail-hardening)
7. [Phase 5: CSS Cleanup and Carbon Audit Round 2 (#681-#686)](#phase-5-css-cleanup-and-carbon-audit-round-2)
8. [Phase 6: SCSS Audit and Deduplication (#687-#692)](#phase-6-scss-audit-and-deduplication)
9. [Screenshots](#screenshots)
10. [Guardrails Added](#guardrails-added)
11. [Files Deleted](#files-deleted)
12. [Carbon Components Adopted](#carbon-components-adopted)
13. [Changes to .claude/rules](#changes-to-clauderules)
14. [Changes to CLAUDE.md](#changes-to-claudemd)
15. [Impact Summary](#impact-summary)

---

## Overview

The frontend started as a React + Vite application with `<TextInput type="password">` instead of Carbon's `PasswordInput`, empty `labelText=""` accessibility violations, `console.error` instead of user-visible notifications, inline styles, and no custom ESLint enforcement. It was styled with a custom Bootstrap-style 12-column grid, hardcoded hex colors, arbitrary pixel/rem spacing, and manual layout hacks. Over 23 PRs, the codebase was modernized with React 19 patterns, a custom ESLint plugin, toast notifications, every stylesheet was migrated to SCSS with Carbon design tokens, the grid was replaced with Carbon's 16-column `Grid`/`Column`, and automated lint rules were added to prevent regression.

### Before

- `<TextInput type="password">` instead of Carbon `PasswordInput`
- Empty `labelText=""` accessibility violations (10 instances)
- `console.error` for error handling (15 instances) — not visible to users
- Inline `style={{}}` attributes instead of CSS classes
- Deprecated `onKeyPress` event handlers
- No custom ESLint enforcement for Carbon or architectural patterns
- Custom 12-column Bootstrap grid (`col-md-*`, `offset-md-*`, `container`, `row`)
- 14 plain CSS files with hardcoded hex colors, px/rem spacing, font sizes
- Manual `#root` padding for header offset
- `window.confirm()` for destructive actions
- Text-swap buttons during form submission
- No lint enforcement for design tokens
- No code splitting (all pages in initial bundle)

### After

- Carbon `PasswordInput` with built-in visibility toggle
- Proper `labelText` + `hideLabel` on all form inputs (WCAG compliant)
- Toast notification system (`ToastContext`/`useToast`/`ToastContainer`) for user-visible errors
- React 19 `useActionState` + `startTransition` on auth forms
- `React.lazy` code splitting for all 9 page components
- Custom ESLint plugin with 10 rules (ARCH001-003, CBN001-007) + 41 tests
- Carbon 16-column `Grid`/`Column` with responsive breakpoints
- 12 SCSS files using Carbon `$spacing-*`, `var(--cds-*)`, and `@include type.type-style()` tokens
- Carbon `Content` component for header/sidenav offset
- Carbon `Modal` for destructive confirmations
- Carbon `InlineLoading` for form submission states
- Full stylelint enforcement of 5 Carbon token rules (color, spacing, typography, motion duration, motion easing)
- `no-explicit-any` + `simple-import-sort` ESLint rules

---

## Phase 0: Frontend Library Audit

The refactor began with a comprehensive audit of frontend library usage against official documentation for Carbon Design System, React Router v7, React 19, react-i18next, and @microsoft/feature-management. PR #658 implemented the changes on main; PR #668 ported the non-domain-specific subset to the multi-tenant starter template.

### PR #658 — refactor: frontend library audit — accessibility, code splitting, React 19
**Type:** Refactor | **Files:** 56 | **+~900 -~700**

The foundational PR that established modern React patterns and the custom ESLint plugin:

**Carbon PasswordInput**
- Replaced `<TextInput type="password">` with Carbon's `<PasswordInput>` on login, register, settings, and invite user forms (built-in visibility toggle)

**Accessibility (WCAG)**
- Fixed 10 instances of empty `labelText=""` on form inputs — added proper `labelText` + `hideLabel` for screen reader compliance

**React 19 Patterns**
- `useActionState` + `startTransition` on LoginPage and RegisterPage (eliminates controlled-input boilerplate)
- `React.lazy` + `Suspense` code splitting for all 9 page components (reduces initial bundle)

**Error Handling Overhaul**
- Replaced 15 `console.error` calls (invisible to users) with `<InlineNotification>` error states on HomePage, ArticlePage, ProfilePage
- Deduplicated HomePage TabPanels — 3 identical `<TabPanel>` blocks replaced with `.map()`

**Code Quality**
- Replaced all inline `style={{}}` attributes with CSS classes using Carbon spacing tokens
- Replaced deprecated `onKeyPress` with `onKeyDown` on EditorPage tag input
- Fixed `useApiCall` memoization — `useRef` for `onSuccess`/`onError` callbacks to prevent `execute` recreation every render

**Custom ESLint Plugin (`eslint-plugin-custom-rules`) — 8 rules with 41 tests**

| Rule | What it prevents |
|------|-----------------|
| `ARCH001` | Direct `useContext()` in pages/components — must use hook wrappers |
| `ARCH002` | Imports from `api/generated/` outside `src/api/` |
| `ARCH003` | API module imports in components — receive data via props |
| `CBN001` | `<TextInput type="password">` — must use `<PasswordInput>` |
| `CBN002` | Empty `labelText=""` — accessibility violation |
| `CBN003` | Inline `style={{}}` in pages/components — use CSS classes |
| `CBN004` | Deprecated `onKeyPress` — use `onKeyDown` |
| `CBN005` | `<InlineNotification>` — must use `<ToastNotification>` via toast system |

**Toast Notification System**
- `ToastContext` + `ToastProvider` for dispatching toasts
- `useToast` hook for component access
- `ToastContainer` component renders fixed-position toast stack
- Migrated all `InlineNotification` usage to `ToastNotification` across all pages and `ErrorDisplay`

**Infrastructure**
- `enforce-nuke.sh` hook: blocks `npm test` and `npx vitest/eslint/tsc` direct invocations — must use Nuke targets
- Built-in `no-console` ESLint rule for production code
- 35 new i18n translation keys (en + ja)

**Not implemented:** `createBrowserRouter` migration — investigated but Carbon's `HeaderContainer` + `SideNav` overlay loses expanded state under react-router v7's `startTransition`, breaking mobile navigation.

### PR #668 — refactor: port frontend architecture improvements from main
**Type:** Refactor (port) | **Files:** 54 | **+~1050 -~400**

Ported all non-domain-specific changes from PR #658 to the multi-tenant starter template. Identical ESLint plugin, toast system, React 19 patterns, and accessibility fixes. Also fixed a mobile E2E test race condition where `LoginOnMobileAsync` was clicking the hamburger before `useActionState`'s deferred navigation completed.

---

## Phase 1: Audit and Grid Migration

### PR #672 — docs: add Carbon Design System audit report
**Type:** Documentation | **Files:** 1 | **+184 -0**

Initial audit of all `@carbon/react` usage against official Carbon documentation. Identified 7 findings across layout, design tokens, typography, and underused components:

- **P1:** Hardcoded hex colors and spacing should use `--cds-*` design tokens (7 CSS files)
- **P2:** Custom Bootstrap-style grid should migrate to Carbon `Grid`/`Column`
- **P2:** Manual UI Shell content offset should use Carbon `Content` wrapper
- **P3:** Underused components: `InlineLoading`, `Breadcrumb`, `ClickableTile`, Carbon `Form`

### PR #673 — feat: replace custom Bootstrap grid with Carbon Grid/Column
**Type:** Feature | **Files:** 17 | **+132 -264**

Replaced the entire custom 12-column grid system with Carbon's native 16-column `Grid`/`Column`.

| Layout | Old CSS | Carbon Column props |
|--------|---------|---------------------|
| narrow | `col-md-6 offset-md-3` | `lg={span:8, offset:4} md={span:6, offset:1} sm=4` |
| wide | `col-md-10 offset-md-1` | `lg={span:14, offset:1} md=8 sm=4` |
| full | `col-md-12` | `lg=16 md=8 sm=4` |
| two-column main | `col-md-9` | `lg=11 md=8 sm=4` |
| two-column sidebar | `col-md-3` | `lg=5 md=8 sm=4` |

Removed ~170 lines of custom grid CSS. Also fixed a Nuke worktree bug where `+` in directory names broke Docker compose project names.

---

## Phase 2: Design Token Enforcement

### PR #674 — feat: add stylelint for Carbon design token enforcement
**Type:** Feature | **Files:** 44 | **+1549 -284**

The largest single PR of the refactor. Three major changes:

**1. CSS to SCSS migration (all 14 stylesheets)**
- Renamed all `.css` files to `.scss`
- Added `@use '@carbon/react/scss/spacing' as *` for spacing tokens
- Replaced all hardcoded sizes (`1rem`, `2rem`, `0.5rem`) with `$spacing-05`, `$spacing-07`, `$spacing-03`
- Replaced all hardcoded hex colors with `var(--cds-text-primary)`, `var(--cds-link-primary)`, etc.
- Removed deprecated `word-wrap` (kept `overflow-wrap: break-word`)
- Updated deprecated `rgba()` to modern `rgb()` with `/` alpha syntax

**2. stylelint configuration (`.stylelintrc.json`)**
- Extends `stylelint-config-standard-scss` and `stylelint-plugin-carbon-tokens/config/recommended`
- `carbon/theme-use` — enforces Carbon color tokens (error severity)
- `carbon/layout-use` — enforces Carbon spacing tokens (error severity)
- `carbon/type-use` — disabled initially (enabled later in PR #677)

**3. Nuke lint target restructure (`Build.Lint.cs`)**

| Target | Type | Executes |
|--------|------|----------|
| `LintClientEslintVerify` | Leaf | `npm run lint` |
| `LintClientEslintFix` | Leaf | `npm run lint:fix` |
| `LintClientStylelintVerify` | Leaf | `npm run lint:stylelint` |
| `LintClientStylelintFix` | Leaf | `npm run lint:stylelint:fix` |
| `LintClientVerify` | Composite | ESLint + stylelint verify |
| `LintClientFix` | Composite | ESLint + stylelint fix |

**4. New `.claude/rules/research.md`** — codified research-first planning approach.

---

## Phase 3: Carbon Component Adoption

### PR #675 — refactor: replace manual content offset with Carbon Content component
**Type:** Refactor | **Files:** 18 | **+230 -42**

- Replaced manual `#root` padding offset with Carbon's `Content` component (`<main class="cds--content">`)
- Removed custom `--header-height` and `--sidebar-width` CSS properties
- Removed banner negative-margin hacks in HomePage and ArticlePage
- For SideNav offset at lg+ breakpoints, used Carbon's `mini-units(32)` function (needed because `Content`'s sibling selector doesn't match when SideNav uses `isChildOfHeader`)

### PR #676 — feat: add CBN006 lint rule and InlineLoading for form submissions
**Type:** Feature | **Files:** 13 | **+106 -23**

- **New ESLint rule CBN006:** Bans raw `<form>` elements, enforcing Carbon `<Form>` usage
- Replaced text-swap submit button pattern with Carbon `InlineLoading` on all 5 form pages:
  - LoginPage, RegisterPage, EditorPage, SettingsPage, ArticlePage (comments)
- Cleaned up stale `InlineNotification` references in `frontend-components.md`

### PR #677 — refactor: Carbon tag styling + typography systematization
**Type:** Refactor | **Files:** 13 | **+75 -82**

- Replaced custom `.tag-pill` CSS with Carbon `OperationalTag` (`type="gray"`, `size="sm"`) for sidebar tags
- Switched global font from Titillium Web to IBM Plex Sans (Carbon default); retained Titillium only for header brand name
- Replaced 15 hardcoded `font-size` values across 6 SCSS files with Carbon type token mixins (`@include type.type-style(...)`)
- **Enabled `carbon/type-use` stylelint rule** — completed the trifecta: color + spacing + typography enforcement
- Added guardrails principle: all guardrails use error severity, never warning

### PR #678 — feat: add Carbon Breadcrumb navigation, close carbon audit
**Type:** Feature | **Files:** 15 | **+76 -284**

- Added location-based breadcrumbs (`Home > page title`) to ArticlePage and ProfilePage using Carbon `Breadcrumb` with React Router `Link`
- Closed remaining carbon audit items: Breadcrumb (done), ClickableTile (N/A — multiple internal CTAs violate Carbon accessibility guidelines)
- Updated CLAUDE.md with `RunLocalHotReload`, visual verification, and teardown-before-spinup invariants

---

## Phase 4: Guardrail Hardening

### PR #679 — lint: enable Carbon motion stylelint rules
**Type:** Lint | **Files:** 2 | **+17 -0**

- Enabled `carbon/motion-duration-use` and `carbon/motion-easing-use` in `.stylelintrc.json`
- This activated all 5 rules from `stylelint-plugin-carbon-tokens`: theme, layout, type, motion-duration, motion-easing
- Added Carbon's token-first philosophy statement to `.claude/rules/frontend.md`

### PR #680 — feat: add ESLint guardrails and tighten stylelint Carbon token enforcement
**Type:** Feature | **Files:** 74 | **+327 -230**

- **`@typescript-eslint/no-explicit-any`** — bans explicit `any` type annotations (error)
- **`eslint-plugin-simple-import-sort`** — enforces consistent import ordering (auto-fixed all existing files)
- Tightened stylelint `carbon/theme-use` whitelist: removed `rgb()` and `white` exceptions
  - `color: white` in ArticlePreview replaced with `var(--cds-text-on-color)`
  - Hardcoded shadow colors in HomePage replaced with `var(--cds-shadow)`

---

## Phase 5: CSS Cleanup and Carbon Audit Round 2

### PR #681 — fix: remove redundant header offset and dead CSS selectors
**Type:** Fix | **Files:** 11 | **+29 -44**

- Replaced `calc(100vh - 56px)` with `100vh` in AuthPages, SettingsPage, EditorPage (Carbon `Content` already handles header offset)
- Removed 4 dead CSS selectors: `.article-grid-container`, `.article-column-offset`, `.settings-notification`, `.users-modal-notification`

### PR #682 — fix: remove custom CSS overrides for Carbon audit findings #3 and #4
**Type:** Fix | **Files:** 5 | **+331 -15**

- **Finding #3:** Removed `line-height: 1.8` override from `.article-body`, accepting Carbon's `body-02` token value (1.5)
- **Finding #4:** Replaced custom `.favorite-button` CSS (border/background overrides on ghost Button) with Carbon-native `kind` prop toggling: `kind="tertiary"` (unfavorited) → `kind="primary"` (favorited)

### PR #683 — fix: run Vite dev server as detached process
**Type:** Infrastructure fix | **Files:** 1 | **+104 -10**

Fixed the Nuke `build.log` lock contention issue ([nuke-build/nuke#1088](https://github.com/nuke-build/nuke/issues/1088)):
- `RunLocalClient` was blocking Nuke indefinitely via `NpmRun`, holding an exclusive lock on `build.log`
- Refactored to start Vite as a detached bash process with PID tracking
- `RunLocalHotReload` now completes in ~14 seconds then exits
- `RunLocalHotReloadDown` runs as a separate Nuke invocation without lock conflict

### PR #684 — fix: replace hardcoded CSS values with Carbon tokens (#5, #6, #7)
**Type:** Fix | **Files:** 8 | **+39 -22**

- **#5:** Replaced hardcoded pixel image dimensions with `$spacing-07` (32px), `$spacing-06` (24px), `$spacing-12` (100px avatars)
- **#6:** Replaced magic `z-index: 9000` with Carbon's `z.z('modal')` layer function
- **#7:** Replaced hardcoded `8px` shadow dimensions with `$spacing-03`
- **Guardrail:** Extended `carbon/layout-use` to enforce tokens on `width`, `height`, `min-height`, `max-height`, `min-width`, `max-width`

### PR #685 — fix: replace window.confirm with Carbon Modal and adopt Stack component
**Type:** Fix | **Files:** 10 | **+63 -24**

- **Finding #8:** Replaced `window.confirm()` in article deletion with Carbon `<Modal danger>` with i18n support (en/ja)
- **New ESLint rule:** `no-restricted-globals` (CBN007) banning `window.confirm`, `window.alert`, `window.prompt`
- **Finding #9:** Replaced manual flexbox column layouts with Carbon `<Stack>` in ArticlePreview, ArticlePage banner, and UsersPage

### PR #686 — fix: extract shared CSS utilities for avatars, banners, and hr (#11, #12, #13)
**Type:** Fix | **Files:** 11 | **+51 -40**

- **#11:** Added Carbon-tokenized global `<hr>` style; removed dead `.settings-page hr` rule
- **#12:** Added `.avatar-sm`/`.avatar-md`/`.avatar-lg` utility classes, replacing 4 duplicated `border-radius: 50%` + sizing declarations
- **#13:** Added `.page-banner` base class with shared `padding: $spacing-07 0`, removing duplication across 3 page banners

---

## Phase 6: SCSS Audit and Deduplication

### PR #687 — docs: SCSS audit for Carbon Design System improvements
**Type:** Documentation | **Files:** 1 | **+102 -0**

Audit of all 14 SCSS files identifying 10 findings: duplicate class definitions, flex patterns replaceable with `<Stack>`, inconsistent loading states, breadcrumb overflow duplication, and ~70 lines of CSS that could be consolidated.

### PR #688 — fix: replace AppHeader CSS overrides with Carbon Theme component
**Type:** Fix | **Files:** 3 | **+33 -18**

- Removed direct `.cds--header` and `.cds--header__name` CSS overrides (pre-Carbon holdovers)
- Wrapped `<Header>` in `<Theme theme="g90">` for Carbon-native dark header theming

### PR #689 — refactor: deduplicate SCSS shared utilities into index.scss
**Type:** Refactor | **Files:** 14 | **+38 -83**

- Consolidated `.article-meta`, `.tag-list`, breadcrumb overflow, and page padding patterns into `index.scss`
- Unified `.article-tags` and `.tag-list` with consistent `$spacing-03` gap
- Deleted `AuthPages.scss` and `SettingsPage.scss` (only contained the now-global padding rule)
- Net reduction: **-45 lines** of duplicated SCSS

### PR #690 — refactor: replace flex patterns with Carbon Stack and consolidate loading states
**Type:** Refactor | **Files:** 5 | **+13 -25**

- Replaced custom `display: flex; gap` CSS for article page actions with Carbon `<Stack orientation="horizontal" gap={3}>`
- Consolidated identical `.article-page.loading` and `.profile-page.loading` into shared `.page-loading` utility

### PR #691 — feat: consistent page layout spacing and branded header
**Type:** Feature | **Files:** 13 | **+38 -54**

- Replaced per-page banners with primary blue header background using `--cds-button-primary` token
- Added consistent top padding (`$spacing-07`) to all pages via centralized `PageShell.scss`
- Removed per-page padding overrides
- Added "Home" title to HomePage (with i18n) — all pages now have titles
- Standardized form pages to `wide` columnLayout per Carbon form alignment guidelines

### PR #692 — fix: remove legacy page padding that doubled editor page spacing
**Type:** Fix | **Files:** 1 | **+0 -6**

- Removed dead CSS rule in `index.scss` applying `$spacing-06` padding to `.editor-page`, `.auth-page`, `.settings-page`
- This padding stacked on top of PageShell's `$spacing-07`, causing 56px gap on editor vs 32px on other pages
- `.auth-page` and `.settings-page` selectors were dead code (no component uses those class names)

---

## Screenshots

All screenshots captured from the running application after the refactor was complete.

### Authentication (Unauthenticated)

| Page | Screenshot |
|------|------------|
| Login | ![Login Page](screenshots/01-login-page.png) |
| Register | ![Register Page](screenshots/02-register-page.png) |

### Home Page (Authenticated)

| View | Screenshot |
|------|------------|
| Your Feed | ![Home - Your Feed](screenshots/03-home-your-feed.png) |
| Global Feed | ![Home - Global Feed](screenshots/04-home-global-feed.png) |
| Tag Filter | ![Home - Tag Filter](screenshots/05-home-tag-filter.png) |

### Article

| View | Screenshot |
|------|------------|
| Article Page (with breadcrumb, comments, tags) | ![Article Page](screenshots/06-article-page.png) |
| Article Not Found | ![Article Not Found](screenshots/13-article-not-found.png) |

### Editor

| View | Screenshot |
|------|------------|
| New Article | ![Editor - New Article](screenshots/07-editor-new-article.png) |
| Edit Article (with tags) | ![Editor - Edit Article](screenshots/08-editor-edit-article.png) |

### Settings

| View | Screenshot |
|------|------------|
| User Settings | ![Settings Page](screenshots/09-settings-page.png) |

### Profile

| View | Screenshot |
|------|------------|
| My Articles | ![Profile - My Articles](screenshots/10-profile-page.png) |
| Favorited Articles | ![Profile - Favorited](screenshots/11-profile-favorited.png) |

### Admin

| View | Screenshot |
|------|------------|
| Users Management | ![Users Page](screenshots/12-users-page.png) |

---

## Guardrails Added

### Stylelint — Carbon Token Enforcement (`.stylelintrc.json`)

All 5 rules from `stylelint-plugin-carbon-tokens` are now active at error severity:

| Rule | Enforces | Added in |
|------|----------|----------|
| `carbon/theme-use` | Carbon color tokens (`var(--cds-*)`) | PR #674 |
| `carbon/layout-use` | Carbon spacing tokens (`$spacing-*`) | PR #674 |
| `carbon/type-use` | Carbon typography mixins (`type.type-style()`) | PR #677 |
| `carbon/motion-duration-use` | Carbon motion duration tokens | PR #679 |
| `carbon/motion-easing-use` | Carbon motion easing tokens | PR #679 |

The `carbon/layout-use` rule was later extended in PR #684 to also cover `width`, `height`, `min-width`, `max-width`, `min-height`, `max-height`.

The `carbon/theme-use` whitelist was tightened in PR #680 (removed `rgb()` and `white`).

### ESLint — Custom Plugin (`eslint-plugin-custom-rules`)

10 custom rules with full test coverage, created across PRs #658 and #676:

| Rule | What it prevents | Added in |
|------|-----------------|----------|
| `ARCH001` | Direct `useContext()` — must use hook wrappers | PR #658 |
| `ARCH002` | Imports from `api/generated/` outside `src/api/` | PR #658 |
| `ARCH003` | API module imports in components — receive data via props | PR #658 |
| `CBN001` | `<TextInput type="password">` — must use `<PasswordInput>` | PR #658 |
| `CBN002` | Empty `labelText=""` — accessibility violation | PR #658 |
| `CBN003` | Inline `style={{}}` in pages/components — use CSS classes | PR #658 |
| `CBN004` | Deprecated `onKeyPress` — use `onKeyDown` | PR #658 |
| `CBN005` | `<InlineNotification>` — must use toast system | PR #658 |
| `CBN006` | Raw `<form>` elements — must use Carbon `<Form>` | PR #676 |
| `CBN007` (`no-restricted-globals`) | `window.confirm`, `window.alert`, `window.prompt` | PR #685 |

### ESLint — Built-in Rules

| Rule | What it prevents | Added in |
|------|-----------------|----------|
| `no-console` | `console.*` in production code | PR #658 |
| `@typescript-eslint/no-explicit-any` | Explicit `any` type annotations | PR #680 |
| `simple-import-sort/imports` | Inconsistent import ordering | PR #680 |
| `simple-import-sort/exports` | Inconsistent export ordering | PR #680 |

### Infrastructure Guardrails

| Guardrail | What it prevents | Added in |
|-----------|-----------------|----------|
| `enforce-nuke.sh` hook | Direct `npm test`, `npx vitest/eslint/tsc` — must use Nuke targets | PR #658 |

### Nuke Lint Target Restructure

`LintClientVerify` and `LintClientFix` became composite targets over separate ESLint and stylelint leaf targets (PR #674), enabling independent execution and clearer CI output.

### Guardrails Principle

Added to `.claude/rules/guardrails.md` (PR #677): "All guardrails use error severity — never warning. A guardrail that doesn't block is just a suggestion."

---

## Files Deleted

| File | Reason | PR |
|------|--------|----|
| `App.css` | Dead Vite boilerplate, never imported | #674 |
| `AuthPages.scss` | Only contained global padding rule (moved to `PageShell.scss`) | #689, #691 |
| `SettingsPage.scss` | Only contained global padding rule (moved to `PageShell.scss`) | #689, #691 |

14 CSS files were renamed to SCSS (PR #674). Net result: 12 SCSS files remaining.

---

## Carbon Components Adopted

| Component | Replaces | PR |
|-----------|----------|----|
| `PasswordInput` | `<TextInput type="password">` | #658 |
| `ToastNotification` | `InlineNotification` for error display | #658 |
| `Grid` / `Column` | Custom Bootstrap `col-md-*` grid | #673 |
| `Content` | Manual `#root` padding for header offset | #675 |
| `Form` | Raw `<form>` elements | #676 |
| `InlineLoading` | Text-swap submit buttons | #676 |
| `OperationalTag` | Custom `.tag-pill` CSS | #677 |
| `Breadcrumb` | No breadcrumbs existed | #678 |
| `Modal` (danger) | `window.confirm()` | #685 |
| `Stack` | Manual flexbox column layouts | #685, #690 |
| `Theme` (g90) | Manual CSS overrides for dark header | #688 |

---

## Changes to .claude/rules

### New Files

| File | Contents | PR |
|------|----------|----|
| `research.md` | Research-first planning — verify framework assumptions against docs before proposing plans | #674 |

### Modified Files

| File | Change | PR |
|------|--------|----|
| `e2e.md` | Added toast notification locator conventions, error-display to toast-error selectors | #658 |
| `frontend.md` | Added Carbon Design System styling rules section: SCSS-only spacing tokens, CSS custom property colors, typography mixins, IBM Plex Sans font, token-first philosophy, "never write direct CSS overrides for Carbon components" | #674, #677, #679 |
| `frontend-components.md` | Cleaned up stale `InlineNotification` references (contradicted CBN005), added `InlineLoading`, `OperationalTag`, `Modal`, `Stack` usage patterns | #676, #677, #685 |
| `guardrails.md` | Added principle: "All guardrails use error severity — never warning" | #677 |

---

## Changes to CLAUDE.md

| Change | PR |
|--------|----|
| Added `RunLocalHotReload` and `RunLocalPublish` as primary local dev commands | #678 |
| Added invariant: "Verify frontend changes visually" — run the app and use Chrome DevTools MCP | #678 |
| Added invariant: "`RunLocal*` lifecycle" — always tear down before spinning up | #678 |
| Added stylelint Carbon Tokens to guardrail classes table | #674 |
| Updated `LintClientVerify`/`LintClientFix` descriptions to reflect composite structure | #674 |

---

## Impact Summary

| Metric | Value |
|--------|-------|
| Total PRs | 23 |
| Files changed (cumulative) | ~340 |
| Lines added (cumulative) | ~5,525 |
| Lines removed (cumulative) | ~2,690 |
| Net new lines | ~2,835 |
| CSS files eliminated | 3 (App.css, AuthPages.scss, SettingsPage.scss) |
| Hardcoded hex colors removed | All |
| Hardcoded spacing values removed | All |
| Hardcoded font-size values removed | All |
| Accessibility violations fixed | 10 (empty labelText) |
| console.error calls replaced | 15 (with toast notifications) |
| Stylelint rules active | 5/5 (all at error severity) |
| Custom ESLint rules | 10 (ARCH001-003, CBN001-007) with 41+ tests |
| Additional ESLint rules | 4 (no-console, no-explicit-any, import-sort x2) |
| Carbon components adopted | 11 new component types |
| Custom grid CSS removed | ~170 lines |
| Pages with code splitting | 9/9 (React.lazy) |
| E2E test regressions | 0 (84/84 passing throughout) |
