#!/bin/bash
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | jq -r '.tool_input.command // empty')

# Only intercept git commit commands
if ! echo "$COMMAND" | grep -qP '^git\s+commit\b'; then
  exit 0
fi

CIRCUIT_BREAKER_MARKER="/tmp/claude-circuit-breaker-skip"

# Allow commits when circuit breaker is active (agent is skipping a stuck feature)
if [ -f "$CIRCUIT_BREAKER_MARKER" ]; then
  rm -f "$CIRCUIT_BREAKER_MARKER"
  exit 0
fi

# Require full gate completion
REQUIRED_MARKERS=(
  "LintAllVerify"
  "BuildServer"
  "TestServerPostmanAuth"
  "TestServerPostmanProfiles"
  "TestServerPostmanArticlesEmpty"
  "TestServerPostmanArticle"
  "TestServerPostmanFeedAndArticles"
  "TestE2e"
)

MISSING=()
for marker in "${REQUIRED_MARKERS[@]}"; do
  if [ ! -f "/tmp/claude-gate/$marker" ]; then
    MISSING+=("$marker")
  fi
done

if [ ${#MISSING[@]} -gt 0 ]; then
  echo "BLOCKED: Missing gate steps: ${MISSING[*]}" >&2
  echo "Run all gate steps before committing. If using the circuit breaker: touch /tmp/claude-circuit-breaker-skip" >&2
  exit 2
fi

# Gate passed — clear markers for next commit cycle
rm -rf /tmp/claude-gate
exit 0
