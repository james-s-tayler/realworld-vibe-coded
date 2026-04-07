---
paths:
  - "SPEC-REFERENCE.md"
  - "Docs/**"
---

## Spec Writing — Required Sections

A complete spec covers both API and frontend behavior. Missing either causes eval coverage gaps.

### API Sections (per endpoint)
- Method, path, auth requirements
- Request body with field constraints (required, min/max, format)
- Success response with example JSON
- Every error response with exact trigger condition
- Business rules (idempotency, side effects, ordering, defaults)

### Frontend UI Behaviors (required sections)

**Navigation & Layout** — links per role, role-restricted access behavior

**Route Guards** — every protected route + redirect target. Distinct from API 401.
- Bad: "API returns 401 for unauthenticated users"
- Good: "Unauthenticated user navigating to /editor is redirected to /login by the frontend route guard"

**Error Display** — how API errors appear in the UI. Distinct from error response format.
- Bad: "Returns 400 with error for 'email'"
- Good: "Error message 'already been registered with that email' is displayed in the registration form"

**Mobile Responsive** — document separately from desktop:
- Layout changes at mobile breakpoints
- Hamburger menu open/close/auto-close behavior
- Touch-specific interactions

**Page-Specific Interactions** — frontend-only behaviors:
- Default tab/state on page load
- Pagination UI navigation (clicking pages, not API limit/offset)
- Form interactions (tag chips, auto-submit keys, pre-population on edit)
- Feature flag gated content (both enabled and disabled states)

**Admin/Management Pages** — tables, modals, self-protection rules in UI

**i18n** — how language changes, what elements translate

**Screenshots** — enumerate each page/state individually, including mobile viewports.
- Bad: "Visual regression tests for key pages"
- Good: "Mobile viewport (375x667): Login page, Register page, Settings page"

### Self-Check

Before finalizing a spec, verify:
1. Every route in router config has a spec entry
2. Every form has validation rules AND UI error display documented
3. Every E2E test page is in Frontend UI Behaviors
4. Every error response has a corresponding UI behavior
5. Mobile behavior documented separately from desktop
6. Feature flags documented with both states
7. Screenshot subjects enumerated individually
