# Harness Assessment: Conduit vs. HumanLayer "Skill Issue" Advice

Assessment of our current agent harness against the recommendations in Kyle's "Skill Issue: Harness Engineering for Coding Agents" (March 2026).

---

## Scorecard

| Configuration Surface | Article Recommendation | Our Harness | Rating |
|---|---|---|---|
| Agentfile (CLAUDE.md) | <60 lines, hand-written, progressive disclosure | ~50 lines, rules index TOC, scoped rules in `.claude/rules/` | **Strong** |
| MCP Servers | Minimal, prefer CLIs, avoid context bloat | No MCP servers for dev tooling; all ops go through `./build.sh` Nuke targets | **Strong** |
| Skills | Progressive disclosure bundles, load on demand | 50+ skills (mostly Nuke target wrappers), plus self-review, debug, on-stop | **Mixed** |
| Sub-Agents | Task-based context firewalls, not role-based | Used for exploration (Task/Explore), CI monitoring; parent stays clean | **Good** |
| Hooks | Silent success, loud failure, exit code 2 | 10 hooks covering enforce, protect, gate, stats, action log | **Strong** |
| Back-Pressure | Typechecks, tests, coverage; context-efficient output | Commit gate requires build + all Postman + all E2E; stop hook forces score | **Strong** |
| Iterative evolution | Start simple, add on failure, pare down | Evolved organically from real failures over sessions | **Good** |

---

## What We're Doing Well

### 1. Progressive Disclosure (Principle 22 — Information-theoretic ceiling)

CLAUDE.md is ~50 lines with a rules index table pointing to 12 scoped rule files. Rules only load when path-matched. This is textbook progressive disclosure — the article explicitly endorses this pattern and criticizes monolithic instruction files.

### 2. Silent Success / Loud Failure Hooks (Core HumanLayer pattern)

Our hooks follow the exact pattern the article recommends:
- `enforce-nuke.sh` — blocks `dotnet`/`docker`/`npx`, exit 2 with guidance
- `enforce-tools.sh` — blocks `find`/`grep` in Bash, exit 2 with tool suggestion
- `protect-files.sh` — blocks edits to protected paths, exit 2
- `test-before-commit.sh` — blocks commit without gate markers, exit 2
- `mark-tests-ran.sh` — silent marker creation on success
- `on-stop.sh` — blocks session exit without verification suite

All follow the pattern: success is invisible, failure surfaces actionable context.

### 3. No MCP Server Bloat

We use zero MCP servers for development tooling. All build/test/lint operations go through `./build.sh` Nuke targets, which are CLI commands already in training data. The article specifically highlights HumanLayer replacing MCP servers with custom CLIs — we never added them in the first place.

### 4. Strong Back-Pressure Pipeline

The commit gate (`test-before-commit.sh`) mechanically enforces that `BuildServer`, all 5 Postman collections, and `TestE2e` must pass before any commit. The stop hook forces a full verification suite with score reporting. This is exactly the "verification at every boundary" the article advocates.

### 5. Mechanical Architecture Enforcement

Roslyn analyzers (SRV007 etc.) enforce backend patterns at compile time. The `enforce-nuke.sh` hook prevents bypassing the build system. `protect-files.sh` guards architectural files. This aligns with the article's emphasis that "encouragement-only approaches do not work at scale."

### 6. Agent Observability

`agent-stats-record.sh` tracks pass/fail per Nuke target. `action-log.sh` records every tool invocation. The on-stop skill reports scores to `SCORES.csv`. This gives us the trace data the article says is essential for iterative improvement.

---

## Improvement Opportunities

### 1. Context-Efficient Test Output (HIGH IMPACT)

**The problem:** The article's most actionable warning — running full test suites (4,000+ lines) floods context and causes hallucination. Our Nuke targets currently return full test output including all passing tests.

**The fix:** Add an output filter layer to Nuke test targets (or a PostToolUse hook on `./build.sh Test*`) that:
- On success: returns only a one-line summary ("TestServerPostmanAuth: 12/12 passing")
- On failure: returns the summary line + only the failing test details

This would dramatically reduce context consumption during the implement phase, where we run tests most frequently. Could be implemented as:
- A wrapper in `on-nuke-test-fail.sh` (already exists but unused in settings.json)
- A PostToolUse hook that parses test output and compacts it
- Nuke target modifications to produce machine-readable summary output

### 2. Tiered Sub-Agent Model Selection (MEDIUM IMPACT)

**The problem:** The article recommends using cheaper models (Haiku/Sonnet) for sub-agents while reserving expensive models (Opus) for orchestration. Our subagent spawns don't specify model preferences — they inherit the parent's Opus model.

**The fix:** When spawning Task agents for exploration/research, explicitly request `model: "haiku"` or `model: "sonnet"` for:
- File/code search (Explore agents)
- CI log analysis
- Grep/glob-heavy research

Reserve Opus for orchestration decisions, plan evaluation, and complex implementation. This is a convention change, not a harness change — document it in the workflow.

### 3. Skill Surface Area Audit (MEDIUM IMPACT)

**The problem:** We have 50+ skills, mostly auto-generated Nuke target wrappers. The article warns against "installing dozens of skills/MCP servers just in case" and notes this creates context bloat (skill descriptions inject into system prompt). The ETH Zurich study found agents spend 14-22% more reasoning tokens processing instructions.

**The fix:**
- Audit which skills are actually invoked (check `action-log.sh` data across sessions)
- Consider consolidating Nuke skills into a single "nuke" skill that accepts a target parameter, rather than 40 individual skills
- Keep only high-value custom skills (on-stop, self-review, debug, github-push-pr) as separate skills
- This would reduce the skill description injection from ~50 entries to ~15

### 4. Loop Detection Hook (MEDIUM IMPACT)

**The problem:** The article and LangChain both highlight that agents get stuck in edit loops — repeating the same unsuccessful approach indefinitely. We have the `/debug` skill as a manual escape hatch, but no automated detection.

**The fix:** Add a PostToolUse hook on `Edit|Write` that:
- Tracks which files are being edited and how frequently
- If the same file is edited 4+ times within a short window, injects a warning: "You've edited {file} {N} times. Consider using /debug to reassess your approach."
- Could track via `/tmp/claude-edit-tracker` with timestamp+filepath

### 5. Coverage Drop Detection Hook (LOW IMPACT, HIGH SIGNAL)

**The problem:** The article mentions using hooks to detect code coverage drops and prompt the agent to improve coverage. We don't currently track coverage.

**The fix:** Add coverage collection to the TestServer Nuke target and a PostToolUse hook that compares coverage before/after. If coverage drops, inject a warning before the commit gate allows passage.

### 6. On-Stop Full Suite Duration (LOW IMPACT)

**The problem:** The article explicitly lists "running entire test suite (5+ minutes) after every session" as something that didn't work. Our on-stop hook forces all 5 Postman collections + full E2E on every session exit.

**Assessment:** This is a conscious trade-off in our harness — we want the score metric. But we could:
- Skip on-stop verification if no code changes were made (check `git diff HEAD`)
- Only run suites related to changed files (if backend-only changes, skip E2E)
- Cache the last successful run and skip if no changes since

### 7. Test Output in Agent Stats (LOW IMPACT)

**The problem:** `agent-stats-record.sh` only records pass/fail. The article's "trace analyzer" approach categorizes failures to focus improvement efforts.

**The fix:** Enhance agent stats to capture failure category (compile error, test failure name, lint violation type). This enables cross-session analysis of which failure modes are most common, following the "boosting" approach from LangChain's trace analyzer.

---

## Summary: Priority Actions

1. **Context-efficient test output** — Biggest bang for buck. Filter test output to summary+failures only.
2. **Skill surface audit** — Consolidate 40 Nuke wrapper skills into fewer entry points.
3. **Loop detection hook** — Automated detection of edit loops with escape hatch suggestion.
4. **Sub-agent model tiering** — Convention to use cheaper models for exploration/search.
5. **Smart on-stop** — Skip verification when no code changes exist.
