# Conduit — RealWorld Vibe-Coded

## Invariants

These are the primary rules.

1. **NEVER run `dotnet` commands directly.** Always use `./build.sh <target> --agent`. The `--agent` flag suppresses verbose Docker output for context efficiency. Run `nuke --help` to see what targets are available.
2. All nuke Test* and RunLocal* targets record Serilog and Audit.NET logs in the Logs/ folder and.
3. All Test* Nuke targets record comprehensive reports in the Reports/ directory. Additionally TestE2e* nuke targets record playwright traces that can be viewed with /view-playwright-traces
4. **Every feature must have its Postman and E2E tests passing before moving to the next feature.** The implementation workflow enforces this.
5. **All compiler warnings and errors must be resolved.** Never suppress or ignore them.
8. **Frontend API client relies on Kiota code generation.** Always make backend changes first, then run `./build.sh BuildGenerateApiClient` before writing frontend code. `BuildClient` does this automatically (chain: `BuildClient → BuildGenerateApiClient → BuildServer`). Never reference fields in frontend that don't exist in the generated types.
9. **Every test failure must be investigated.** Never dismiss failures as "pre-existing", "unrelated", or "flaky" without investigation. Read the logs, check the traces, understand the root cause. If the failure is genuinely outside the scope of current work, file it as a known issue with evidence — but never ignore it.
10. **Never guess the app URL.** Worktrees use port offsets and the Vite dev server uses HTTPS (via `basicSsl` plugin). Never construct the URL from `ss`/`lsof`/`docker port` — always read the Nuke build output which logs the correct `https://` URL with the right port. If the build ran in background, read the task output file.
11. **Verify frontend changes visually.** After implementing UI changes, run `./build.sh RunLocalHotReload --agent` to spin up the app, then use Chrome DevTools MCP tools to navigate, take screenshots, and independently verify the work looks and functions correctly before considering it done.
12. **RunLocal\* lifecycle.** Always tear down before spinning up: run the matching `*Down` target (e.g., `RunLocalHotReloadDown`, `RunLocalPublishDown`) before starting a new `RunLocal*` target — port conflicts silently break things. Always run `RunLocal*` targets in background mode (`run_in_background: true`) so they don't block the conversation.

Reference as needed:
- `SPEC-REFERENCE.md` — complete API spec (the source of truth for what to build)
- `.specify/specs/` — per-feature specifications, plans, and acceptance criteria
- `Docs/System/Architecture/architecture.md` — tech stack, folder structure, build commands
- `Docs/System/Architecture/observability.md` — OpenTelemetry tracing/metrics, Grafana/Jaeger/Prometheus stack
- Chrome DevTools MCP — visual browser inspection via `/browser-inspect` and debugging via `/browser-debug`. Use to verify UI changes with screenshots.
- Docs MCP server — search/scrape indexed library documentation before planning

## Rules Index

Rules in `.claude/rules/` are loaded automatically by path scope. Read the relevant files before starting work.

| File | Scope | Contents |
|------|-------|----------|
| `backend.md` | `App/Server/**` | Endpoint, CQRS, persistence, validation, error handling patterns |
| `backend-analyzers.md` | `App/Server/**` | Roslyn analyzers that enforce architectural invariants, Result→HTTP mapping |
| `backend-templates-endpoint.md` | `App/Server/**` | Copy-paste: Endpoint, Request/Response DTOs |
| `backend-templates-commands.md` | `App/Server/**` | Copy-paste: MediatR Command + Handler |
| `backend-templates-queries.md` | `App/Server/**` | Copy-paste: MediatR Query + Handler, FluentValidation |
| `backend-templates-persistence.md` | `App/Server/**` | Copy-paste: EF Core config, ResponseMapper |
| `frontend.md` | `App/Client/**` | Kiota bridge workflow, project structure, routing, state |
| `frontend-components.md` | `App/Client/**` | Hooks, Carbon components, CSS classes, API module template |
| `feature-flags.md` | `App/Server/**`, `App/Client/**` | FeatureFlags constants, FF001/FF002 analyzers, Azure App Configuration |
| `e2e.md` | `Test/e2e/**` | Playwright conventions, ARIA selectors, Expect() only |
| `functional-tests.md` | `App/Server/tests/**` | FastEndpoints test extensions (SRV007), AppFixture |
| `testing.md` | — | E2E test structure overview, progressive tier targets |
| `cicd.md` | `.github/**` | GitHub Actions naming, path-based job gating |
| `nuke.md` | `Task/Runner/**` | Worktree port isolation, Vite env var conventions |
| `research.md` | — | Docs MCP research-first workflow for planning |
| `harness.md` | `.claude/**`, `scripts/**` | Hook conventions, settings.json, marker files, protected files |
