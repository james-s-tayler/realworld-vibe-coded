---
description: Run before finishing to self-review, run the full test suite, and report a score. Triggered by the Stop hook.
---

This is mandatory before finishing. Three steps: self-review, full test suite, score report.

## Step 1: Self-review

Run `/self-review` to check your changes for agent-specific failure modes (over-abstraction, missing fields, unnecessary error handling, hardcoded values, stale TODOs). Fix any issues found before proceeding.

## Step 2: Run all Postman test collections

Run each collection individually and record results:

```bash
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanArticle
./build.sh TestServerPostmanFeedAndArticles
```

For each collection, note: X passing, Y failing out of Z total.

## Step 3: Run E2E test suite

```bash
./build.sh TestE2e
```

Note: X passing, Y failing out of Z total.

## Step 4: Calculate and report score

Calculate the overall score:

```
Score = (total passing across all suites) / (total tests across all suites)
```

Report the score to the user in this format:

```
## Final Score

| Suite | Passing | Failing | Total |
|-------|---------|---------|-------|
| PostmanAuth | X | Y | Z |
| PostmanProfiles | X | Y | Z |
| PostmanArticlesEmpty | X | Y | Z |
| PostmanArticle | X | Y | Z |
| PostmanFeedAndArticles | X | Y | Z |
| E2E | X | Y | Z |
| **Total** | **X** | **Y** | **Z** |

**Score: X/Z (N%)**
```

## Step 5: Agent Performance Summary

If `Reports/agent-stats.csv` exists, summarize it:

1. Count total invocations per target
2. Count pass/fail per target
3. Calculate pass rate per target
4. Report in this format:

```
## Agent Performance

| Target | Runs | Pass | Fail | Pass Rate |
|--------|------|------|------|-----------|
| BuildServer | 5 | 4 | 1 | 80% |
| TestServerPostmanArticle | 3 | 1 | 2 | 33% |
| ... | | | | |
| **Total** | **N** | **X** | **Y** | **Z%** |
```

Append this table to PROGRESS.md under a new "## Agent Performance" section.

## Step 6: Append to PROGRESS.md

Append the score to `PROGRESS.md` under "Test Results Log":

```
- YYYY-MM-DD HH:MM — Final score: X/Z (N%) — Auth: X/Y, Profiles: X/Y, ArticlesEmpty: X/Y, Article: X/Y, FeedAndArticles: X/Y, E2E: X/Y
```

## Step 7: Append to SCORES.csv

Append a row to `SCORES.csv` with the results. The format is:

```
date,duration_minutes,auth,profiles,articles_empty,article,feed_and_articles,e2e,total_pass,total_tests,score
```

- `date`: ISO 8601 timestamp (e.g. `2026-03-27T14:30:00Z`)
- `duration_minutes`: estimate how long the session ran (from first commit to now)
- Each suite column: `X/Y` (passing/total)
- `total_pass`: sum of all passing
- `total_tests`: sum of all total
- `score`: decimal (e.g. `0.85`)

Example row:
```
2026-03-27T14:30:00Z,87,12/12,8/8,5/6,20/25,10/15,8/12,63,78,0.81
```

## Important

- Run ALL suites even if earlier ones fail — we need the complete picture.
- If a suite fails to run at all (e.g. build error), record 0/0 and note the error.
- Do NOT skip this skill. The score is the metric for the time-to-realworld challenge.
