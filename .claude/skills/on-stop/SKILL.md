---
description: Run before finishing to run the full test suite and report a score. Triggered by the Stop hook.
---

Run the complete test suite and report a pass/fail score. This is mandatory before finishing.

## Step 1: Run all Postman test collections

Run each collection individually and record results:

```bash
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanArticle
./build.sh TestServerPostmanFeedAndArticles
```

For each collection, note: X passing, Y failing out of Z total.

## Step 2: Run E2E test suite

```bash
./build.sh TestE2e
```

Note: X passing, Y failing out of Z total.

## Step 3: Calculate and report score

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

## Step 4: Append to PROGRESS.md

Append the score to `PROGRESS.md` under "Test Results Log":

```
- YYYY-MM-DD HH:MM — Final score: X/Z (N%) — Auth: X/Y, Profiles: X/Y, ArticlesEmpty: X/Y, Article: X/Y, FeedAndArticles: X/Y, E2E: X/Y
```

## Important

- Run ALL suites even if earlier ones fail — we need the complete picture.
- If a suite fails to run at all (e.g. build error), record 0/0 and note the error.
- Do NOT skip this skill. The score is the metric for the time-to-realworld challenge.
