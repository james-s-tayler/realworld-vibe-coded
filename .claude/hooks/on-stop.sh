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

echo "IMPORTANT: Before finishing, run the on-stop skill to verify your changes. Use: /on-stop"
exit 2
