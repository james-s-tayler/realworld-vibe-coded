#!/bin/bash
# extract-trace.sh — Extract Playwright trace zip into structured JSON for model grading
#
# Playwright trace zips contain NDJSON files:
#   trace.trace    — actions, DOM snapshots, console, screenshots
#   trace.network  — network requests/responses
#   resources/     — binary blobs (screenshots, DOM HTML, response bodies)
#
# Usage: ./scripts/evals/extract-trace.sh <trace.zip> [--screenshots-dir DIR]
#
# Output: JSON to stdout with { actions, network, dom_snapshots, console, screenshots, metadata }

set -euo pipefail

TRACE_ZIP="${1:?Usage: extract-trace.sh <trace.zip> [--screenshots-dir DIR]}"
SCREENSHOTS_DIR=""

shift || true
while [[ $# -gt 0 ]]; do
  case $1 in
    --screenshots-dir) SCREENSHOTS_DIR="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

if [[ ! -f "$TRACE_ZIP" ]]; then
  echo "ERROR: Trace file not found: $TRACE_ZIP" >&2
  exit 1
fi

# Create temp dir for extraction
TMPDIR=$(mktemp -d)
trap 'rm -rf "$TMPDIR"' EXIT

# Unzip trace
unzip -q -o "$TRACE_ZIP" -d "$TMPDIR"

# Find trace files — may be chunked (trace.trace, trace-chunk1.trace, etc.)
# Use associative array to deduplicate (overlapping globs can match the same file)
declare -A SEEN_TRACE SEEN_NETWORK
TRACE_FILES=()
for f in "$TMPDIR"/trace*.trace "$TMPDIR"/*-chunk*.trace; do
  [[ -f "$f" && -z "${SEEN_TRACE[$f]:-}" ]] && TRACE_FILES+=("$f") && SEEN_TRACE[$f]=1
done

NETWORK_FILES=()
for f in "$TMPDIR"/trace*.network "$TMPDIR"/*-chunk*.network; do
  [[ -f "$f" && -z "${SEEN_NETWORK[$f]:-}" ]] && NETWORK_FILES+=("$f") && SEEN_NETWORK[$f]=1
done

# Extract actions (before/after pairs)
extract_actions() {
  for f in "${TRACE_FILES[@]}"; do
    cat "$f"
  done | jq -c 'select(.type == "before" or .type == "after")' 2>/dev/null | \
  jq -s '
    # Group by callId to pair before/after
    group_by(.callId) |
    map(
      (map(select(.type == "before")) | first) as $before |
      (map(select(.type == "after")) | first) as $after |
      {
        callId: ($before // $after).callId,
        method: ($before.apiName // ($before.class + "." + $before.method)),
        params: $before.params,
        wallTime: $before.wallTime,
        duration_ms: (if $after.endTime and $before.startTime then ($after.endTime - $before.startTime) else null end),
        error: $after.error,
        result_error: ($after.result.error // null),
        passed: ($after.error == null and ($after.result.error // null) == null)
      }
    ) |
    # Filter to interesting actions (skip internal playwright setup)
    map(select(
      .method != null and (
        (.method | test("goto|click|fill|check|uncheck|press|selectOption|setInputFiles|tap|type|dispatch"; "i")) or
        (.method | test("expect|assert"; "i")) or
        (.method | test("page\\.goto|locator\\.|frame\\."; "i"))
      )
    ))
  '
}

# Extract network requests
extract_network() {
  if [[ ${#NETWORK_FILES[@]} -eq 0 ]]; then
    echo "[]"
    return
  fi

  for f in "${NETWORK_FILES[@]}"; do
    cat "$f"
  done | jq -c '.' 2>/dev/null | jq -s '
    map(
      # Network entries may have different shapes depending on trace version
      if .request then
        {
          method: .request.method,
          url: .request.url,
          status: (.response.status // null),
          status_text: (.response.statusText // null),
          request_headers: (.request.headers // []),
          response_headers: (.response.headers // []),
          resource_type: (.resourceType // null),
          request_body_sha1: (.request.postData // null),
          response_body_sha1: (.response.content.sha1 // null)
        }
      else
        # resource-snapshot format
        {
          method: (.method // null),
          url: (.url // null),
          status: (.status // null),
          resource_type: (.type // null)
        }
      end
    ) |
    # Filter to API calls (skip static assets)
    map(select(
      .url != null and (
        (.url | test("/api/"; "i")) or
        (.url | test("/identity/"; "i")) or
        (.url | test("/dev-only/"; "i"))
      )
    ))
  '
}

# Extract DOM snapshots — get visible text from frame-snapshot HTML resources
extract_dom_snapshots() {
  for f in "${TRACE_FILES[@]}"; do
    cat "$f"
  done | jq -c 'select(.type == "frame-snapshot")' 2>/dev/null | \
  jq -s '
    # Take a sample — every 5th snapshot to avoid overwhelming output
    [to_entries[] | select(.key % 5 == 0) | .value] |
    map({
      snapshot_id: .callId,
      wallTime: .wallTime,
      sha1: .snapshot.sha1
    })
  ' | jq -c '.' 2>/dev/null || echo "[]"
}

# Extract console messages
extract_console() {
  for f in "${TRACE_FILES[@]}"; do
    cat "$f"
  done | jq -c 'select(.type == "console")' 2>/dev/null | jq -s '
    map({
      type: .messageType,
      text: .text,
      url: .url,
      line: .lineNumber
    }) |
    # Only keep warnings and errors
    map(select(.type == "warning" or .type == "error"))
  '
}

# Extract metadata (context options, viewport, etc.)
extract_metadata() {
  for f in "${TRACE_FILES[@]}"; do
    cat "$f"
  done | jq -c 'select(.type == "context-options")' 2>/dev/null | head -1 | \
  jq '{
    viewport: .options.viewport,
    userAgent: .options.userAgent,
    baseURL: .options.baseURL,
    isMobile: .options.isMobile,
    hasTouch: .options.hasTouch
  }' 2>/dev/null || echo "{}"
}

# Extract screenshots if requested
extract_screenshots() {
  if [[ -z "$SCREENSHOTS_DIR" ]]; then
    echo "[]"
    return
  fi

  mkdir -p "$SCREENSHOTS_DIR"

  local screenshot_paths=()
  for f in "${TRACE_FILES[@]}"; do
    cat "$f"
  done | jq -c 'select(.type == "screencast-frame")' 2>/dev/null | while IFS= read -r line; do
    sha1=$(echo "$line" | jq -r '.sha1')
    timestamp=$(echo "$line" | jq -r '.wallTime // .timestamp // "unknown"')
    resource_file="$TMPDIR/resources/$sha1"
    if [[ -f "$resource_file" ]]; then
      out_file="$SCREENSHOTS_DIR/${timestamp}_${sha1}.jpeg"
      cp "$resource_file" "$out_file"
      echo "$out_file"
    fi
  done | jq -R -s 'split("\n") | map(select(length > 0))'
}

# Resolve DOM snapshot SHA1s to visible text
resolve_dom_text() {
  local snapshots="$1"
  echo "$snapshots" | jq -c '.[]' | while IFS= read -r snap; do
    sha1=$(echo "$snap" | jq -r '.sha1 // empty')
    if [[ -n "$sha1" && -f "$TMPDIR/resources/$sha1" ]]; then
      # Extract visible text from HTML — strip tags, collapse whitespace
      visible_text=$(sed 's/<[^>]*>//g' "$TMPDIR/resources/$sha1" 2>/dev/null | \
        tr '\n' ' ' | sed 's/  */ /g' | head -c 2000)
      echo "$snap" | jq --arg text "$visible_text" '. + {visible_text: $text}'
    else
      echo "$snap"
    fi
  done | jq -s '.'
}

# Build the full extraction
ACTIONS=$(extract_actions)
NETWORK=$(extract_network)
DOM_RAW=$(extract_dom_snapshots)
DOM_SNAPSHOTS=$(resolve_dom_text "$DOM_RAW")
CONSOLE=$(extract_console)
METADATA=$(extract_metadata)
SCREENSHOTS=$(extract_screenshots)

# Compose final JSON
jq -n \
  --argjson actions "$ACTIONS" \
  --argjson network "$NETWORK" \
  --argjson dom_snapshots "$DOM_SNAPSHOTS" \
  --argjson console "$CONSOLE" \
  --argjson metadata "$METADATA" \
  --argjson screenshots "$SCREENSHOTS" \
  --arg source_file "$(basename "$TRACE_ZIP")" \
  '{
    source_file: $source_file,
    metadata: $metadata,
    actions: $actions,
    network: $network,
    dom_snapshots: $dom_snapshots,
    console_messages: $console,
    screenshots: $screenshots,
    summary: {
      total_actions: ($actions | length),
      total_network_calls: ($network | length),
      total_dom_snapshots: ($dom_snapshots | length),
      total_console_warnings_errors: ($console | length),
      failed_actions: ([$actions[] | select(.passed == false)] | length)
    }
  }'
