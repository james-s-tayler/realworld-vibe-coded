---
description: Focused browser debugging recipes. Use to diagnose rendering bugs, failing API calls, console errors, or to manually walk through E2E test steps.
---

Focused debugging recipes using Chrome DevTools MCP tools. Requires the app to be running (`./build.sh RunLocalPublish --agent`).

## Rendering Bugs

1. `take_screenshot` — see the visual state
2. `take_snapshot` — get the DOM/accessibility tree
3. `evaluate_script` — inspect computed styles: `getComputedStyle(document.querySelector('.selector')).property`
4. Fix CSS/component code → rebuild → restart → screenshot to verify

## Failing API Calls

1. `list_network_requests` — find requests with non-2xx status codes
2. `get_network_request` — inspect request/response details (headers, body, status)
3. Cross-reference with Serilog logs in `Logs/Server.Web/Serilog/` for server-side errors
4. Fix backend code → `./build.sh BuildServer --agent` → restart → verify

## Console Errors

1. `list_console_messages` — scan for errors and warnings
2. `get_console_message` — get full stack traces
3. Fix frontend code → `./build.sh BuildClient --agent` → restart → verify

## Authentication

If you need to debug pages behind authentication, **register a new account** via the Sign up page first — never guess credentials for an existing account. If you get redirected to `/login`, register before retrying.

## Walking Through E2E Test Steps

Manually reproduce what a Playwright test does to diagnose failures:

1. `navigate_page` to the starting URL
2. For each test step: `click` / `fill` / `press_key` as the test would
3. `take_screenshot` after each step to see the state
4. `list_network_requests` to verify API calls fired correctly
5. Compare against the test's expected behavior in `Test/e2e/E2eTests/`
