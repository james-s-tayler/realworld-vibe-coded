#!/bin/bash
# analyze-experiment.sh — Parse ACTION-LOG.md into structured JSON
# Usage: ./scripts/analyze-experiment.sh <worktree-dir>

set -eu

WORKTREE_DIR="${1:-.}"
LOG_FILE="$WORKTREE_DIR/ACTION-LOG.md"

if [ ! -f "$LOG_FILE" ]; then
  echo '{"error": "ACTION-LOG.md not found"}'
  exit 0
fi

# Skip header lines (start with # or >), process only action lines
ACTION_LINES=$(grep -E '^\[' "$LOG_FILE" || true)

if [ -z "$ACTION_LINES" ]; then
  echo '{"total_actions": 0, "by_tool": {}, "failures": {"total": 0}, "build_targets": {}, "timeline": {}, "retry_sequences": []}'
  exit 0
fi

TOTAL=$(echo "$ACTION_LINES" | wc -l)

# Count by tool
BY_TOOL=$(echo "$ACTION_LINES" | awk '{print $2}' | sort | uniq -c | sort -rn | awk '{printf "\"%s\": %d,", $2, $1}' | sed 's/,$//')

# Count failures by tool
FAIL_LINES=$(echo "$ACTION_LINES" | grep ' FAIL ' || true)
FAIL_TOTAL=0
FAIL_BY_TOOL='"total": 0'
if [ -n "$FAIL_LINES" ]; then
  FAIL_TOTAL=$(echo "$FAIL_LINES" | wc -l)
  FAIL_BY_TOOL_COUNTS=$(echo "$FAIL_LINES" | awk '{print $2}' | sort | uniq -c | sort -rn | awk '{printf "\"%s\": %d,", $2, $1}' | sed 's/,$//')
  FAIL_BY_TOOL="\"total\": $FAIL_TOTAL,$FAIL_BY_TOOL_COUNTS"
fi

# Build targets analysis
BUILD_LINES=$(echo "$ACTION_LINES" | grep 'Bash' | grep '\./build\.sh' || true)
BUILD_TARGETS="{}"
if [ -n "$BUILD_LINES" ]; then
  # Extract target name and pass/fail
  BUILD_DATA=$(echo "$BUILD_LINES" | while IFS= read -r line; do
    target=$(echo "$line" | grep -oP '(?<=\./build\.sh\s)\S+' | head -1)
    if [ -n "$target" ]; then
      if echo "$line" | grep -q ' PASS '; then
        echo "${target}:pass"
      else
        echo "${target}:fail"
      fi
    fi
  done)

  if [ -n "$BUILD_DATA" ]; then
    # Get unique targets
    TARGETS=$(echo "$BUILD_DATA" | cut -d: -f1 | sort -u)
    BUILD_JSON="{"
    first=true
    while IFS= read -r target; do
      [ -z "$target" ] && continue
      total_count=$(echo "$BUILD_DATA" | grep "^${target}:" | wc -l || true)
      pass_count=$(echo "$BUILD_DATA" | { grep "^${target}:pass" || true; } | wc -l)
      fail_count=$(echo "$BUILD_DATA" | { grep "^${target}:fail" || true; } | wc -l)
      if [ "$first" = true ]; then
        first=false
      else
        BUILD_JSON+=","
      fi
      BUILD_JSON+="\"$target\": {\"total\": $total_count, \"pass\": $pass_count, \"fail\": $fail_count}"
    done <<< "$TARGETS"
    BUILD_JSON+="}"
    BUILD_TARGETS="$BUILD_JSON"
  fi
fi

# Timeline
FIRST_TS=$(echo "$ACTION_LINES" | head -1 | grep -oP '(?<=\[)\d{2}:\d{2}:\d{2}')
LAST_TS=$(echo "$ACTION_LINES" | tail -1 | grep -oP '(?<=\[)\d{2}:\d{2}:\d{2}')
DURATION_MIN=""
if [ -n "$FIRST_TS" ] && [ -n "$LAST_TS" ]; then
  first_secs=$(echo "$FIRST_TS" | awk -F: '{print $1*3600 + $2*60 + $3}')
  last_secs=$(echo "$LAST_TS" | awk -F: '{print $1*3600 + $2*60 + $3}')
  diff_secs=$((last_secs - first_secs))
  # Handle day wrap
  if [ $diff_secs -lt 0 ]; then
    diff_secs=$((diff_secs + 86400))
  fi
  DURATION_MIN=$((diff_secs / 60))
fi

# Retry sequences: consecutive FAIL on same build target followed by PASS
RETRY_JSON="[]"
if [ -n "$BUILD_LINES" ]; then
  # Pre-process: extract "target status" pairs
  RETRY_INPUT=$(echo "$BUILD_LINES" | while IFS= read -r line; do
    target=$(echo "$line" | grep -oP '(?<=\./build\.sh\s)\S+' | head -1)
    if echo "$line" | grep -q ' PASS '; then
      echo "$target pass"
    else
      echo "$target fail"
    fi
  done)

  RETRY_ITEMS=""
  prev_target=""
  fail_count=0
  while IFS= read -r entry; do
    target=$(echo "$entry" | cut -d' ' -f1)
    status=$(echo "$entry" | cut -d' ' -f2)
    if [ "$target" = "$prev_target" ] && [ "$status" = "pass" ] && [ "$fail_count" -gt 0 ]; then
      [ -n "$RETRY_ITEMS" ] && RETRY_ITEMS+=","
      RETRY_ITEMS+="{\"target\": \"$target\", \"fails\": $fail_count, \"then_pass\": true}"
      fail_count=0
    elif [ "$target" = "$prev_target" ] && [ "$status" = "fail" ]; then
      fail_count=$((fail_count + 1))
    elif [ "$target" != "$prev_target" ]; then
      if [ "$fail_count" -gt 0 ] && [ -n "$prev_target" ]; then
        [ -n "$RETRY_ITEMS" ] && RETRY_ITEMS+=","
        RETRY_ITEMS+="{\"target\": \"$prev_target\", \"fails\": $fail_count, \"then_pass\": false}"
      fi
      if [ "$status" = "fail" ]; then
        fail_count=1
      else
        fail_count=0
      fi
    fi
    prev_target="$target"
  done <<< "$RETRY_INPUT"
  # Flush trailing failures
  if [ "$fail_count" -gt 0 ] && [ -n "$prev_target" ]; then
    [ -n "$RETRY_ITEMS" ] && RETRY_ITEMS+=","
    RETRY_ITEMS+="{\"target\": \"$prev_target\", \"fails\": $fail_count, \"then_pass\": false}"
  fi
  RETRY_JSON="[$RETRY_ITEMS]"
fi

# Nuke build log analysis — extract per-target durations from .nuke/temp/*.log
NUKE_DIR="$WORKTREE_DIR/.nuke/temp"
NUKE_TARGETS="{}"
if [ -d "$NUKE_DIR" ] && ls "$NUKE_DIR"/build*.log >/dev/null 2>&1; then
  # Extract events: "timestamp target_name event_type" from all logs
  # Log format: "18:01:22.970 | V | TargetName           | EventInvoker.OnTargetRunning ..."
  # build.log has timestamps; build.2026-*.log files don't. Use build.log if it has events.
  NUKE_EVENTS_FILE=$(mktemp)
  grep -E 'EventInvoker\.OnTarget(Running|Succeeded|Failed)' "$NUKE_DIR/build.log" 2>/dev/null | \
    sed -E 's/^([0-9:.]+) *\| *[A-Z] *\| *([A-Za-z]+) *\| *EventInvoker\.OnTarget(Running|Succeeded|Failed).*/\1 \2 \3/' \
    > "$NUKE_EVENTS_FILE" 2>/dev/null || true

  # Also extract from timestamped logs — they lack inline timestamps but we can
  # get per-log summary: filename has start time, content has target + result
  NUKE_LOG_SUMMARY=$(for logfile in "$NUKE_DIR"/build.2026-*.log; do
    [ -f "$logfile" ] || continue
    fname=$(basename "$logfile")
    # Extract targets and results from each log
    grep -E 'EventInvoker\.OnTarget(Succeeded|Failed)' "$logfile" 2>/dev/null | \
      sed -E 's/.*\| *([A-Za-z]+) *\| *EventInvoker\.OnTarget(Succeeded|Failed).*/\1 \2/' | \
      while IFS= read -r line; do echo "$fname $line"; done
  done)

  if [ -s "$NUKE_EVENTS_FILE" ]; then
    # Use timestamped events from build.log for per-target timing
    NUKE_TARGETS=$(awk '
    function to_secs(ts) {
      split(ts, p, /[:.]/);
      return p[1]*3600 + p[2]*60 + p[3] + (length(p)>=4 ? p[4]/1000 : 0)
    }
    {
      target = $2; event = $3; ts = to_secs($1)
      if (event == "Running") {
        start[target] = ts
      } else if (start[target] > 0) {
        dur = ts - start[target]
        runs[target]++
        total[target] += dur
        if (event == "Failed") fails[target]++
        delete start[target]
      }
    }
    END {
      printf "{"
      first = 1
      for (t in runs) {
        if (!first) printf ","
        avg = total[t] / runs[t]
        printf "\"%s\": {\"runs\": %d, \"fails\": %d, \"total_sec\": %.1f, \"avg_sec\": %.1f}", t, runs[t], fails[t]+0, total[t], avg
        first = 0
      }
      printf "}"
    }
    ' "$NUKE_EVENTS_FILE")
  elif [ -n "$NUKE_LOG_SUMMARY" ]; then
    # Fallback: count invocations from per-file summaries (no timing)
    NUKE_TARGETS=$(echo "$NUKE_LOG_SUMMARY" | awk '{
      target=$2; result=$3
      runs[target]++
      if (result == "Failed") fails[target]++
    }
    END {
      printf "{"
      first = 1
      for (t in runs) {
        if (!first) printf ","
        printf "\"%s\": {\"runs\": %d, \"fails\": %d}", t, runs[t], fails[t]+0
        first = 0
      }
      printf "}"
    }')
  fi
  rm -f "$NUKE_EVENTS_FILE"
fi

cat <<EOF
{
  "total_actions": $TOTAL,
  "by_tool": {$BY_TOOL},
  "failures": {$FAIL_BY_TOOL},
  "build_targets": $BUILD_TARGETS,
  "nuke_targets": $NUKE_TARGETS,
  "timeline": {"first": "$FIRST_TS", "last": "$LAST_TS", "duration_min": ${DURATION_MIN:-0}},
  "retry_sequences": $RETRY_JSON
}
EOF
