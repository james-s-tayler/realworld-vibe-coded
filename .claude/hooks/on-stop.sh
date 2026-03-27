#!/bin/bash
INPUT=$(cat)
STOP_REASON=$(echo "$INPUT" | jq -r '.stop_hook_reason // empty')

# Only fire on end_turn, not interrupts
if [ "$STOP_REASON" != "end_turn" ]; then
  exit 0
fi

# Prevent infinite loops — if the marker exists, verification already ran
MARKER="/tmp/claude-stop-hook-active"
if [ -f "$MARKER" ]; then
  rm -f "$MARKER"
  exit 0
fi

# Create marker so the next stop allows through
touch "$MARKER"

cat >&2 << 'EOF'
BLOCKED: Session cannot end without running the verification suite.

You MUST execute the /on-stop skill before finishing. This skill:
1. Runs all 5 Postman test collections
2. Runs TestE2e
3. Records scores to SCORES.csv and PROGRESS.md

Run this command now: /on-stop

If E2E tests fail due to infrastructure (not code), document the failure in PROGRESS.md and create /tmp/claude-stop-hook-active to bypass.
EOF
exit 2
