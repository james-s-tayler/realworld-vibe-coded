# Architecture

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
| `RunLocalPublish` | Start local dev server from published artifact |
| `DbMigrations*` | Database migration targets |

When Nuke build targets fail, **carefully read the error messages** — they contain specific guidance on how to access logs and reports.
