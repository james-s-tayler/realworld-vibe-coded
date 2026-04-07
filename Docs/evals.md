# Trace-Based Eval System

## Problem

Tests verify that code behaves correctly. But what verifies that the tests themselves are good?

A test can pass mechanically while proving almost nothing — checking that a page loaded without verifying its content, asserting an element is visible without confirming it's the right element, or covering a happy path without exercising the actual business logic. When tests are generated (by a model or by a human in a hurry), this problem gets worse: the generator has an incentive to produce tests that pass, not tests that catch bugs.

The trace eval system adds an independent observational layer: a model watches what actually happened during each test (via Playwright traces) and judges whether the test genuinely demonstrated what it claims.

## Architecture

```
                             SPEC-REFERENCE.md
                                    │
                            ┌───────▼────────┐
                            │  generate-      │
                            │  expectations   │  (Model A — one-time)
                            └───────┬────────┘
                                    │
                             expectations.json
                              ╱            ╲
                  ┌──────────╱──────────┐   ╲
                  │     Layer 2:        │    ╲
                  │   Trace Grading     │     ╲
                  │                     │      ╲
               trace₁   trace₂   trace₃   ┌───▼──────────────┐
                 │         │        │      │    Layer 3:       │
              extract   extract  extract   │ Coverage Check    │
                 │         │        │      │                   │
               grade     grade    grade    │ master-list.json  │
              (Model B)           │        │ (from existing    │
                 │         │        │      │  test suite)      │
               PASS      WEAK    FAIL     │        ↕           │
                 │         │        │      │ expectations.json │
                 └─────────┼────────┘      │  ⊇ master-list?  │
                           │               └───────┬──────────┘
                   eval-report.json                │
                                          coverage-report.json
```

There are three layers of evaluation, each independent:

**Layer 1 — Mechanical test execution.** Playwright tests run against the app. Each test either passes or fails based on its coded assertions. This is the traditional test layer.

**Layer 2 — Trace grading.** An independent model (the "judge") examines the Playwright trace of each test run and decides whether the trace evidence actually demonstrates what the test claims. The judge sees actions, network requests, DOM snapshots, console messages, and screenshots — raw observational data, not assertion logic.

**Layer 3 — Coverage completeness.** The existing hand-written test suite is reverse-engineered into a master list of known behaviors that should be tested. The generated expectations (from the spec) are then checked against this master list. If the generator missed something the hand-written tests cover, it's a gap. This ensures the spec-to-expectations generator doesn't silently drop coverage.

The key property: **the judge cannot be fooled by weak assertions.** A test that checks `toBeVisible` on a generic element gets graded WEAK even if it passed mechanically, because the judge sees that the DOM was never inspected for the specific content the spec requires.

## The Recursion Problem

> We need tests to test the code. But we also need evals to test the tests.

This sounds like an infinite regress, but the eval layer breaks it because it's grounded differently:

| Layer | Grounded in | Can be wrong because |
|-------|------------|---------------------|
| Code | Implementation logic | Bugs |
| Tests | Assertion logic | Weak/wrong assertions |
| Trace evals (L2) | Raw observation (DOM, network, screenshots) | Model misreads evidence |
| Coverage evals (L3) | Existing hand-written tests | Master list extraction misses a test, or semantic matching is wrong |

The eval layer's failure mode (model misreads a screenshot or misunderstands the spec) is independent of the test layer's failure mode (wrong assertion logic). This means the two layers catch different classes of bugs. A test with correct assertions but broken code fails at Layer 1. A test with weak assertions but working code passes Layer 1 but fails Layer 2.

## Components

### expectations.json

Generated from `SPEC-REFERENCE.md` by `generate-expectations.sh`. Each expectation describes one testable behavior:

```json
{
  "id": "auth-001",
  "name": "UserCanSignIn_WithExistingCredentials",
  "category": "happy_path",
  "spec_section": "Authentication > Login Flow",
  "feature_area": "auth",
  "expected_behavior": "User navigates to /login, fills email and password, clicks Sign in. App authenticates and redirects to home page showing the user is logged in.",
  "key_assertions": [
    "POST /api/identity/login returns 200 with accessToken",
    "Settings link becomes visible in sidebar (indicates authenticated state)",
    "User's email/username is displayed in the navigation"
  ],
  "ui_indicators": [
    "Login form has Email and Password fields",
    "After login, sidebar shows Settings link",
    "User profile link shows the email address"
  ],
  "network_indicators": [
    "POST /api/identity/login?useCookies=false -> 200",
    "GET /api/user -> 200"
  ]
}
```

The expectation is the "ground truth" — what a correct test for this behavior must demonstrate. It's derived from the spec, not from the tests themselves.

### Trace Extraction

Playwright traces are ZIP files containing NDJSON (newline-delimited JSON):

| File in ZIP | Contents |
|-------------|----------|
| `trace.trace` | Action events (goto, click, fill, expect), DOM snapshots, console messages, screenshots |
| `trace.network` | HTTP request/response events with status codes and headers |
| `resources/` | Binary blobs — screenshot JPEGs, DOM snapshot HTML, response bodies (keyed by SHA1) |

`extract-trace.sh` parses these into a single JSON structure:

```json
{
  "actions": [
    { "method": "page.goto", "params": { "url": "/login" }, "passed": true },
    { "method": "locator.fill", "params": { "selector": "getByPlaceholder('Email')", "value": "user@test.com" }, "passed": true },
    { "method": "expect(locator).toBeVisible", "params": { "selector": "getByRole('link', { name: 'Settings' })" }, "passed": true }
  ],
  "network": [
    { "method": "POST", "url": "/api/identity/login?useCookies=false", "status": 200 },
    { "method": "GET", "url": "/api/user", "status": 200 }
  ],
  "dom_snapshots": [
    { "visible_text": "Settings Users testuser@test.com Dashboard Welcome" }
  ],
  "console_messages": [],
  "summary": { "total_actions": 6, "failed_actions": 0 }
}
```

### Grading

`grade-trace.sh` sends the extracted trace + expectation to a model with a grading prompt. The model acts as an independent judge, examining the evidence and returning a structured verdict:

```json
{
  "verdict": "WEAK",
  "confidence": 0.82,
  "demonstrated": ["POST /api/identity/login returns 200"],
  "not_demonstrated": [
    "Settings link visible in sidebar",
    "User's email displayed in navigation"
  ],
  "concerns": [
    "Only asserts URL redirect, never verifies authenticated UI state",
    "No DOM snapshots captured to verify UI indicators"
  ],
  "evidence": {
    "actions_match": true,
    "network_match": true,
    "ui_match": false,
    "no_errors": true
  },
  "reasoning": "The trace shows correct login flow and successful API calls, but the test only asserts a URL redirect and never verifies the two most important indicators..."
}
```

### Verdicts

| Verdict | Meaning | Score |
|---------|---------|-------|
| **PASS** | Trace demonstrates ALL key assertions. Actions, network, and UI evidence all confirm the feature works and the test verifies it. | 1.0 |
| **WEAK** | Feature appears to work (network succeeds, no errors) but the test doesn't fully verify it. A broken app could also pass this test. | 0.4 |
| **FAIL** | Trace does not demonstrate expected behavior. Errors, wrong status codes, timeouts, or missing actions. | 0.0 |

The WEAK verdict is the most valuable signal — it catches tests that provide false confidence.

### Composite Score

```
score = (PASS_count * 1.0 + WEAK_count * 0.4) / graded_count * 100
```

The `eval-report.json` also tracks:
- Per-verdict counts and pass rate
- Feature area coverage (how many spec sections have at least one PASS)
- A concerns list aggregating all WEAK/FAIL issues for targeted improvement

## Layer 3: Coverage Completeness

Layer 2 answers "are the tests good?" Layer 3 answers "are there enough tests?"

The existing hand-written E2E test suite represents known-good coverage — behaviors that a human decided were important enough to test. When generating expectations from the spec, the generator might miss some of these. Layer 3 catches that.

### Master list extraction

Every `[Fact]` test method is annotated with a `[TestCoverage]` attribute directly in the source code:

```csharp
[Fact]
[TestCoverage(
    Id = "auth-happy-001",
    FeatureArea = "auth",
    Behavior = "User can log in with valid credentials and see authenticated UI state",
    Verifies = ["Settings link visible in sidebar", "User profile link shows email"]
)]
public async Task UserCanSignIn_WithExistingCredentials()
```

The `TestEvalsMasterList` Nuke target parses these attributes from the source files (regex over `.cs` files — no assembly loading, no LLM call) and produces `master-list.json`:

```json
{
  "test_count": 73,
  "by_feature": { "auth": 8, "articles": 7, "editor": 10, "feed": 11, ... },
  "tests": [
    {
      "id": "auth-happy-001",
      "test_method": "UserCanSignIn_WithExistingCredentials",
      "feature_area": "auth",
      "category": "happy_path",
      "behavior": "User can log in with valid credentials and see authenticated UI state",
      "verifies": ["Settings link visible in sidebar", "User profile link shows email"]
    }
  ]
}
```

This is deterministic, fast (<1 second), and requires no LLM call.

A Roslyn analyzer (E2E008) enforces that every `[Fact]` method has a `[TestCoverage]` attribute — you can't add a test without documenting what it proves. The master list is a checked-in artifact, regenerated whenever tests change.

### Coverage comparison

`eval-coverage.sh` takes both files and uses a model to semantically match each master list entry against the generated expectations:

```json
{
  "master_id": "users-happy-005",
  "master_method": "DeactivatedUserCannotLogin",
  "match_status": "missing",
  "match_reasoning": "No expectation covers the specific behavior of deactivated users being unable to log in. The auth expectations cover login success/failure but not account lockout."
}
```

Match statuses:
- **covered** — an expectation exists that tests the same behavior with equivalent assertions
- **weak** — an expectation exists in the same area but doesn't fully cover the specific behavior
- **missing** — no expectation covers this behavior at all

The coverage score: `(covered * 1.0 + weak * 0.5) / total * 100`

`TestEvalsCoverage` **fails** if any master list entries are missing — this is a hard gate, not a soft score. If the generator can't produce expectations that cover what the hand-written tests cover, the generator (or the spec) needs to be improved.

## Usage

### Via Nuke targets (recommended)

```bash
# Generate expectations from spec (once, or when spec changes)
./build.sh TestEvalsGenerate --agent

# Extract master list from existing test suite (once, or when tests change)
./build.sh TestEvalsMasterList --agent

# Check that generated expectations cover all known tests (Layer 3)
./build.sh TestEvalsCoverage --agent

# Run E2E with tracing + grade traces (Layer 2)
./build.sh TestEvals --agent
```

| Target | What it does | When to run |
|--------|-------------|-------------|
| `TestEvalsGenerate` | Reads `SPEC-REFERENCE.md`, calls Claude to produce `expectations.json` | Once, or when spec changes |
| `TestEvalsMasterList` | Reads existing test code, calls Claude to produce `master-list.json` | Once, or when E2E tests change |
| `TestEvalsCoverage` | Asserts `expectations.json ⊇ master-list.json` — fails if gaps exist | After generating both files |
| `TestEvals` | Runs E2E with tracing, extracts traces, grades each against expectations | Full eval run |

Output goes to `Reports/Test/Evals/`:
- `expectations.json` — generated from spec (Layer 2 + 3 input)
- `master-list.json` — extracted from existing tests (Layer 3 input)
- `coverage-report.json` — Layer 3 results (gaps, weak coverage)
- `Results/eval-report.json` — Layer 2 results (trace verdicts + composite score)
- `Results/extracted/` — per-trace extracted JSON
- `Results/verdicts/` — per-trace grading verdicts

### Via shell scripts (for debugging or custom pipelines)

```bash
# Generate expectations
./scripts/evals/generate-expectations.sh \
  --spec SPEC-REFERENCE.md \
  --output Reports/Test/Evals/expectations.json

# Extract a single trace
./scripts/evals/extract-trace.sh path/to/trace.zip > extracted.json

# Grade a single trace
./scripts/evals/grade-trace.sh extracted.json expectation.json

# Run full pipeline on a directory of traces
./scripts/evals/eval-traces.sh \
  Reports/Test/e2e/Artifacts/ \
  Reports/Test/Evals/expectations.json \
  --output Reports/Test/Evals/Results/
```

## Controlling Tracing

Tracing is controlled by the `PLAYWRIGHT_ALWAYS_TRACE` environment variable:

| Value | Behavior | When to use |
|-------|----------|-------------|
| `false` (default) | Traces saved only for **failed** tests | Normal `TestE2e` runs |
| `true` | Traces saved for **all** tests | `TestEvals` sets this automatically |

This flows through the stack as:
1. Nuke target sets `PLAYWRIGHT_ALWAYS_TRACE=true` in the process environment
2. `docker-compose.yml` passes it to the playwright container via `${PLAYWRIGHT_ALWAYS_TRACE:-false}`
3. `AppPageTest.StopTracingAsync()` checks the env var and saves traces accordingly

Running `TestE2e` directly is unaffected — tracing stays failure-only.

## Integration with the Autoresearch Harness

In `score.sh`, the eval pipeline slots in after test execution:

```
1. Agent builds the app from spec                    (existing)
2. Run all test suites                               (existing)
3. Generate expectations from spec                   (new — Layer 2)
4. Run E2E tests again with always-on tracing        (new — Layer 2)
5. eval-traces.sh grades each trace                  (new — Layer 2)
6. Composite score combines test results + eval      (modified)
```

The modified scoring formula:

```
if all tests pass:
  base = 50
  eval_bonus = floor(eval_score / 100 * 15)    # 0-15 for trace quality
  alignment  = alignment_audit (0-15)           # reduced from 30
  time_bonus = existing formula (0-20)
  score = base + eval_bonus + alignment + time_bonus
else:
  score = floor(tests_passing / total_tests * 45)
```

This creates pressure not just to pass tests, but to pass them with strong assertions that the eval layer grades as PASS rather than WEAK.

## Calibration

Before trusting the eval system on generated tests, calibrate it against known-good and known-bad traces:

```bash
# Generate synthetic traces for all three scenarios
./scripts/evals/create-test-trace.sh /tmp/calibration/ --scenario good
./scripts/evals/create-test-trace.sh /tmp/calibration/ --scenario weak
./scripts/evals/create-test-trace.sh /tmp/calibration/ --scenario bad

# Grade each — verify: good→PASS, weak→WEAK, bad→FAIL
./scripts/evals/eval-traces.sh /tmp/calibration/ expectations.json
```

Calibration results from initial testing:

| Scenario | Expected | Actual | Confidence |
|----------|----------|--------|------------|
| Good (strong assertions) | PASS | PASS | 0.92 |
| Weak (URL-only assertion) | WEAK | WEAK | 0.82 |
| Bad (wrong password, 401) | FAIL | FAIL | 0.99 |
