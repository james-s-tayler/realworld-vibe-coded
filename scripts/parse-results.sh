#!/bin/bash
# parse-results.sh — Parse test output from Reports/ directory into JSON counts
# Usage: ./scripts/parse-results.sh [REPO_ROOT]
#
# Parses Newman JSON reports and E2E TRX results into a unified JSON object:
# {
#   "postman": { "Auth": { "total": N, "passed": N, "failed": N }, ... },
#   "e2e": { "total": N, "passed": N, "failed": N },
#   "summary": { "total": N, "passed": N, "failed": N, "all_passing": bool }
# }

set -euo pipefail

REPO_ROOT="${1:-$(pwd)}"
REPORTS_DIR="$REPO_ROOT/Reports/Test"

POSTMAN_COLLECTIONS=("Auth" "Profiles" "ArticlesEmpty" "Article" "FeedAndArticles")

# Start building JSON
postman_json="{"
total_all=0
passed_all=0
failed_all=0
first=true

for collection in "${POSTMAN_COLLECTIONS[@]}"; do
  report_file="$REPORTS_DIR/Postman/$collection/Results/newman-report.json"

  if [[ -f "$report_file" ]]; then
    total=$(jq '.run.stats.assertions.total // 0' "$report_file")
    failed=$(jq '.run.stats.assertions.failed // 0' "$report_file")
    passed=$((total - failed))
  else
    total=0
    passed=0
    failed=0
  fi

  total_all=$((total_all + total))
  passed_all=$((passed_all + passed))
  failed_all=$((failed_all + failed))

  if [[ "$first" == "true" ]]; then
    first=false
  else
    postman_json+=","
  fi
  postman_json+="\"$collection\":{\"total\":$total,\"passed\":$passed,\"failed\":$failed}"
done
postman_json+="}"

# Parse E2E TRX results
e2e_trx="$REPORTS_DIR/e2e/Results/e2e-results.trx"
if [[ -f "$e2e_trx" ]]; then
  # TRX is XML — extract Counters from ResultSummary
  e2e_total=$(xmllint --xpath 'string(//*[local-name()="Counters"]/@total)' "$e2e_trx" 2>/dev/null || echo "0")
  e2e_passed=$(xmllint --xpath 'string(//*[local-name()="Counters"]/@passed)' "$e2e_trx" 2>/dev/null || echo "0")
  e2e_failed=$(xmllint --xpath 'string(//*[local-name()="Counters"]/@failed)' "$e2e_trx" 2>/dev/null || echo "0")

  # Handle empty strings
  e2e_total=${e2e_total:-0}
  e2e_passed=${e2e_passed:-0}
  e2e_failed=${e2e_failed:-0}
else
  e2e_total=0
  e2e_passed=0
  e2e_failed=0
fi

total_all=$((total_all + e2e_total))
passed_all=$((passed_all + e2e_passed))
failed_all=$((failed_all + e2e_failed))

if [[ "$failed_all" -eq 0 && "$total_all" -gt 0 ]]; then
  all_passing="true"
else
  all_passing="false"
fi

# Output unified JSON
cat <<EOF
{
  "postman": $postman_json,
  "e2e": {"total": $e2e_total, "passed": $e2e_passed, "failed": $e2e_failed},
  "summary": {"total": $total_all, "passed": $passed_all, "failed": $failed_all, "all_passing": $all_passing}
}
EOF
