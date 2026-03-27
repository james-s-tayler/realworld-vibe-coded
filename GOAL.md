# GOAL: Optimize the Agent Harness

## Objective

Maximize `harness-quality` score (0-100) by iteratively improving the agent harness — the set of files (CLAUDE.md, workflow.md, PROGRESS.md, architecture.md, hooks, rules) that guide Claude Code to implement any spec from a single starter prompt.

The current domain is RealWorld (Medium clone), but the harness must generalize. The only domain-specific input is `SPEC-REFERENCE.md`.

## Metric: harness-quality (0-100)

```
if all tests pass:
  score = 50 + alignment_bonus(0-30) + time_bonus(0-20)
else:
  score = floor(tests_passing / total_tests * 45)    # max 45 for failing runs
```

| Component | Range | Weight | Formula |
|-----------|-------|--------|---------|
| **All tests pass** | 50 base (or 0-45 partial) | Highest | Gate: all tests = 50pts base. Partial: `floor(pass/total * 45)` |
| **Principle alignment** | 0-30 | Second | 30-item checklist audit of harness vs principles+tactics |
| **Time-To-RealWorld** | 0-20 | Third | `max(0, floor(20 * (90 - min) / 60))`. 30min=20, 60min=10, 90min=0 |

Any passing run (50+) always beats any failing run (max 45).

## Fitness Function

```bash
# Full experiment (runs agent + tests + audit):
./scripts/score.sh

# Alignment audit only (fast, no experiment):
./scripts/score.sh --audit-only

# Reuse existing experiment results:
./scripts/score.sh --skip-experiment --results-file /tmp/harness-runs/run-xxx-results.json
```

## Principle Alignment Checklist (30 items, 1 pt each)

Audited by `scripts/alignment-audit.sh`. Grouped by source principle:

**Context & Knowledge (P1, P7, P21, P23):**
1. CLAUDE.md under 50 lines
2. Progressive disclosure — CLAUDE.md points to deeper docs
3. Total mandatory reading (Session Start files) under 200 lines
4. Agent-legible repo structure — clear naming, discoverable files
5. Specs as source of truth — SPEC-REFERENCE.md is canonical

**Workflow & Process (P2, P3, P5, P20):**
6. Research phase — agent analyzes spec before planning
7. Planning phase — agent generates execution plan from spec
8. Phase boundary validation — explicit gates between R → P → I
9. Incremental execution — one feature/story at a time
10. Selection pressure loop — generate → test → reject/accept cycle

**Evaluation & Quality (P4, P9, P14):**
11. Generator-evaluator separation — self-review before completion
12. Automated backpressure — hooks enforce build/lint/test gates
13. Test-before-commit enforcement — mechanical, not documentary
14. Regression detection — compare current results vs previous

**Resilience & Recovery (P11, P12, P16, P25):**
15. Circuit breaker — stuck detection with time limit
16. Context management — explicit compaction guidance
17. Session boundary recovery — PROGRESS.md survives resets
18. Append-only progress — never delete, only add
19. Assumptions labeled — scaffolding marked for potential removal
20. Designed for simplification — removing components is easy
21. Session startup checklist — defined reading order at session start
22. Infrastructure smoke test — verify env before implementation
23. Commit-gate semantics — progress survives via commits
24. Learning log — PROGRESS.md records gotchas/patterns
25. Environmental context assembly — agent knows its tools/constraints
26. One task per commit — atomic, revertible changes
27. Loop/stuck detection — circuit breaker is mechanical
28. Stop condition defined — clear "done" criteria
29. Domain-agnostic workflow — harness works for any spec
30. Minimal domain coupling — only SPEC-REFERENCE.md carries domain knowledge

## Constraints

1. **Tests immutable** — Postman collections and E2E tests cannot be modified
2. **Skills auto-generated** — don't remove skill directories
3. **on-stop hook essential** — required for E2E test execution
4. **Full gate stays** — all test suites required; don't reduce gate frequency
5. **No domain-specific knowledge** — harness must generalize; only SPEC-REFERENCE.md is domain input
6. **Exec plan agent-generated** — DAG/story order created by agent from spec, not pre-baked
7. **Roslyn analyzers immutable** — protected files
8. **One change per iteration** — for attribution
9. **Scoring formula locked** — prevents gaming

## Action Catalog

### Tier 1: Structural — Make Harness Domain-Agnostic

| # | Action | Est. Impact | Status |
|---|--------|-------------|--------|
| 1 | Replace baked exec plan with R→P→I workflow | +10-15 | done (24→28) |
| 2 | Add explicit R→P→I phase structure to workflow.md | +5-8 | done (merged into #1) |
| 3 | Add phase boundary validation | +3-5 | done (merged into #1) |

### Tier 2: Workflow Optimization

| # | Action | Est. Impact | Status |
|---|--------|-------------|--------|
| 4 | Compress SPEC-REFERENCE.md | +3-5 | pending |
| 5 | Add regression detection (domain-agnostic) | +2-3 | pending |
| 6 | Improve PROGRESS.md template | +2-3 | pending |
| 7 | Add context utilization guidance | +1-2 | pending |

### Tier 3: Refinement

| # | Action | Est. Impact | Status |
|---|--------|-------------|--------|
| 8 | Add assumptions-to-stress-test section | +1-2 | pending |
| 9 | Improve environmental context assembly | +1-2 | pending |
| 10 | Optimize starter prompt | +1-2 | pending |

## Improvement Loop

```
1. Read GOAL.md (this file) and iterations.jsonl
2. Pick the highest-impact pending action from the Action Catalog
3. Implement ONE change (constraint: one change per iteration)
4. Run ./scripts/score.sh --audit-only to check alignment impact
5. If alignment improved: run ./scripts/score.sh for full experiment
6. Record result in iterations.jsonl
7. Update action status in this file
8. Repeat from step 1
```

## Bootstrap

**Baseline alignment score (pre-optimization):** 24/30
**Post-Action #1 alignment score:** 30/30

Changes: Removed exec plan reference from CLAUDE.md, rewrote workflow.md with R→P→I phases + phase gates + scaffolding assumptions section, enhanced PROGRESS.md template with Plan Generated section, fixed audit to count only numbered mandatory files.

**Iteration 0 — Baseline experiment (R→P→I harness):**
- Score: **84/100** (tests=50, alignment=30, time=4)
- All 6 test suites passed (exit code 0). 55/55 E2E tests passed.
- Agent duration: 78 min (time bonus: 4/20)
- Agent made 2 commits: follow/unfollow + articles/tags/list
- Newman reports lost to PathsCleanDirectories bug (fixed in d16830d0)

## Iteration Log

See `iterations.jsonl` for machine-readable history.
