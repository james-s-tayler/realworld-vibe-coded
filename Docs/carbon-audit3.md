# SCSS Audit — Carbon Design Improvements

**14 SCSS files, ~550 lines of custom CSS, ~50+ custom class names.** Token compliance is excellent (all spacing/color/type use Carbon tokens), but there's significant room to reduce custom CSS by leaning on Carbon's React components and consolidating repeated patterns.

## Finding 1: Duplicate `.article-meta` block

Defined identically in both `ArticlePreview.scss:9-13` and `ArticlePage.scss:39-43`:
```scss
.article-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
```
**Fix:** Consolidate into `index.scss` (like the avatar/banner utilities already there).

## Finding 2: Duplicate `.article-tags` block

Defined in `ArticlePreview.scss:78-82` and `ArticlePage.scss:95-100` (slightly different gap values — `$spacing-02` vs `$spacing-03`):
```scss
.article-tags { display: flex; flex-wrap: wrap; gap: ...; }
```
And `.tag-list` in `TagList.scss:4-7` and `.editor-page .tag-list` in `EditorPage.scss:7-12` are the same pattern again.
**Fix:** Unify into one shared `.tag-list` utility with consistent gap.

## Finding 3: Repeated flex layout patterns — replace with `<Stack>`

These patterns appear 8+ times across SCSS and could be replaced with `<Stack>` in TSX:

| Pattern | Files | Carbon replacement |
|---------|-------|--------------------|
| `display:flex; flex-direction:column` | `.author-details`, `.info`, `.edit-roles-checkboxes` | `<Stack gap={N}>` |
| `display:flex; gap:$spacing-03` | `.actions`, `.comment-footer`, `.comment-author`, `.tag-list` | `<Stack gap={3} orientation="horizontal">` |
| `display:flex; justify-content:space-between; align-items:center` | `.article-meta`, `.article-footer`, `.comment-form-footer`, `.users-page-header` | `<Stack>` with CSS `justify-content` or restructure |

Each `<Stack>` replacement eliminates 2-4 lines of custom SCSS.

## Finding 4: Duplicate loading-state patterns

Three different approaches:
- `.article-page.loading` (ArticlePage.scss:20-25) — `min-height: 50vh` (hardcoded)
- `.profile-page.loading` (ProfilePage.scss:21-26) — identical
- `.article-list-loading` (ArticleList.scss:8-12) — similar but padding-based
- `.loading-fullscreen` (index.scss:70-75) — `height: 100vh`

**Fix:** Consolidate into one or two shared loading utilities in `index.scss`.

## Finding 5: Duplicate breadcrumb overflow handling

Identical 7-line block in both `ArticlePage.scss:5-17` and `ProfilePage.scss:5-19`:
```scss
.cds--breadcrumb { margin-bottom: $spacing-05; }
.cds--breadcrumb-item:last-child {
  overflow: hidden;
  .cds--link { overflow: hidden; text-overflow: ellipsis; }
}
```
**Fix:** Move to `index.scss` as a global breadcrumb overflow rule.

## Finding 6: Page padding pattern repeated

Three pages use identical top padding:
```scss
.auth-page    { padding: $spacing-06 0; }
.editor-page  { padding: $spacing-06 0; }
.settings-page { padding: $spacing-06 0; }
```
**Fix:** Single `.page-content-padded` utility or apply padding in `PageShell`.

## Finding 7: `AppHeader.scss` directly overrides Carbon component classes

Lines 3-16 override `.cds--header`, `.cds--header__name`, and `.cds--header__name:hover`. The `frontend.md` rule says: _"Never write direct CSS overrides for Carbon components."_
**Fix:** Use Carbon's `<Theme>` component to scope header theming, or use `HeaderName` props.

## Finding 8: Inconsistent author-info patterns between pages

`ArticlePreview.scss` uses `.author-info`, `.author-image`, `.author-details`, `.author-name` while `ArticlePage.scss` uses `.author-info`, `.info`, `.author`, `.date` — same conceptual pattern, different class names.
**Fix:** Extract a shared author-meta component (React + SCSS) used by both article preview and article page.

## Finding 9: `HomePage.scss` banner uses `var(--cds-shadow)` as a color in `box-shadow`

```scss
box-shadow: inset 0 $spacing-03 $spacing-03 (-$spacing-03) var(--cds-shadow)
```
`--cds-shadow` is a shadow token value, not a color token — semantically incorrect usage.

## Finding 10: Comment styles are page-scoped but could be a component

`ArticlePage.scss:102-170` has ~70 lines for comment styling (`.comment-form`, `.comment-tile`, `.comment-body`, `.comment-footer`, `.comment-author`, `.comment-auth-prompt`). These are all specific to a single page but substantial enough to warrant extraction into a `CommentSection` component with its own `.scss`.

---

## Summary — Improvement opportunities ranked by impact

| # | Category | Impact | Effort |
|---|----------|--------|--------|
| 1 | **Replace flex patterns with `<Stack>`** | High — eliminates ~30 lines of SCSS | Medium |
| 2 | **Extract shared author-meta component** | High — removes duplicate SCSS + TSX patterns | Medium |
| 3 | **Consolidate duplicates to `index.scss`** (.article-meta, .article-tags, breadcrumb overflow, page padding, loading states) | Medium — removes ~40 lines of duplicated SCSS | Low |
| 4 | **Extract CommentSection component** | Medium — better separation of concerns | Medium |
| 5 | **Fix AppHeader Carbon overrides** | Low — use `<Theme>` component | Low |
| 6 | **Fix `--cds-shadow` misuse in banner** | Low — semantic correctness | Low |
