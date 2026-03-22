# Conduit — RealWorld Vibe-Coded

Conduit is a social blogging site (Medium.com clone) with a custom API for all requests including authentication.

## Tech Stack

- **Backend:** .NET 10, FastEndpoints, MediatR (CQRS), FluentValidation, EF Core + SQLite, Serilog, xUnit
- **Frontend:** React + Vite + TypeScript, Carbon Design System
- **Testing:** xUnit (backend), Vitest + Testing Library (frontend), Playwright (E2E), Postman (API)
- **Build:** Nuke build system (`./build.sh`), GitHub Actions CI/CD
- **Infra:** Docker, Bicep (Azure), Azure App Service

## Folder Structure

- `/App/Client` — React-Vite-TypeScript frontend
- `/App/Server` — .NET backend (Ardalis Clean Architecture, no Aspire)
- `/Infra` — Bicep Azure IaC
- `/Logs` — Serilog and Audit.NET logs for debugging (subdirectories per run mode)
- `/Test` — Playwright, Postman, and performance tests
- `/Task/Runner` — Nuke build system
- `/Task/LocalDev` — Docker Compose for local dev
- `/Reports` — Test reports from Nuke test targets

## Build & Test Commands

All commands use Nuke via `./build.sh`:

| Target | Purpose |
|:-------|:--------|
| `LintAllVerify` | Check all linting (run before commit) |
| `LintAllFix` | Auto-fix lint issues |
| `BuildServer` | Build .NET backend |
| `BuildClient` | Build React frontend |
| `TestServer` | Run backend xUnit tests |
| `TestClient` | Run frontend Vitest tests |
| `TestE2e` | Run Playwright E2E tests |
| `TestServerPostmanAll` | Run all Postman API tests |
| `RunLocalHotReload` | Start local dev server with hot-reload |
| `RunLocalPublish` | Start local dev server from published artifact |
| `DbMigrations*` | Database migration targets |

When Nuke build targets fail, **carefully read the error messages** — they contain specific guidance on how to access logs and reports.

## Critical Rules

1. **NEVER run `dotnet` commands directly.** Always use `./build.sh <target>`.
2. **NEVER suppress warnings or errors** in code unless explicitly instructed.
3. **NEVER use magic strings.** Use configurable values, enums, constants, or reflection (exception: SQL or UI text).
4. **NEVER write XML documentation comments.** Only add if explicitly asked.
5. **NEVER add comments to code** unless the logic is inherently unclear. Preserve existing comments.
6. **NEVER use python, perl, awk, sed, or regex for mass refactoring.** Only make direct, manual edits.
7. **NEVER modify Roslyn analyzers or ArchUnit rules** unless explicitly instructed.
8. **NEVER add or update documentation** unless explicitly asked.
9. **If modifying the Nuke build, build it first** before committing.
10. **Check Postman tests are passing** before finishing API work.

## Task Tracking

- Read `TODO.md` at the start of every session to understand current project tasks.
- Update `TODO.md` when tasks are completed, started, discovered, or blocked.
- Move completed tasks to the "Done" section with a date stamp (e.g. `- [x] 2026-03-21 Description`).
- Keep the file concise — periodically archive old done items by removing them.
