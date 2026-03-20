---
description: Run before finishing to verify changes. Triggered by the Stop hook — maps modified files to minimal lint and test targets.
---

Run verification based on what files actually changed. Skip if nothing was modified.

## Step 1: Identify changed files

Run `git diff --name-only HEAD` to get all modified files (staged + unstaged) relative to last commit.

If no files changed, skip verification entirely and stop.

## Step 2: Map to lint targets

Match changed paths to the minimal set of lint targets:

| Path pattern | Lint target |
|:-------------|:------------|
| `App/Server/**` | `LintServerVerify` |
| `App/Client/**` | `LintClientVerify` |
| `Task/Runner/**` | `LintNukeVerify` |
| `Test/e2e/**` | `LintAllVerify` |
| `.claude/skills/**` | `LintSkillsVerify` |
| `CLAUDE.md` | `LintClaudeMdVerify` |

## Step 3: Map to test targets

Match changed paths to the minimal set of test targets:

| Path pattern | Test target |
|:-------------|:------------|
| `App/Server/**` | `TestServer` |
| `App/Client/**` | `TestClient` |
| `Test/e2e/**` | `TestE2e` |
| `App/Server/**` AND `App/Client/**` | `TestE2e` |

When both backend and frontend changed, `TestE2e` is required — it's the only test that validates the actual contract between the two layers.

For server endpoint/handler/validator changes, also run the relevant Postman collection:

| Endpoint area | Postman target |
|:-------------|:---------------|
| Auth/identity endpoints | `TestServerPostmanAuth` |
| Profile endpoints | `TestServerPostmanProfiles` |
| Article CRUD/comments/favorites | `TestServerPostmanArticle` |
| Feed/article listing | `TestServerPostmanFeedAndArticles` |
| Unclear which collection | `TestServerPostmanAuth` (smoke test) |

## Step 4: Execute

Run each target sequentially via `./build.sh <target>`. Stop on first failure.

**Lint targets first, then test targets.**

If a lint target fails, run the corresponding `*Fix` target and re-verify before moving to tests:
- `LintServerVerify` → `LintServerFix`
- `LintClientVerify` → `LintClientFix`
- `LintNukeVerify` → `LintNukeFix`
- `LintAllVerify` → `LintAllFix`
- `LintSkillsVerify` → `LintSkillsFix`

Report results to the user when done.
