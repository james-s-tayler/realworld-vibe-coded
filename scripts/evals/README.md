# Trace-Based Eval System

Evals for the tests. Tests verify code correctness; these evals verify that the tests actually prove what they claim.

## Architecture

```
Layer 1: Tests run against the app (mechanical pass/fail)
Layer 2: Traces graded by model (independent observational judge)

SPEC-REFERENCE.md → generate-expectations.sh → expectations.json
                                                     ↓
Playwright traces → extract-trace.sh → JSON → grade-trace.sh → verdict
                                                     ↓
                                          eval-traces.sh → report
```

## Scripts

| Script | Purpose |
|--------|---------|
| `extract-trace.sh` | Unzips a Playwright trace, extracts actions/network/DOM/console into JSON |
| `generate-expectations.sh` | Uses Claude to produce expectations.json from the spec |
| `grade-trace.sh` | Grades one extracted trace against one expectation → PASS/WEAK/FAIL |
| `eval-traces.sh` | Orchestrates: extract all traces → match to expectations → grade → report |
| `create-test-trace.sh` | Generates synthetic traces for testing the pipeline |

## Usage

### Full pipeline
```bash
# 1. Generate expectations from spec (one-time, or when spec changes)
./scripts/evals/generate-expectations.sh --spec SPEC-REFERENCE.md --output expectations.json

# 2. Run tests with tracing always on (see "Enabling always-on tracing" below)
./build.sh TestE2e --agent

# 3. Evaluate the traces
./scripts/evals/eval-traces.sh Reports/Test/e2e/Artifacts/ expectations.json --output eval-results/
```

### Individual trace grading
```bash
# Extract a single trace
./scripts/evals/extract-trace.sh path/to/trace.zip > extracted.json

# Grade it against an expectation
./scripts/evals/grade-trace.sh extracted.json expectation.json
```

## Verdicts

- **PASS** — Trace demonstrates all key assertions. Actions, network, and UI all match.
- **WEAK** — Feature appears to work but test assertions don't fully verify it. A broken app could also pass this test.
- **FAIL** — Trace does not demonstrate expected behavior. Errors, wrong responses, or missing actions.

## Scoring

```
PASS  = 1.0 points
WEAK  = 0.4 points
FAIL  = 0.0 points
Score = (total points / graded traces) * 100
```

## Enabling always-on tracing

By default, `AppPageTest` only saves traces for failed tests. For evals, you need traces for ALL tests. Apply this change to `Test/e2e/E2eTests/AppPageTest.cs`:

In `StopTracingAsync()`, change the conditional save to always save:
```csharp
// Always save traces for eval grading
await Context.Tracing.StopAsync(new()
{
    Path = Path.Combine(Constants.ReportsTestE2eArtifacts,
        $"{testName}_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip"),
});
```

## Trace format

Playwright traces are ZIP files containing NDJSON:
- `trace.trace` — action events (before/after pairs), DOM snapshots, console, screenshots
- `trace.network` — HTTP request/response events
- `resources/` — binary blobs (screenshots as JPEG, DOM as HTML, response bodies)

## Matching traces to expectations

Trace filenames are fuzzy-matched to expectation names:
- `UserCanSignIn_WithExistingCredentials_trace_20260407.zip` matches `auth-001` (name: `UserCanSignIn_WithExistingCredentials`)
- Unmatched traces get generic grading ("describe what this demonstrates")
