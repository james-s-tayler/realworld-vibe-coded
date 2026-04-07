#!/bin/bash
# lint-spec.sh — Structural completeness check for SPEC-REFERENCE.md
#
# Deterministic (no LLM call). Checks that required sections and patterns
# exist in the spec. Catches the categories that caused eval coverage gaps.
#
# Usage: ./scripts/evals/lint-spec.sh [--spec FILE]
# Exit code: 0 = pass, 1 = failures found

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
SPEC_FILE="${1:-$REPO_ROOT/SPEC-REFERENCE.md}"

if [[ "${1:-}" == "--spec" ]]; then
  SPEC_FILE="${2:-$REPO_ROOT/SPEC-REFERENCE.md}"
fi

if [[ ! -f "$SPEC_FILE" ]]; then
  echo "ERROR: Spec file not found: $SPEC_FILE" >&2
  exit 1
fi

FAILURES=0
WARNINGS=0
SPEC=$(cat "$SPEC_FILE")

pass() { echo "  PASS  $1"; }
fail() { echo "  FAIL  $1"; FAILURES=$((FAILURES + 1)); }
warn() { echo "  WARN  $1"; WARNINGS=$((WARNINGS + 1)); }

check_section() {
  local heading="$1"
  local description="$2"
  if echo "$SPEC" | grep -qi "$heading"; then
    pass "$description"
  else
    fail "$description — missing section matching '$heading'"
  fi
}

check_pattern() {
  local pattern="$1"
  local description="$2"
  local min_count="${3:-1}"
  local count
  count=$(echo "$SPEC" | grep -ci "$pattern" || true)
  if [[ "$count" -ge "$min_count" ]]; then
    pass "$description ($count found)"
  else
    fail "$description — expected at least $min_count, found $count"
  fi
}

echo "=== Spec Structural Lint: $(basename "$SPEC_FILE") ==="
echo ""

# --- Required top-level sections ---
echo "--- Required Sections ---"
check_section "## Overview"                    "Overview section"
check_section "## Base URL"                    "Base URL & Conventions"
check_section "## Error Response"              "Error Response Format"
check_section "## Endpoints"                   "At least one Endpoints section"
check_section "## Data Models"                 "Data Models section"
check_section "## Business Rules"              "Business Rules section"
check_section "## Frontend UI Behaviors"       "Frontend UI Behaviors section"

echo ""

# --- Frontend UI subsections ---
echo "--- Frontend UI Subsections ---"
check_section "### Navigation"                 "Navigation & Layout"
check_section "### Route Guards"               "Route Guards (Frontend Redirects)"
check_section "### Mobile"                     "Mobile Responsive Behavior"
check_section "### Screenshots"                "Screenshots / Visual Regression"

echo ""

# --- Frontend behavior patterns ---
echo "--- Frontend Behavior Coverage ---"
check_pattern "/login"                         "Login redirect references" 3
check_pattern "error.*message.*display\|display.*error\|visible.*error\|error.*visible\|error.*UI\|UI.*error" \
                                               "UI error display descriptions" 3
check_pattern "mobile\|375\|hamburger\|sidebar.*hidden\|responsive" \
                                               "Mobile-specific behavior descriptions" 3
check_pattern "screenshot\|visual regression"  "Screenshot/visual regression entries" 3
check_pattern "feature.flag\|feature flag"     "Feature flag references" 1

echo ""

# --- API completeness signals ---
echo "--- API Completeness ---"
check_pattern "## Endpoint"                    "Endpoint group sections" 3
check_pattern "Auth.*Required\|Auth.*Bearer\|Auth.*Not required" \
                                               "Auth requirement declarations" 5
check_pattern "Success Response\|200 OK\|201 Created\|204 No Content" \
                                               "Success response examples" 5
check_pattern "Error Response\|400 Bad\|401 Unauthorized\|403 Forbidden\|404 Not Found" \
                                               "Error response documentation" 5
check_pattern "Validation Rules\|required.*not empty\|min.*length\|max.*length" \
                                               "Validation rule declarations" 3

echo ""

# --- Dual-perspective checks ---
echo "--- Dual-Perspective (API + UI) ---"

# Check that redirect behaviors mention browser/frontend, not just API 401
redirect_mentions=$(echo "$SPEC" | grep -ci "redirect.*login\|browser.*url\|frontend.*redirect\|route guard" || true)
api_only_auth=$(echo "$SPEC" | grep -ci "401 Unauthorized" || true)
if [[ "$redirect_mentions" -ge 3 ]]; then
  pass "Frontend redirect behaviors documented ($redirect_mentions references)"
else
  if [[ "$api_only_auth" -ge 3 ]]; then
    warn "Found $api_only_auth API auth references but only $redirect_mentions frontend redirect descriptions — spec may be API-only for auth"
  else
    fail "Missing frontend redirect behavior documentation"
  fi
fi

# Check that validation errors describe UI display, not just status codes
ui_error_display=$(echo "$SPEC" | grep -ci "error.*displayed\|error.*visible\|displays.*error\|shows.*error\|message.*DOM\|visible.*user" || true)
if [[ "$ui_error_display" -ge 3 ]]; then
  pass "UI error display behaviors documented ($ui_error_display references)"
else
  warn "Only $ui_error_display UI error display descriptions — validation errors may only describe API status codes"
fi

# Check for pagination UI (not just API limit/offset)
pagination_ui=$(echo "$SPEC" | grep -ci "pagination.*click\|pagination.*navigat\|page.*number\|pagination.*control\|pagination.*render" || true)
pagination_api=$(echo "$SPEC" | grep -ci "limit.*offset\|offset.*limit" || true)
if [[ "$pagination_ui" -ge 2 ]]; then
  pass "Pagination UI behavior documented ($pagination_ui references)"
else
  if [[ "$pagination_api" -ge 2 ]]; then
    warn "Found $pagination_api API pagination references but only $pagination_ui UI pagination descriptions"
  fi
fi

echo ""

# --- Summary ---
echo "=== Summary ==="
TOTAL=$((FAILURES + WARNINGS))
if [[ "$FAILURES" -eq 0 && "$WARNINGS" -eq 0 ]]; then
  echo "All checks passed."
elif [[ "$FAILURES" -eq 0 ]]; then
  echo "$WARNINGS warning(s), 0 failures."
else
  echo "$FAILURES failure(s), $WARNINGS warning(s)."
fi

exit "$FAILURES"
