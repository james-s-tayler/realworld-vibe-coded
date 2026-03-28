# Workflow

## R→P→I Workflow (MANDATORY)

This workflow has three phases: Research, Plan, Implement. Follow them in order.

### Phase 1 — Research

Read and analyze before doing anything:

1. Read `SPEC-REFERENCE.md` — understand every endpoint, entity, and validation rule
2. Scan the existing codebase — identify what's already built (run `./build.sh BuildServer` to confirm it compiles)
3. Run `./build.sh RunLocalDependencies` — verify infrastructure works before proceeding
4. Run existing test suites to establish a baseline — note which pass and which fail

**Gate:** Before proceeding to Plan, you must know: (a) what the spec requires, (b) what already exists, (c) what's missing, (d) infrastructure is healthy.

### Phase 2 — Plan

Generate your own execution plan from the spec analysis. The plan must be written to `PROGRESS.md` under "Plan Generated" in this exact format:

1. Identify all features/entities/endpoints that need to be built
2. Map dependencies between features (what must exist before what)
3. Write a **Dependency DAG** (ASCII or text) showing feature build order
4. Break the work into **checkbox stories** — each story is ONE commit (~8 endpoints max):
   ```
   - [ ] Story 1: <name> — gate: `./build.sh <target>`
     Deliverables: <entities, endpoints, migrations>
     Depends on: (none)
   - [ ] Story 2: <name> — gate: `./build.sh <target>`
     Deliverables: <entities, endpoints>
     Depends on: Story 1
   ```
5. Each story targets a specific test suite gate — when those tests pass, the story is done

**Gate:** Before proceeding to Implement, `PROGRESS.md` must contain: (a) a dependency DAG, (b) checkbox stories with deliverables and depends-on fields, (c) each story mapped to a `./build.sh` gate command.

### Phase 3 — Implement (Story Loop) — Each story = one commit

Execute stories from your plan, one at a time:

1. Pick the next incomplete story from your plan
2. Implement the feature (backend first, then frontend if applicable). The Kiota API client regenerates automatically when `BuildClient` runs (chain: `BuildClient → BuildGenerateApiClient → BuildServer`). Always finish and build backend changes before writing frontend code that uses new/changed API types.
3. Run the **full gate** (commit hook enforces these — running them creates gate markers):
   a. `./build.sh BuildServer` — must compile cleanly
   b. Run ALL Postman suites: `TestServerPostmanAuth`, `TestServerPostmanProfiles`, `TestServerPostmanArticlesEmpty`, `TestServerPostmanArticle`, `TestServerPostmanFeedAndArticles`
   c. `./build.sh TestE2e` — note failures (infra vs code)
4. Compare results against your previous run:
   - **Regression** = a suite that previously passed now fails → fix before committing
   - **Expected progress** = this story's target suite now passes → good
   - **Expected failure** = a suite for a later story still fails → fine, move on
5. If no regressions and story target passes: commit with message `feat(story-N): <story-name> — tests passing`
6. If regressions or story target fails: fix and re-run the full gate
7. Update `PROGRESS.md` immediately: mark `- [x]` on the completed story and append a summary under "Completed Stories" with test counts. Do this BEFORE starting the next story.
8. **Do NOT start the next story until you have committed the current one.** Each story must be its own commit. Do not batch multiple stories into one commit.
9. Repeat from step 1

### Stop Condition

You are done when ALL Postman tests AND all E2E tests pass:
```
./build.sh TestServerPostmanAuth TestServerPostmanProfiles TestServerPostmanArticlesEmpty TestServerPostmanArticle TestServerPostmanFeedAndArticles TestE2e
```

## Circuit Breaker

If you are stuck on a single failing test or feature for more than 20 minutes:

1. Commit what you have (even if tests are failing for this feature)
2. Note the issue in `PROGRESS.md` under "Blocked Items"
3. Move to the next story in your plan
4. Return to blocked items only after all other stories are attempted

Breadth of feature coverage is more important than depth on any single feature.

## Context Management

If you notice your context is getting large (many files read, long conversation):

1. Commit all current progress immediately
2. Update `PROGRESS.md` with current state and next steps
3. If context exceeds 60%, compact immediately
4. After compact, re-read `PROGRESS.md` and `Docs/workflow.md` to recover context

Commit-gate semantics ensure progress survives context resets. `PROGRESS.md` ensures knowledge survives.

## Progress Tracking

- Read `PROGRESS.md` at the start of every session to recover context from previous sessions.
- After each story: append what was done, which tests pass, any gotchas discovered.
- **NEVER delete or modify existing entries** — `PROGRESS.md` is append-only.
- This file survives context compression and is the primary cross-session memory.

## Scaffolding Assumptions

The following harness components encode assumptions about model capabilities. As models improve, these may become unnecessary and can be removed:

- **Circuit breaker** — assumes the agent can get stuck in unproductive loops
- **Explicit phase gates** — assumes the agent won't naturally research before planning
- **Mandatory reading order** — assumes the agent won't discover files on its own
- **Full test gate after every story** — assumes the agent might miss regressions

These are designed for simplification — removing any of them is safe to try. Measure the impact via `./scripts/score.sh`.
