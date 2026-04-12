# Carbon Design System Audit — Frontend CSS & Layout

**Date:** 2026-04-13
**Scope:** All 14 SCSS files and 22 TSX components in `App/Client/src/`
**Baseline:** Carbon v11 — @carbon/react v1.92.1, @carbon/styles v1.91.0

## What's Done Well

The codebase is already well-integrated with Carbon:

- All colors use `var(--cds-*)` CSS custom properties
- All spacing uses `$spacing-*` tokens
- Typography consistently uses `@include type.type-style()` mixins
- Pages use the `PageShell` abstraction with Carbon Grid/Column
- Good Carbon component usage (Button, DataTable, Modal, Tabs, Pagination, etc.)
- `<Theme>` component used correctly for SideNav dark zone
- No inline styles in any TSX file

---

## Findings

### ~~1. Hardcoded Header Height — `calc(100vh - 56px)`~~ FIXED

**Severity:** Medium
**Files:** `AuthPages.scss:4`, `SettingsPage.scss:4`, `EditorPage.scss:4`

All three use `min-height: calc(100vh - 56px)` where `56px` is the Carbon UI Shell header height. Meanwhile `HomePage.scss`, `ArticlePage.scss`, and `ProfilePage.scss` use `min-height: 100vh` (inconsistent approach).

**Recommendation:** The header offset is already handled by `.cds--content` in `index.scss`. These pages render inside `<Content>` via the app layout, so the `calc(100vh - 56px)` is compensating for something `.cds--content` should handle. Either use a consistent approach across all pages or reference Carbon's shell height token: `shell.mini-units(6)` from `@carbon/styles/scss/components/ui-shell/functions`.

**Resolution:** Centralized full-height layout in `.cds--content` using `min-height: calc(100vh - shell.mini-units(6))`. Removed `min-height` from all 6 page SCSS files and `body`/`#root`. Also fixes finding #10.

---

### ~~2. Custom Container Instead of Carbon Grid~~ FIXED

**Severity:** Medium
**File:** `ArticlePage.scss:110-118`

```scss
.article-grid-container {
  max-width: 1140px;
  margin-left: auto;
  margin-right: auto;
  padding: 0 $spacing-05;
}

.article-column-offset {
  margin-left: auto;
  margin-right: auto;
}
```

This is a hand-rolled centered container. The `1140px` max-width is arbitrary and doesn't align with any Carbon breakpoint (`max` = 1584px, `xlg` = 1312px, `lg` = 1056px).

**Recommendation:** Use Carbon Grid with a `Column` span/offset to constrain content width, which is what `PageShell` already does for other pages. The article content section should flow through PageShell's Grid system rather than bypassing it.

**Resolution:** Removed unused `.article-grid-container` and `.article-column-offset` selectors (dead CSS — never referenced in TSX). Also removed `.settings-notification` and `.users-modal-notification`.

---

### ~~3. Line-Height Override Breaking Type Token~~ FIXED

**Severity:** Low
**File:** `ArticlePage.scss:89`

```scss
.article-body {
  @include type.type-style('body-02');
  line-height: 1.8;  /* overrides the token's 1.5 */
}
```

Applies the `body-02` type style then immediately overrides its line-height. This breaks out of the type system. The `/1\\.8/` regex was added to `carbon/type-use` `acceptValues` in `.stylelintrc.json:57` specifically to whitelist this violation.

**Recommendation:** Either accept the token's value (1.5) or document why the override is needed. Remove the `/1\\.8/` exception from stylelint config if the override is removed.

**Resolution:** Removed the `line-height: 1.8` override, accepting the `body-02` token value (1.5). Removed the `/1\\.8/` exception from `.stylelintrc.json`.

---

### ~~4. Custom Button Styling~~ FIXED

**Severity:** Medium
**File:** `ArticlePreview.scss:53-59`

```scss
.favorite-button {
  color: var(--cds-link-primary);
  border: 1px solid var(--cds-link-primary);
}

.favorite-button.favorited {
  background-color: var(--cds-link-primary);
  color: var(--cds-text-on-color);
}
```

This overrides a Carbon `<Button>` with custom border/background to create a toggle appearance. The `.favorite-button` class is applied via `className` on a `<Button kind="ghost">` in `ArticlePreview.tsx:58`.

**Recommendation:** Use Carbon Button's `kind` prop to distinguish states — `kind="ghost"` for unfavorited, `kind="primary"` for favorited — toggling in the JSX rather than overriding CSS. Or use a `Toggle`/`IconButton` pattern.

**Resolution:** Replaced custom CSS with Carbon-native `kind` prop toggling: `kind="tertiary"` (bordered) for unfavorited, `kind="primary"` (filled) for favorited. Deleted `.favorite-button` CSS rules.

---

### ~~5. Hardcoded Image Dimensions~~ FIXED

**Severity:** Medium
**Files:** Multiple

| File | Class | Size | Carbon Equivalent |
|------|-------|------|-------------------|
| `ArticlePreview.scss:23-24` | `.author-image` | `32px` | `$spacing-07` (2rem = 32px) |
| `ArticlePage.scss:53-54` | `.article-meta img` | `32px` | `$spacing-07` |
| `ArticlePage.scss:168-169` | `.comment-author-img` | `24px` | `$spacing-06` (1.5rem = 24px) |
| `ProfilePage.scss:30-31` | `.user-img` | `100px` | No exact token — use `6.25rem` |

All avatar images use hardcoded pixel widths/heights.

**Recommendation:** Replace with spacing tokens: `width: $spacing-07; height: $spacing-07;` for 32px avatars, `width: $spacing-06; height: $spacing-06;` for 24px avatars. For the 100px profile image, there's no exact spacing token, but use rem units at minimum.

**Resolution:** Replaced all hardcoded pixel dimensions with Carbon spacing tokens: `$spacing-07` for 32px avatars, `$spacing-06` for 24px avatars, `$spacing-12` (96px) for the 100px profile avatar (nearest token, imperceptible difference). Extended `carbon/layout-use` stylelint rule to include `width`, `height`, `min-height`, `max-height`, `min-width`, `max-width` to prevent future regressions.

---

### ~~6. Hardcoded `z-index: 9000`~~ FIXED

**Severity:** Low
**File:** `ToastContainer.scss:7`

```scss
.toast-container {
  z-index: 9000;
}
```

Carbon has its own z-index layers. The toast should sit above Carbon components but this magic number could conflict.

**Recommendation:** Use Carbon's z-index utilities: `@use '@carbon/react/scss/utilities' as *;` and then reference a documented z-index level, or at minimum extract this to a named variable.

**Resolution:** Replaced `z-index: 9000` with `z-index: z.z('modal')` using Carbon's z-index utility function from `@carbon/styles/scss/utilities/z-index`. Also removed redundant `max-width: 25rem` — Carbon's `ToastNotification` already controls its own width (`inline-size: 288px`).

---

### ~~7. Hardcoded Shadow Dimensions~~ FIXED

**Severity:** Low
**File:** `HomePage.scss:10-11, 15`

```scss
.banner {
  box-shadow: inset 0 8px 8px -8px var(--cds-shadow),
              inset 0 -8px 8px -8px var(--cds-shadow);
}

.banner-title {
  text-shadow: 0 1px 3px var(--cds-shadow);
}
```

The shadow color correctly uses `var(--cds-shadow)`, but the dimensions (`8px`, `1px 3px`) are hardcoded. The `8px` values could use `$spacing-03` to stay on the spacing grid.

**Resolution:** Replaced `8px` with `$spacing-03` in the banner `box-shadow`. Left `text-shadow: 0 1px 3px` as-is — sub-grid decorative values with no meaningful Carbon token mapping.

---

### 8. `window.confirm()` Instead of Carbon Modal

**Severity:** High
**File:** `ArticlePage.tsx:176`

```tsx
if (!article || !window.confirm('Are you sure you want to delete this article?')) return;
```

Two issues: (a) uses native browser confirm dialog instead of Carbon Modal, and (b) the string is hardcoded in English, not going through i18n.

**Recommendation:** Use a Carbon `Modal` with `danger` prop for delete confirmation, and use `t('article.confirmDelete')` for the message text.

---

### 9. Flex Layouts That Could Use Carbon `<Stack>`

**Severity:** Low
**Files:** Multiple

Many components use manual flexbox column layouts with gaps that are exactly what Carbon's `<Stack>` component provides:

| File | Class | Pattern |
|------|-------|---------|
| `ArticlePreview.scss:31-32` | `.author-details` | `flex-direction: column` |
| `ArticlePage.scss:64-65` | `.article-meta .info` | `flex-direction: column` |
| `UsersPage.scss:15-17` | `.edit-roles-checkboxes` | `flex-direction: column; gap: $spacing-03` |
| `EditorPage.scss:9-11` | `.editor-page .tag-list` | `flex-wrap: wrap; gap: $spacing-03` |
| `ToastContainer.scss:8-10` | `.toast-container` | `flex-direction: column; gap: $spacing-03` |

**Recommendation:** Replace with `<Stack gap={N}>` in JSX where the layout is a simple vertical stack. Not all flex layouts should be Stack (e.g., `justify-content: space-between` patterns are better as flex), but pure column stacks with gaps are a good fit.

---

### ~~10. Inconsistent Full-Height Patterns~~ FIXED

**Severity:** Low
**Files:** 6 page SCSS files

Three different approaches to full-height pages:
- `min-height: 100vh` — HomePage, ArticlePage, ProfilePage
- `min-height: calc(100vh - 56px)` — AuthPages, SettingsPage, EditorPage
- `min-height: 50vh` — Loading states in ArticlePage, ProfilePage

**Recommendation:** Standardize on one approach. Since all pages render inside `.cds--content` (which handles the header offset), the content area height should be consistent. Consider adding a single utility class in `index.scss` and applying it from `PageShell`.

**Resolution:** Fixed as part of finding #1 — centralized in `.cds--content` with `min-height: calc(100vh - shell.mini-units(6))`.

---

### ~~11. `<hr>` With Custom/Missing Styling~~ FIXED

**Severity:** Low
**Files:** `SettingsPage.scss:8-11`, `ArticlePage.tsx:269`

```scss
.settings-page hr {
  margin: $spacing-07 0;
  border: 0;
  border-top: 1px solid var(--cds-border-subtle);
}
```

And `ArticlePage.tsx:269` has a bare `<hr />` with no class — it inherits browser defaults.

**Recommendation:** Extract a shared `.divider` class or add a global `hr` reset in `index.scss` that uses Carbon border tokens consistently.

**Resolution:** Added a global `hr` reset in `index.scss` using `$spacing-07` margin and `var(--cds-border-subtle)` border. Removed the dead `.settings-page hr` rule from `SettingsPage.scss` (SettingsPage.tsx has no `<hr>` elements).

---

### ~~12. Repeated Avatar `border-radius: 50%`~~ FIXED

**Severity:** Low
**Files:** `ArticlePreview.scss`, `ArticlePage.scss`, `ProfilePage.scss`

```scss
.author-image { border-radius: 50%; }     /* ArticlePreview.scss */
.article-meta img { border-radius: 50%; } /* ArticlePage.scss */
.comment-author-img { border-radius: 50%; } /* ArticlePage.scss */
.user-img { border-radius: 50%; }         /* ProfilePage.scss */
```

Four separate declarations of the same circular avatar pattern.

**Recommendation:** Extract a shared `.avatar` SCSS class (or mixin) that handles the `border-radius: 50%` + sizing, and apply it in one place. Could have `.avatar--sm` (24px), `.avatar--md` (32px), `.avatar--lg` (100px) variants using spacing tokens.

**Resolution:** Added three global avatar utility classes in `index.scss`: `.avatar-sm` (`$spacing-06`), `.avatar-md` (`$spacing-07`), `.avatar-lg` (`$spacing-12`), each with `border-radius: 50%`. Stripped sizing/rounding from the four component-level selectors, keeping only contextual margins. Replaced `.article-meta img` element selector with class-based `.article-meta .avatar-md`. Deleted `.comment-author-img` block entirely.

---

### ~~13. Banner Pattern Duplicated~~ FIXED

**Severity:** Low
**Files:** `HomePage.scss`, `ArticlePage.scss`, `ProfilePage.scss`

Three pages have their own banner implementations with duplicated inner styles (padding `$spacing-07 0`, Grid wrapping, full-width background). The `PageShell` banner prop handles placement, but the banner content styles are repeated.

**Recommendation:** Extract a shared `.page-banner` base class for the common padding/layout pattern.

**Resolution:** Added `.page-banner` base class in `index.scss` with the shared `padding: $spacing-07 0`. Removed padding from the three page-specific banner selectors. Composed the base class on each banner `<div>` in TSX (`className="page-banner banner"`, `className="page-banner user-info"`).

---

## Guardrail Gap Analysis

### Why Stylelint Doesn't Catch These

The existing stylelint rules (`carbon/theme-use`, `carbon/layout-use`, `carbon/type-use`, `carbon/motion-duration-use`, `carbon/motion-easing-use`) are correctly configured and the code is clean for everything they check. The findings fall into gaps:

| Gap | Reason | Findings Affected |
|-----|--------|-------------------|
| `width`, `height`, `min-height`, `max-width` not checked by `carbon/layout-use` | Not in the rule's default `includeProps` | #1, #2, #5 |
| `z-index` not checked by any rule | No Carbon rule covers z-index | #6 |
| `line-height: 1.8` explicitly whitelisted | `/1\\.8/` added to `acceptValues` in `.stylelintrc.json:57` | #3 |
| `border-radius` not checked by any rule | Not a color, spacing, type, or motion property | #12 |
| Structural/architectural patterns beyond CSS linting | Stylelint checks values, not component usage patterns | #2, #4, #8, #9, #10, #13 |
| `display`, `flex-direction`, `justify-content`, `align-items` not checked | Carbon doesn't prescribe tokens for layout mechanics | #9 |

### Recommendations to Tighten Guardrails

#### 1. Extend `carbon/layout-use` to include size properties

Add `width`, `height`, `min-height`, `max-width`, `min-width` to the rule's `includeProps`. This would flag hardcoded dimensions and force use of spacing tokens or explicit `acceptValues` exceptions.

```json
"carbon/layout-use": [true, {
  "includeProps": [
    "/^width$/", "/^height$/", "/^min-height$/", "/^max-width$/", "/^min-width$/"
  ],
  "acceptValues": ["/^0$/", "auto", "/mini-units/", "/^100(vh|%)$/", "/^50vh$/"],
  "severity": "error"
}]
```

Note: `includeProps` extends (not replaces) the defaults. The `acceptValues` would need viewport units and percentages whitelisted since those are legitimate.

#### 2. Remove the `/1\\.8/` exception from `carbon/type-use`

Either fix the `line-height: 1.8` override in `ArticlePage.scss` to use the token value, or if the override is intentional, replace the regex with a stylelint inline disable comment (`/* stylelint-disable-next-line */`) with a justification — making the exception visible at the point of use rather than hidden in the config.

#### 3. Add an ESLint rule to flag `window.confirm()` / `window.alert()`

A custom ESLint rule (or `no-restricted-globals` config) can flag usage of native browser dialogs in TSX files, suggesting Carbon Modal instead:

```json
"no-restricted-globals": ["error", {
  "name": "confirm",
  "message": "Use Carbon Modal with danger prop instead of window.confirm()"
}]
```

#### 4. Add a custom stylelint rule for `z-index`

Either use `stylelint-declaration-strict-value` for `z-index` to require variables, or add a custom rule. Alternatively, add `z-index` to a project-level convention and enforce via code review.

---

## Summary

| # | Severity | Finding | Files |
|---|----------|---------|-------|
| ~~1~~ | ~~Medium~~ | ~~Hardcoded `56px` header height~~ | ~~AuthPages, Settings, Editor~~ |
| ~~2~~ | ~~Medium~~ | ~~Custom container bypassing Grid~~ | ~~ArticlePage~~ |
| ~~3~~ | ~~Low~~ | ~~Line-height overriding type token~~ | ~~ArticlePage~~ |
| ~~4~~ | ~~Medium~~ | ~~Custom button CSS overrides~~ | ~~ArticlePreview~~ |
| ~~5~~ | ~~Medium~~ | ~~Hardcoded px image dimensions~~ | ~~ArticlePreview, ArticlePage, ProfilePage~~ |
| ~~6~~ | ~~Low~~ | ~~Magic `z-index: 9000`~~ | ~~ToastContainer~~ |
| ~~7~~ | ~~Low~~ | ~~Hardcoded shadow dimensions~~ | ~~HomePage~~ |
| 8 | High | `window.confirm()` + missing i18n | ArticlePage |
| 9 | Low | Manual flex stacks could use `<Stack>` | Multiple (5 files) |
| ~~10~~ | ~~Low~~ | ~~Inconsistent full-height approach~~ | ~~6 page files~~ |
| ~~11~~ | ~~Low~~ | ~~Unstyled `<hr>` / duplicate styling~~ | ~~ArticlePage, SettingsPage~~ |
| ~~12~~ | ~~Low~~ | ~~Repeated avatar `border-radius: 50%`~~ | ~~4 files~~ |
| ~~13~~ | ~~Low~~ | ~~Banner padding pattern duplicated~~ | ~~3 page files~~ |

**High:** 1 finding (window.confirm)
**Medium:** 0 findings remaining
**Low:** 1 finding remaining (#9 — manual flex stacks could use `<Stack>`)
