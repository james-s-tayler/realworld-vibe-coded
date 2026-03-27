#!/bin/bash
# alignment-audit.sh — Grep-based checklist scorer for principle alignment
# Usage: ./scripts/alignment-audit.sh [REPO_ROOT]
#
# Checks 30 items against harness files. Each passing item = 1 point (max 30).
# Outputs JSON: { "score": N, "max": 30, "items": [...] }

set -euo pipefail

REPO_ROOT="${1:-$(pwd)}"
score=0
items_json="["
first=true

check() {
  local id="$1"
  local name="$2"
  local result="$3" # "pass" or "fail"

  if [[ "$first" == "true" ]]; then
    first=false
  else
    items_json+=","
  fi

  if [[ "$result" == "pass" ]]; then
    score=$((score + 1))
    items_json+="{\"id\":$id,\"name\":\"$name\",\"pass\":true}"
  else
    items_json+="{\"id\":$id,\"name\":\"$name\",\"pass\":false}"
  fi
}

# Helper: count lines in a file (excluding blank lines and comments)
count_content_lines() {
  grep -cve '^\s*$' "$1" 2>/dev/null || echo "0"
}

# --- Context & Knowledge (P1, P7, P21, P23) ---

# 1. CLAUDE.md under 50 lines
claude_lines=$(wc -l < "$REPO_ROOT/CLAUDE.md" 2>/dev/null || echo "999")
[[ "$claude_lines" -le 50 ]] && check 1 "CLAUDE.md under 50 lines" "pass" || check 1 "CLAUDE.md under 50 lines" "fail"

# 2. Progressive disclosure — CLAUDE.md points to deeper docs, doesn't inline them
if grep -qE '(Docs/|\.md)' "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 2 "Progressive disclosure" "pass"
else
  check 2 "Progressive disclosure" "fail"
fi

# 3. Total mandatory reading (Session Start files) under 200 lines
total_mandatory=0
# Parse only numbered items (1. 2. 3.) in Session Start — not "Reference as needed" files
while IFS= read -r file; do
  if [[ -f "$REPO_ROOT/$file" ]]; then
    lines=$(wc -l < "$REPO_ROOT/$file")
    total_mandatory=$((total_mandatory + lines))
  fi
done < <(sed -n '/## Session Start/,/## /p' "$REPO_ROOT/CLAUDE.md" | grep -E '^[0-9]+\.' | grep -oE '`[^`]+\.md`' | tr -d '`')
[[ "$total_mandatory" -le 200 ]] && check 3 "Mandatory reading under 200 lines" "pass" || check 3 "Mandatory reading under 200 lines" "fail"

# 4. Agent-legible repo structure — clear naming, discoverable files
if [[ -d "$REPO_ROOT/App" && -d "$REPO_ROOT/Test" && -d "$REPO_ROOT/Task" && -d "$REPO_ROOT/Docs" ]]; then
  check 4 "Agent-legible repo structure" "pass"
else
  check 4 "Agent-legible repo structure" "fail"
fi

# 5. Specs as source of truth — SPEC-REFERENCE.md is canonical
if [[ -f "$REPO_ROOT/SPEC-REFERENCE.md" ]] && grep -q "SPEC-REFERENCE" "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 5 "Specs as source of truth" "pass"
else
  check 5 "Specs as source of truth" "fail"
fi

# --- Workflow & Process (P2, P3, P5, P20) ---

# 6. Research phase — agent analyzes spec before planning
if grep -qiE '(research|read.*spec|analyze.*spec|read.*SPEC-REFERENCE)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 6 "Research phase" "pass"
else
  check 6 "Research phase" "fail"
fi

# 7. Planning phase — agent generates execution plan from spec
if grep -qiE '(plan|generate.*plan|create.*plan|execution plan|dependency.*DAG|story order)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 7 "Planning phase" "pass"
else
  check 7 "Planning phase" "fail"
fi

# 8. Phase boundary validation — explicit gates between R -> P -> I
if grep -qiE '(phase.*boundary|validate.*before.*proceed|gate.*between|before.*proceeding|phase.*gate)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 8 "Phase boundary validation" "pass"
else
  check 8 "Phase boundary validation" "fail"
fi

# 9. Incremental execution — one feature/story at a time
if grep -qiE '(one.*story|one.*feature|incremental|each.*story|story.*loop|per.*story)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 9 "Incremental execution" "pass"
else
  check 9 "Incremental execution" "fail"
fi

# 10. Selection pressure loop — generate -> test -> reject/accept cycle
if grep -qiE '(test.*fix|run.*test.*fix|gate|regression.*fix|re-run)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 10 "Selection pressure loop" "pass"
else
  check 10 "Selection pressure loop" "fail"
fi

# --- Evaluation & Quality (P4, P9, P14) ---

# 11. Generator-evaluator separation — self-review before completion
if [[ -f "$REPO_ROOT/.claude/hooks/on-stop.sh" ]] || grep -qiE '(self-review|review.*before|on-stop)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 11 "Generator-evaluator separation" "pass"
else
  check 11 "Generator-evaluator separation" "fail"
fi

# 12. Automated backpressure — hooks enforce build/lint/test gates
if [[ -f "$REPO_ROOT/.claude/settings.json" ]] && grep -q "hooks" "$REPO_ROOT/.claude/settings.json" 2>/dev/null; then
  check 12 "Automated backpressure" "pass"
else
  check 12 "Automated backpressure" "fail"
fi

# 13. Test-before-commit enforcement — mechanical, not documentary
if [[ -f "$REPO_ROOT/.claude/hooks/test-before-commit.sh" ]]; then
  check 13 "Test-before-commit enforcement" "pass"
else
  check 13 "Test-before-commit enforcement" "fail"
fi

# 14. Regression detection — compare current results vs previous
if grep -qiE '(regression|previously passed.*now fails|compare.*previous|compare.*results)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 14 "Regression detection" "pass"
else
  check 14 "Regression detection" "fail"
fi

# --- Resilience & Recovery (P11, P12, P16, P25) ---

# 15. Circuit breaker — stuck detection with time limit
if grep -qiE '(circuit breaker|stuck.*20 min|stuck.*minutes|move.*next)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 15 "Circuit breaker" "pass"
else
  check 15 "Circuit breaker" "fail"
fi

# 16. Context management — explicit compaction guidance
if grep -qiE '(context.*management|compact|context.*large|context.*overflow)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 16 "Context management" "pass"
else
  check 16 "Context management" "fail"
fi

# 17. Session boundary recovery — PROGRESS.md survives resets
if [[ -f "$REPO_ROOT/PROGRESS.md" ]] && grep -qiE '(PROGRESS.md|progress.*survives|cross-session)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 17 "Session boundary recovery" "pass"
else
  check 17 "Session boundary recovery" "fail"
fi

# 18. Append-only progress — never delete, only add
if grep -qiE '(append.only|never delete|NEVER delete)' "$REPO_ROOT/PROGRESS.md" 2>/dev/null; then
  check 18 "Append-only progress" "pass"
else
  check 18 "Append-only progress" "fail"
fi

# 19. Assumptions labeled — scaffolding marked for potential removal
if grep -qiE '(assumption|scaffolding|may.*remov|potential.*removal)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null || \
   grep -qiE '(assumption|scaffolding)' "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 19 "Assumptions labeled" "pass"
else
  check 19 "Assumptions labeled" "fail"
fi

# 20. Designed for simplification — removing components is easy
if grep -qiE '(simplif|remov.*component|modular|decouple)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null || \
   grep -qiE '(simplif|modular)' "$REPO_ROOT/Docs/architecture.md" 2>/dev/null; then
  check 20 "Designed for simplification" "pass"
else
  check 20 "Designed for simplification" "fail"
fi

# --- Tactics ---

# 21. Session startup checklist — defined reading order at session start
if grep -qiE '(Session Start|read.*in order|read.*files)' "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 21 "Session startup checklist" "pass"
else
  check 21 "Session startup checklist" "fail"
fi

# 22. Infrastructure smoke test — verify env before implementation
if grep -qiE '(smoke test|infrastructure.*smoke|RunLocalDependencies|verify.*infra)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 22 "Infrastructure smoke test" "pass"
else
  check 22 "Infrastructure smoke test" "fail"
fi

# 23. Commit-gate semantics — progress survives via commits
if grep -qiE '(commit.*gate|commit.*progress|progress.*survives.*commit)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 23 "Commit-gate semantics" "pass"
else
  check 23 "Commit-gate semantics" "fail"
fi

# 24. Learning log — PROGRESS.md records gotchas/patterns
if grep -qiE '(gotcha|pattern|learned|discovery|Discovered)' "$REPO_ROOT/PROGRESS.md" 2>/dev/null; then
  check 24 "Learning log" "pass"
else
  check 24 "Learning log" "fail"
fi

# 25. Environmental context assembly — agent knows its tools/constraints upfront
if grep -qiE '(build.*command|nuke|build\.sh|test.*command)' "$REPO_ROOT/Docs/architecture.md" 2>/dev/null; then
  check 25 "Environmental context assembly" "pass"
else
  check 25 "Environmental context assembly" "fail"
fi

# 26. One task per commit — atomic, revertible changes
if grep -qiE '(commit.*story|commit.*feature|one.*commit|atomic)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 26 "One task per commit" "pass"
else
  check 26 "One task per commit" "fail"
fi

# 27. Loop/stuck detection — circuit breaker is mechanical, not advisory
if [[ -f "$REPO_ROOT/.claude/hooks/on-stop.sh" ]] && grep -qiE '(circuit breaker|stuck)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 27 "Loop/stuck detection" "pass"
else
  check 27 "Loop/stuck detection" "fail"
fi

# 28. Stop condition defined — clear done criteria
if grep -qiE '(stop condition|done when|win condition|all.*pass)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 28 "Stop condition defined" "pass"
else
  check 28 "Stop condition defined" "fail"
fi

# 29. Domain-agnostic workflow — harness works for any spec, not just RealWorld
# Fails if workflow references domain-specific artifacts like realworld-spec.md
if grep -qiE '(realworld-spec|exec-plans/active)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 29 "Domain-agnostic workflow" "fail"
elif grep -qiE '(exec.plan|active exec plan)' "$REPO_ROOT/Docs/workflow.md" 2>/dev/null; then
  check 29 "Domain-agnostic workflow" "fail"
else
  check 29 "Domain-agnostic workflow" "pass"
fi

# 30. Minimal domain coupling — only SPEC-REFERENCE.md carries domain knowledge
# Fails if CLAUDE.md references domain-specific exec plans
if grep -qiE '(realworld-spec|exec-plans/active)' "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 30 "Minimal domain coupling" "fail"
elif grep -qiE '(exec.plan.*realworld|active exec plan)' "$REPO_ROOT/CLAUDE.md" 2>/dev/null; then
  check 30 "Minimal domain coupling" "fail"
else
  check 30 "Minimal domain coupling" "pass"
fi

items_json+="]"

cat <<EOF
{
  "score": $score,
  "max": 30,
  "items": $items_json
}
EOF
