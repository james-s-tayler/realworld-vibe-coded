#!/bin/bash
# create-test-trace.sh — Generate a synthetic Playwright trace zip for testing the eval pipeline
#
# Creates a realistic-looking trace with NDJSON action log, network entries,
# and fake resources. Useful for verifying extract → grade pipeline without
# running actual Playwright tests.
#
# Usage: ./scripts/evals/create-test-trace.sh <output-dir> [--scenario good|weak|bad]

set -euo pipefail

OUTPUT_DIR="${1:?Usage: create-test-trace.sh <output-dir> [--scenario good|weak|bad]}"
SCENARIO="good"

shift || true
while [[ $# -gt 0 ]]; do
  case $1 in
    --scenario) SCENARIO="$2"; shift 2 ;;
    *) echo "Unknown option: $1" >&2; exit 1 ;;
  esac
done

mkdir -p "$OUTPUT_DIR"
TMPDIR=$(mktemp -d)
trap 'rm -rf "$TMPDIR"' EXIT

mkdir -p "$TMPDIR/resources"

WALL_TIME=$(date +%s)000

# Generate trace.trace (NDJSON)
generate_trace_events() {
  local t=$WALL_TIME

  # Context options
  cat <<EOF
{"type":"context-options","options":{"viewport":{"width":1280,"height":720},"userAgent":"Mozilla/5.0 E2E-Test-Suite","ignoreHTTPSErrors":true},"wallTime":$t}
EOF

  if [[ "$SCENARIO" == "good" || "$SCENARIO" == "weak" ]]; then
    # Navigate to login
    cat <<EOF
{"type":"before","callId":"call@1","class":"Page","method":"goto","apiName":"page.goto","params":{"url":"https://localhost:5001/login"},"wallTime":$((t+100)),"startTime":$((t+100))}
{"type":"after","callId":"call@1","endTime":$((t+300))}
EOF

    # Fill email
    cat <<EOF
{"type":"before","callId":"call@2","class":"Locator","method":"fill","apiName":"locator.fill","params":{"selector":"getByPlaceholder('Email')","value":"testuser1@test.com"},"wallTime":$((t+400)),"startTime":$((t+400))}
{"type":"after","callId":"call@2","endTime":$((t+500))}
EOF

    # Fill password
    cat <<EOF
{"type":"before","callId":"call@3","class":"Locator","method":"fill","apiName":"locator.fill","params":{"selector":"getByPlaceholder('Password')","value":"TestPassword123!"},"wallTime":$((t+600)),"startTime":$((t+600))}
{"type":"after","callId":"call@3","endTime":$((t+700))}
EOF

    # Click sign in
    cat <<EOF
{"type":"before","callId":"call@4","class":"Locator","method":"click","apiName":"locator.click","params":{"selector":"getByRole('button', { name: 'Sign in' })"},"wallTime":$((t+800)),"startTime":$((t+800))}
{"type":"after","callId":"call@4","endTime":$((t+900))}
EOF

    if [[ "$SCENARIO" == "good" ]]; then
      # Expect assertion — settings link visible (strong assertion)
      cat <<EOF
{"type":"before","callId":"call@5","class":"LocatorAssertions","method":"toBeVisible","apiName":"expect(locator).toBeVisible","params":{"selector":"getByRole('link', { name: 'Settings' })"},"wallTime":$((t+1000)),"startTime":$((t+1000))}
{"type":"after","callId":"call@5","endTime":$((t+1100))}
EOF

      # Expect assertion — user profile link visible (verifies logged in as correct user)
      cat <<EOF
{"type":"before","callId":"call@6","class":"LocatorAssertions","method":"toBeVisible","apiName":"expect(locator).toBeVisible","params":{"selector":"getByRole('link', { name: 'testuser1@test.com' })"},"wallTime":$((t+1200)),"startTime":$((t+1200))}
{"type":"after","callId":"call@6","endTime":$((t+1300))}
EOF
    else
      # Weak scenario — only checks page loaded, no content verification
      cat <<EOF
{"type":"before","callId":"call@5","class":"PageAssertions","method":"toHaveURL","apiName":"expect(page).toHaveURL","params":{"url":"https://localhost:5001/"},"wallTime":$((t+1000)),"startTime":$((t+1000))}
{"type":"after","callId":"call@5","endTime":$((t+1100))}
EOF
    fi

  else
    # Bad scenario — login fails
    cat <<EOF
{"type":"before","callId":"call@1","class":"Page","method":"goto","apiName":"page.goto","params":{"url":"https://localhost:5001/login"},"wallTime":$((t+100)),"startTime":$((t+100))}
{"type":"after","callId":"call@1","endTime":$((t+300))}
EOF

    cat <<EOF
{"type":"before","callId":"call@2","class":"Locator","method":"fill","apiName":"locator.fill","params":{"selector":"getByPlaceholder('Email')","value":"testuser1@test.com"},"wallTime":$((t+400)),"startTime":$((t+400))}
{"type":"after","callId":"call@2","endTime":$((t+500))}
EOF

    cat <<EOF
{"type":"before","callId":"call@3","class":"Locator","method":"fill","apiName":"locator.fill","params":{"selector":"getByPlaceholder('Password')","value":"WrongPassword!"},"wallTime":$((t+600)),"startTime":$((t+600))}
{"type":"after","callId":"call@3","endTime":$((t+700))}
EOF

    cat <<EOF
{"type":"before","callId":"call@4","class":"Locator","method":"click","apiName":"locator.click","params":{"selector":"getByRole('button', { name: 'Sign in' })"},"wallTime":$((t+800)),"startTime":$((t+800))}
{"type":"after","callId":"call@4","endTime":$((t+900))}
EOF

    # Expect times out — assertion fails
    cat <<EOF
{"type":"before","callId":"call@5","class":"LocatorAssertions","method":"toBeVisible","apiName":"expect(locator).toBeVisible","params":{"selector":"getByRole('link', { name: 'Settings' })"},"wallTime":$((t+1000)),"startTime":$((t+1000))}
{"type":"after","callId":"call@5","error":{"message":"Timeout 10000ms exceeded. Waiting for locator('role=link[name=\"Settings\"]') to be visible"},"endTime":$((t+11000))}
EOF

    # Console error
    cat <<EOF
{"type":"console","messageType":"error","text":"POST /api/identity/login 401 Unauthorized","url":"https://localhost:5001/login","lineNumber":1,"wallTime":$((t+900))}
EOF
  fi
}

# Generate trace.network (NDJSON)
generate_network_events() {
  if [[ "$SCENARIO" == "good" || "$SCENARIO" == "weak" ]]; then
    cat <<EOF
{"request":{"method":"POST","url":"https://localhost:5001/api/identity/login?useCookies=false","headers":[{"name":"content-type","value":"application/json"}],"postData":"{\"email\":\"testuser1@test.com\",\"password\":\"TestPassword123!\"}"},"response":{"status":200,"statusText":"OK","headers":[{"name":"content-type","value":"application/json"}]},"resourceType":"fetch"}
{"request":{"method":"GET","url":"https://localhost:5001/api/user","headers":[{"name":"authorization","value":"Bearer eyJhbG..."}]},"response":{"status":200,"statusText":"OK","headers":[{"name":"content-type","value":"application/json"}]},"resourceType":"fetch"}
{"request":{"method":"GET","url":"https://localhost:5001/api/feature-flags","headers":[]},"response":{"status":200,"statusText":"OK","headers":[]},"resourceType":"fetch"}
EOF
  else
    cat <<EOF
{"request":{"method":"POST","url":"https://localhost:5001/api/identity/login?useCookies=false","headers":[{"name":"content-type","value":"application/json"}],"postData":"{\"email\":\"testuser1@test.com\",\"password\":\"WrongPassword!\"}"},"response":{"status":401,"statusText":"Unauthorized","headers":[]},"resourceType":"fetch"}
EOF
  fi
}

# Write files
generate_trace_events > "$TMPDIR/trace.trace"
generate_network_events > "$TMPDIR/trace.network"

# Create a fake DOM snapshot resource
DOM_SHA="abc123def456"
echo '<html><body><nav><a href="/settings">Settings</a><a href="/profile/testuser1@test.com">testuser1@test.com</a></nav><h1>Dashboard</h1><p>Welcome, testuser1@test.com</p></body></html>' > "$TMPDIR/resources/$DOM_SHA"

# Package as zip
TRACE_NAME="UserCanSignIn_WithExistingCredentials_trace_$(date +%Y%m%d_%H%M%S)"
(cd "$TMPDIR" && zip -q -r "$OUTPUT_DIR/${TRACE_NAME}.zip" trace.trace trace.network resources/)

echo "Created: $OUTPUT_DIR/${TRACE_NAME}.zip (scenario: $SCENARIO)" >&2
echo "$OUTPUT_DIR/${TRACE_NAME}.zip"
