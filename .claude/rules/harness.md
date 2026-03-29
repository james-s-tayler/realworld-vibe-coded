---
paths:
  - ".claude/**"
  - "scripts/**"
---

## Path Portability

Use `$CLAUDE_PROJECT_DIR` in settings.json hook commands — never hardcode absolute paths.
Correct: `"$CLAUDE_PROJECT_DIR"/.claude/hooks/my-hook.sh`
Wrong: `/home/user/project/.claude/hooks/my-hook.sh`

## Hook Script Conventions

Hooks receive tool call JSON on stdin. Parse with `jq`:
```bash
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | jq -r '.tool_input.file_path // empty')
```

Exit codes: `0` = allow (with optional JSON output), `2` = block.
To block, output `hookSpecificOutput` JSON with `permissionDecision: "block"` and a `reason`.
To prompt user, use `permissionDecision: "ask"` with `permissionDecisionReason`.

## Settings.json Structure

- **Event types:** `PreToolUse`, `PostToolUse`, `PostToolUseFailure`, `Stop`
- **matcher:** regex against tool name (e.g., `"Bash"`, `"Edit|Write"`, `""` for all)
- **allowedTools:** auto-approve patterns (e.g., `"Bash(./build.sh *)"`)
- `settings.json` — checked in, shared; `settings.local.json` — gitignored, personal overrides

## Marker Files

Use `/tmp/claude-*` namespace for inter-hook state (e.g., `/tmp/claude-tests-ran`).
Hooks create/check these files within a session; they persist until system reboot or explicit cleanup.

## Rules File Format

- Frontmatter required: `paths:` array scoping when the rule is loaded
- Terse, imperative style — no prose paragraphs
- **≤85 lines per file** — enforced by `./build.sh LintClaudeRulesVerify`
- Split large topics into multiple scoped files (e.g., `backend.md` + `backend-analyzers.md`)

## Protected Files

`protect-files.sh` guards: `App/Server/analyzers/`, `.editorconfig`, `.husky/`, `.nuke/`, `.claude/`, `Nuke.Tests/`.
Modifications to these paths trigger an "ask" prompt — only modify when explicitly instructed.
