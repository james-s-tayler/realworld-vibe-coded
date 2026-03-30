#!/bin/bash
# Wrapper to launch chrome-devtools-mcp with auto-detected Chrome.
# The MCP server needs a Chrome/Chromium binary. This script checks
# common locations so the settings.json config works for the whole team.

if [ -z "$CHROME_PATH" ]; then
  for candidate in \
    "$(find "$HOME/.cache/ms-playwright"/chromium-*/chrome-linux64/chrome 2>/dev/null | sort -V | tail -1)" \
    "$(which google-chrome-stable 2>/dev/null)" \
    "$(which google-chrome 2>/dev/null)" \
    "$(which chromium-browser 2>/dev/null)" \
    "$(which chromium 2>/dev/null)" \
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"; do
    if [ -n "$candidate" ] && [ -x "$candidate" ]; then
      export CHROME_PATH="$candidate"
      break
    fi
  done
fi

if [ -z "$CHROME_PATH" ]; then
  echo "No Chrome/Chromium found. Install one or set CHROME_PATH." >&2
  exit 1
fi

exec npx -y chrome-devtools-mcp@latest "$@"
