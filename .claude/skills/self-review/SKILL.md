---
description: Review your own changes for agent-specific failure modes before finishing. Run as part of on-stop or standalone.
---

Review all changes made in the current session for common agent failure modes.

## Step 1: Get the diff

Run `git diff HEAD` to see all uncommitted changes. If there are also staged changes, run `git diff --cached` too.

If no changes exist, skip this review.

## Step 2: Check for agent-specific failure modes

Review the diff against each item. For each violation found, fix it before proceeding.

### Over-abstraction
- Did you create helper methods, utility classes, or abstractions that are only used once?
- Did you add a base class or interface for a single implementation?
- Could you inline the abstraction and make the code simpler?

**Fix:** Inline single-use abstractions. Three similar lines of code is better than a premature abstraction.

### Missing or extra response fields
- Does every API response match the exact shape in `SPEC-REFERENCE.md`?
- Are there fields in the response that the spec doesn't require?
- Are there fields the spec requires that are missing or null?

**Fix:** Compare your response DTOs and mappers against `SPEC-REFERENCE.md` field-by-field.

### Unnecessary error handling
- Did you add try/catch blocks around code that can't throw?
- Did you add null checks for values guaranteed non-null by the framework?
- Did you add fallback behavior for scenarios that can't happen?

**Fix:** Remove error handling that guards against impossible cases. Trust the `Result<T>` pipeline and MediatR behaviors — they handle exceptions, transactions, and logging automatically.

### Hardcoded values
- Are there string literals that should be constants on the entity class?
- Are there magic numbers (status codes, lengths) that should reference domain constants?

**Fix:** Move to constants on the relevant entity or a shared constants class.

### Stale TODO/FIXME/HACK comments
- Did you leave any `TODO`, `FIXME`, or `HACK` comments in the code you wrote?
- Are there any from a previous iteration that are now resolved?

**Fix:** Remove them. If the work is genuinely deferred, note it in `PROGRESS.md` under Blocked Items instead.

## Step 3: Report

If violations were found and fixed, briefly note what was cleaned up. If the review was clean, just say so and move on.
