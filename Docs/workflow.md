# Workflow

## Implementation Workflow (MANDATORY)

Follow this workflow for every feature. No exceptions.

### Step 0 — Infrastructure Smoke Test

Before starting any stories, verify the test infrastructure works:
```
./build.sh RunLocalDependencies
```
Wait for health checks to pass. If Docker or SQL Server fails, fix infrastructure before proceeding. This prevents discovering infra failures only at the E2E stage.

### Story Loop

1. Read the active exec plan and pick the next incomplete story
2. Implement the feature (backend first, then frontend if applicable)
3. Run the story's test command (e.g., `./build.sh TestServerPostmanAuth`)
4. If tests **PASS**: commit with message `feat: implement <story-name> — tests passing`
5. If tests **FAIL**: fix and re-test. Do NOT move to the next story until this one passes.
6. Append to `PROGRESS.md`: what was implemented, test results, any gotchas discovered
7. Repeat from step 1

### Stop Condition

You are done when ALL Postman tests AND all E2E tests pass:
```
./build.sh TestServerPostmanAuth TestServerPostmanProfiles TestServerPostmanArticlesEmpty TestServerPostmanArticle TestServerPostmanFeedAndArticles TestE2e
```

## Circuit Breaker

If you are stuck on a single failing test or feature for more than 20 minutes:

1. Commit what you have (even if tests are failing for this feature)
2. Note the issue in `PROGRESS.md` under "Blocked Items"
3. Move to the next story in the active exec plan
4. Return to blocked items only after all other stories are attempted

Breadth of feature coverage is more important than depth on any single feature.

## Context Management

If you notice your context is getting large (many files read, long conversation):

1. Commit all current progress immediately
2. Update `PROGRESS.md` with current state and next steps
3. If using compact, ensure `PROGRESS.md` and the active exec plan are referenced in the summary
4. After compact, re-read `PROGRESS.md` and the active exec plan to recover context

Commit-gate semantics ensure progress survives context resets. `PROGRESS.md` ensures knowledge survives.

## Progress Tracking

- Read `PROGRESS.md` at the start of every session to recover context from previous sessions.
- After each story: append what was done, which tests pass, any gotchas discovered.
- **NEVER delete or modify existing entries** — `PROGRESS.md` is append-only.
- This file survives context compression and is the primary cross-session memory.
