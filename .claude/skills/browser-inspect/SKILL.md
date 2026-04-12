---
description: Visual browser inspection workflow. Use to visually inspect the running app — screenshots, DOM snapshots, console messages, network requests — and fix issues in a closed loop.
---

Full browser inspection workflow using Chrome DevTools MCP tools.

## Lifecycle

1. **Start the app** (detached): `./build.sh RunLocalPublish --agent`
2. **Navigate**: `navigate_page` to `http://localhost:5000`
3. **Inspect & interact** using the tools below
4. **Fix issues** → rebuild (`./build.sh BuildServer --agent` / `./build.sh BuildClient --agent`) → restart (`./build.sh RunLocalPublish --agent`)
5. **Stop the app**: `./build.sh RunLocalPublishDown --agent`

## Available Tools

### Visual Inspection
- `take_screenshot` — capture what the page looks like right now
- `take_snapshot` — DOM snapshot (accessibility tree) for structural analysis

### Console & Network
- `list_console_messages` — see browser console output (errors, warnings, logs)
- `get_console_message` — get full details of a specific console message
- `list_network_requests` — see all HTTP requests the page made
- `get_network_request` — get full request/response details (headers, body, status)

### Interaction
- `click` — click an element by CSS selector or coordinates
- `fill` — fill an input field with text (clears existing value)
- `type_text` — type text character by character (for autocomplete, search, etc.)
- `press_key` — press a keyboard key (Enter, Tab, Escape, etc.)
- `evaluate_script` — run arbitrary JavaScript in the page context

## Authentication

If you need to inspect pages behind authentication (settings, editor, profile, etc.):

1. **Register a new account** via the Sign up page — never guess credentials for an existing account
2. **Log in** with the account you just created
3. **Then navigate** to the target page

If you navigate to an authenticated page and get redirected to `/login`, this means you need to register first.

## Workflow Pattern

```
navigate → screenshot → identify issue → fix code → rebuild → restart → screenshot → verify
```

Iterate until the page renders correctly. Cross-reference with:
- Serilog logs in `Logs/Server.Web/Serilog/` for backend errors
- Network requests for failed API calls
- Console messages for frontend errors
