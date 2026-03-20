---
description: Push to PR and monitor CI. Use when you need to push commits and wait for CI results. Pushes, polls until all jobs finish, and auto-investigates failures.
---

Push the current branch to the remote PR and monitor CI until all jobs complete.

## Step 1: Push

Use `git push` to push the current branch. If the branch has no upstream, use `git push -u origin <branch>`.

## Step 2: Poll CI status

Use `gh pr checks <PR_NUMBER> --repo <OWNER/REPO> --watch` to poll until all CI jobs have finished. If `--watch` is unavailable, poll manually with `gh pr checks` every 30 seconds until no jobs are in `pending` or `in_progress` state.

## Step 3: Evaluate results

Once all jobs have completed, check if any failed. If all jobs passed, report success to the user.

If any jobs failed, invoke the `github-investigate-ci-failures` skill to diagnose and attempt to fix the failures.
