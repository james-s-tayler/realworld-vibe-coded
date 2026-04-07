# Prompt: Generate Full-Stack Application Specification

Use this prompt with a model that has access to the full codebase. The goal is to produce a single specification document that covers BOTH backend API behavior AND frontend UI behavior in enough detail to generate E2E test expectations from it.

---

## The Prompt

You are generating a complete application specification by reading the codebase. This spec will be used to:
1. Generate E2E test expectations (what Playwright tests should verify)
2. Grade test traces (did the test actually demonstrate what it claims?)
3. Audit test coverage (are all specified behaviors covered by tests?)

The spec must describe the application from TWO perspectives:
- **API perspective:** Every endpoint, its inputs, outputs, status codes, validation rules, and business logic
- **User perspective:** Every UI flow, page behavior, navigation pattern, error display, responsive layout, and visual state

Both perspectives are equally important. A spec that only covers APIs will miss frontend-only behaviors (route guards, mobile layout, i18n, feature flags, pagination UI). A spec that only covers UI flows will miss validation edge cases and error formats.

### Step 1: Discover the application structure

Read these locations to understand what exists:

**Backend:**
- Route/endpoint definitions (controllers, endpoint classes, route registrations)
- Request/response DTOs and data models
- Validation rules (FluentValidation, data annotations, manual checks)
- Authentication/authorization configuration
- Database entities and their constraints (field lengths, required fields, unique constraints)
- Business rules (ordering, defaults, idempotency, error handling)
- Middleware (error handling, tenant resolution, feature flags)

**Frontend:**
- Router configuration (all routes, route guards, protected routes, redirects)
- Page components (what each page renders, forms, tables, modals)
- Navigation structure (sidebar, header, mobile hamburger menu)
- Responsive behavior (mobile breakpoints, layout changes, touch interactions)
- Feature flags (which features are gated, what changes when toggled)
- Internationalization (supported languages, what gets translated)
- Error handling (how API errors are displayed to the user, toast/inline/page-level)
- State management (authentication state, how login/logout affects the UI)

**Tests (as specification evidence):**
- E2E tests reveal UI behaviors the developer considered important
- API/integration tests reveal validation edge cases and error formats
- Test fixtures reveal data setup patterns and preconditions

### Step 2: Write the specification

Organize the spec into these sections:

#### Overview
- What the application does (one paragraph)
- Tech stack summary
- Key differences from standard/expected behavior (non-obvious API choices, custom auth flows)

#### Base URL & Conventions
- URL structure, content types, common headers
- Authentication mechanism (token format, header name, how tokens are obtained)

#### Error Response Format
- All error response shapes the API can return
- How validation errors are structured
- Status code semantics (when 400 vs 401 vs 403 vs 404)

#### Endpoints (one section per resource group)
For EACH endpoint, document:
- Method, path, auth requirements
- Request body schema with field constraints (required, min/max length, format)
- Success response with example JSON
- Every error response with the exact condition that triggers it
- Business rules (idempotency, side effects, ordering, defaults)
- Test assertions (what the tests actually check — this reveals edge cases)

#### Frontend UI Behaviors
This section is CRITICAL and most often missing from specs. Document:

**Navigation & Layout:**
- What links/items appear in navigation for different user roles
- What happens when a role-restricted item is accessed by the wrong role

**Route Guards (Frontend Redirects):**
- Every protected route and where it redirects unauthenticated users
- This is DIFFERENT from the API returning 401 — the frontend prevents the request entirely

**Mobile Responsive Behavior:**
- What changes at mobile viewport sizes
- Hamburger menu behavior (open, close, auto-close on navigation)
- Touch-specific interactions

**Page-Specific Behaviors:**
For each page, document behaviors that aren't covered by API docs:
- Default tab/state on page load
- How API errors are rendered (inline messages, toasts, redirect to error page)
- Pagination UI (when controls appear, how navigation works, edge cases with few items)
- Form interactions (tag input chips, auto-submit on certain keys, pre-population on edit)
- Feature flag gated content (what appears/disappears when flags toggle)

**Authentication UI Flows:**
- Registration → login → authenticated state (what changes in the UI)
- Login error display (how failed login is shown — not just the 401, the visible error)
- Sign out behavior (redirect target, UI state change)
- Invite flow (admin invites user, signs out, invited user signs in)

**Admin/Management Pages:**
- Table layouts, columns, pagination
- Modal dialogs (edit roles, confirm actions)
- Self-protection rules rendered in UI (can't deactivate self, can't remove own admin role)

**Internationalization:**
- How language is changed
- What elements are translated (nav, titles, messages, form labels)

**Screenshots / Visual Regression:**
- List every distinct page/state that should have a visual regression test
- Include mobile viewport states separately from desktop

#### Data Models
- All entities with field constraints
- Request/response DTOs
- Default values

#### Business Rules
- Cross-cutting rules (multi-tenancy isolation, ordering, pagination defaults)
- Authorization matrix (who can modify/delete what)

### Step 3: Self-check

After writing the spec, verify:

1. **Every route in the router config has at least one spec entry** (API endpoint or frontend redirect)
2. **Every form has its validation rules documented** (both API-level and UI-level error display)
3. **Every page mentioned in E2E tests is described** in the Frontend UI Behaviors section
4. **Every error response has a corresponding UI behavior** (how does the user see this error?)
5. **Mobile behavior is documented separately** from desktop (responsive is not automatic — interactions differ)
6. **Feature flags are documented** with both enabled and disabled states
7. **Screenshot test subjects are enumerated** individually (not "various pages" — list each one)

### Output format

Write the spec as a single Markdown document. Use tables for endpoint summaries, code blocks for JSON examples, and bullet lists for behavioral rules. The document should be self-contained — a reader (human or model) should be able to understand every testable behavior without accessing the codebase.
