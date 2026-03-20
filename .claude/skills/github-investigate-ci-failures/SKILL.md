---
description: Investigate CI failures. Use when a GitHub Actions CI job fails and needs diagnosis. Checks CI logs, identifies root cause, and attempts local reproduction. Invoked automatically by github-push-pr on failure.
---

Investigate a CI failure by examining logs, forming a hypothesis, and reproducing locally.

## Step 1: Identify failing jobs

Run `gh pr checks <PR_NUMBER> --repo <OWNER/REPO>` to list all CI job statuses. Note which jobs failed.

If a specific run ID or job URL was provided, use that directly.

## Step 2: Examine CI logs and form a hypothesis

Prioritize investigating more fundamental failures first: lint failures before build failures, build failures before test failures. A build failure may explain downstream test failures, so fixing the root cause first avoids wasted effort.

For each failing job, fetch logs using `gh run view <RUN_ID> --repo <OWNER/REPO> --job <JOB_ID> --log` and analyze them to understand what went wrong. Form a hypothesis about the root cause.

## Step 3: Reproduce locally

For each failing job, inspect the GitHub Actions workflow (`.github/workflows/ci.yml`) to find which Nuke target it invokes. If it runs a Nuke target, attempt to reproduce the failure locally using `./build.sh <target>`.

Regardless of whether the local run passes or fails, carefully inspect the full output logs for anomalies. A target may appear to pass while an underlying error has occurred. Compare local logs against the CI logs — do they match? If the failure reproduces locally, begin troubleshooting. If it only fails in CI, investigate environmental differences.

## Step 4: Report findings

Report to the user: which jobs failed, the root cause, whether it reproduced locally, and the fix applied or next steps.
