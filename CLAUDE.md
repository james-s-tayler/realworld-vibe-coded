# Conduit — RealWorld Vibe-Coded

## Invariants

These are the primary rules.

1. **NEVER run `dotnet` commands directly.** Always use `./build.sh <target> --agent`. The `--agent` flag suppresses verbose Docker output for context efficiency. Run `nuke --help` to see what targets are available.
2. All nuke Test* and RunLocal* targets record Serilog and Audit.NET logs in the Logs/ folder and.
3. All Test* Nuke targets record comprehensive reports in the Reports/ directory. Additionally TestE2e* nuke targets record playwright traces that can be viewed with /view-playwright-traces
4. **Every feature must have its Postman and E2E tests passing before moving to the next feature.** The implementation workflow enforces this.
5. **All compiler warnings and errors must be resolved.** Never suppress or ignore them.
8. **Frontend API client relies on Kiota code generation.** Always make backend changes first, then run `./build.sh BuildGenerateApiClient` before writing frontend code. `BuildClient` does this automatically (chain: `BuildClient → BuildGenerateApiClient → BuildServer`). Never reference fields in frontend that don't exist in the generated types.

Reference as needed:
- `SPEC-REFERENCE.md` — complete API spec (the source of truth for what to build)
- `.specify/specs/` — per-feature specifications, plans, and acceptance criteria
- `Docs/architecture.md` — tech stack, folder structure, build commands
- `Docs/observability.md` — OpenTelemetry tracing/metrics, Grafana/Jaeger/Prometheus stack
- Chrome DevTools MCP — visual browser inspection via `/browser-inspect` and debugging via `/browser-debug`

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
| `harness.md` | `.claude/**`, `scripts/**` | Hook conventions, settings.json, marker files, protected files |
