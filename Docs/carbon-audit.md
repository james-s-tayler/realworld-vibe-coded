# Carbon Design System Audit

**Date:** 2026-04-11
**Package:** `@carbon/react@^1.92.1`
**Overall:** Good component adoption, significant layout & token gaps

## Overall Assessment

The codebase correctly uses Carbon React components for all interactive UI elements (forms, buttons, tables, notifications, tabs, pagination). Where it falls short is in **layout**, **design tokens**, and a handful of **underused components**.

### Components in Use

| Category | Components |
|----------|-----------|
| Layout | Header, HeaderContainer, HeaderMenuButton, HeaderName, SkipToContent, Theme, SideNav, SideNavItems, SideNavLink |
| Forms | Form, TextInput, PasswordInput, TextArea, Dropdown |
| Buttons | Button, IconButton |
| Data Display | DataTable, TableContainer, Table, TableHead, TableRow, TableHeader, TableBody, TableCell, Tag |
| Navigation | Tabs, TabList, Tab, TabPanels, TabPanel, Pagination |
| Feedback | ToastNotification, Modal |
| Layout Blocks | Tile, Stack |
| Loading State | Loading, SkeletonText |
| Selection | Checkbox |
| Menus | OverflowMenu, OverflowMenuItem |

### Icons in Use

Home, Edit, UserMultiple, Settings, UserAvatar, Login, UserFollow, Logout, Add, FavoriteFilled, Favorite, TrashCan

---

## Findings

### 1. Layout: Custom Bootstrap-style grid instead of Carbon Grid

**Impact: HIGH** | Files: `PageShell.tsx`, `PageShell.css`, `index.css`, `HomePage.css`

The app rolls its own 12-column grid system with `col-md-*`, `offset-md-*`, and `row` classes -- effectively a mini-Bootstrap. Carbon provides `Grid` and `Column` components with a 16-column CSS Grid, responsive breakpoints, and built-in gutters.

**Current** (`PageShell.tsx:23-28`):
```tsx
const columnClasses = {
  narrow: 'col-md-6 offset-md-3',
  wide: 'col-md-10 offset-md-1',
  full: 'col-md-12',
  'two-column': 'col-md-9',
};
```

**Carbon equivalent:**
```tsx
import { Grid, Column } from '@carbon/react';
<Grid fullWidth>
  <Column lg={8} md={6} sm={4}>{children}</Column>   {/* narrow */}
  <Column lg={14} md={8} sm={4}>{children}</Column>   {/* wide */}
  <Column lg={16} md={8} sm={4}>{children}</Column>   {/* full */}
</Grid>
```

**Benefits:** Responsive breakpoints (sm/md/lg/xl/max) come free, sub-grid support, consistent gutters, and eliminates ~100 lines of custom CSS.

---

### 2. Hard-coded colors instead of Carbon design tokens

**Impact: HIGH** | Files: `index.css`, `ArticlePreview.css`, `ProfilePage.css`, `TagList.css`, `AppHeader.css`, `HomePage.css`

Many CSS files use raw hex values where Carbon tokens exist:

| File | Hard-coded | Carbon token equivalent |
|------|-----------|------------------------|
| `index.css:9` | `#373a3c` | `var(--cds-text-primary)` |
| `index.css:10` | `#ffffff` | `var(--cds-background)` |
| `index.css:36` | `#5cb85c` (link color) | `var(--cds-link-primary)` or custom brand token |
| `ArticlePreview.css:3` | `#e5e5e5` (border) | `var(--cds-border-subtle-01)` |
| `ArticlePreview.css:42,90` | `#bbb` (muted text) | `var(--cds-text-helper)` |
| `ArticlePreview.css:33,65` | `#373a3c`, `#999` | `var(--cds-text-primary)`, `var(--cds-text-secondary)` |
| `ProfilePage.css:13` | `#f3f3f3` (banner bg) | `var(--cds-layer-01)` |
| `ProfilePage.css:35` | `#999` | `var(--cds-text-secondary)` |
| `TagList.css:9` | `#818181` (tag bg) | `var(--cds-icon-secondary)` or use Carbon `Tag` `type` prop |
| `HomePage.css:57` | `#f3f3f3` (sidebar) | `var(--cds-layer-01)` |
| `AppHeader.css:2-3` | `#f4f4f4`, `#e0e0e0` | `var(--cds-layer-01)`, `var(--cds-border-subtle-01)` |

**Why this matters:** Using tokens enables dark mode support and theme switching. `ArticlePage.css` already does this correctly (e.g., `var(--cds-background-inverse)`). The rest of the CSS should follow that pattern.

---

### 3. Custom font instead of IBM Plex Sans

**Impact: MEDIUM** | File: `index.css:1,6`

The app imports `Titillium Web` from Google Fonts and sets it as the root font-family. Carbon ships with **IBM Plex Sans** and all typography tokens (`heading-01` through `heading-07`, `body-*`, `label-*`) are calibrated for it. Using a non-Carbon font means all Carbon components render in Plex while custom content renders in Titillium -- creating a visual inconsistency.

**Recommendation:** Either adopt IBM Plex Sans globally (Carbon handles font loading), or at minimum acknowledge this is an intentional brand divergence. If keeping Titillium, at least remove hard-coded font-size values (e.g., `3.5rem`, `1.5rem`, `0.9rem`, `0.8rem` throughout CSS) and use Carbon type tokens instead for consistent scale.

---

### 4. No Carbon `Content` wrapper for UI Shell offset

**Impact: MEDIUM** | File: `index.css:23-32`

The app manually offsets content for the Header and SideNav:
```css
#root { padding-top: var(--header-height); }
@media (min-width: 66rem) { #root { padding-left: var(--sidebar-width); } }
```

Carbon's `Content` component handles this automatically when used inside a `HeaderContainer`.

---

### 5. Underused Carbon components

#### 5b. Raw `<form>` instead of Carbon `Form` in ArticlePage

**Impact: LOW** | `ArticlePage.tsx:263`

The comment form uses a raw `<form>` element. Other forms (Login, Register, Settings, Editor) correctly use Carbon's `<Form>` component.

#### 5c. No `InlineLoading` for submit states

**Impact: LOW** | All form pages

When forms submit, buttons are disabled but show no visual loading indicator. Carbon's `InlineLoading` component provides a spinner + status text that can be placed inside or next to buttons.

#### 5d. No `ClickableTile` for article previews

**Impact: LOW** | `ArticlePage.tsx:295`, `HomePage.tsx`

Comment tiles and article previews could use `ClickableTile` instead of plain `Tile` + wrapper `Link`, giving them built-in hover/focus states and better accessibility.

#### 5e. No `Breadcrumb` navigation

**Impact: LOW** | Article and Profile pages

Deep pages like `/article/:slug` and `/profile/:username` lack breadcrumb trails. Carbon's `Breadcrumb` component would improve navigation context.

#### 5f. Tag styling override instead of using Carbon `Tag` props

**Impact: LOW** | `TagList.css:7-15`

Custom `.tag-pill` class overrides Carbon Tag styling with hard-coded background/border-radius. Carbon Tag supports `type` variants (`red`, `blue`, `cyan`, `gray`, `outline`, etc.) and the `size` prop. The custom pill appearance could likely be achieved with an appropriate `type` and removing the CSS override.

---

### 6. Hard-coded spacing values

**Impact: MEDIUM** | Multiple CSS files

`ArticlePage.css` already correctly uses `var(--cds-spacing-*)` tokens. Other files use raw values that map directly to Carbon spacing tokens:

| Raw value | Carbon token | Files |
|-----------|-------------|-------|
| `0.25rem` / `4px` | `--cds-spacing-02` | ArticlePreview.css |
| `0.5rem` / `8px` | `--cds-spacing-03` | Multiple |
| `1rem` / `16px` | `--cds-spacing-05` | Multiple |
| `1.5rem` / `24px` | `--cds-spacing-06` | PageShell.css, AuthPages.css |
| `2rem` / `32px` | `--cds-spacing-07` | HomePage.css, ProfilePage.css |
| `15px` | Not on scale | index.css, PageShell.css (use `--cds-spacing-05` = 16px instead) |

---

## Recommended Changes by Priority

| Priority | Change | Effort | Status |
|----------|--------|--------|--------|
| ~~P1~~ | ~~Replace hard-coded colors with `--cds-*` tokens~~ | ~~Medium~~ | Done |
| ~~P1~~ | ~~Replace hard-coded spacing with `--cds-spacing-*` tokens~~ | ~~Medium~~ | Done |
| ~~P2~~ | ~~Migrate PageShell to Carbon `Grid`/`Column`~~ | ~~High~~ | Done |
| ~~P2~~ | ~~Use Carbon `Content` wrapper instead of manual offset~~ | ~~Low~~ | Done |
| ~~P3~~ | ~~Use `InlineLoading` for form submission states~~ | ~~Low~~ | Done |
| ~~P3~~ | ~~Wrap comment `<form>` in Carbon `Form`~~ | ~~Trivial~~ | Done |
| ~~P3~~ | ~~Remove TagList.css overrides, use Carbon `OperationalTag`~~ | ~~Low~~ | Done |
| **P3** | Add `Breadcrumb` to article/profile pages | Low | |
| ~~P4~~ | ~~Systematize typography: IBM Plex Sans + Carbon type tokens~~ | ~~Medium~~ | Done |
| **P4** | Use `ClickableTile` for interactive tiles | Low | |
