# Conduit — RealWorld Vibe-Coded

Conduit is a social blogging site (Medium.com clone) implementing the RealWorld spec.

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

## Invariants (What Must Be True)

These are the primary rules. The Postman test suites are the spec — if a test expects something, it must be true.

1. **All API endpoints must return the exact response shapes defined in SPEC-REFERENCE.md.** The Postman tests validate response shapes — if a test expects a field, that field must exist with the correct type.
2. **All mutating endpoints must validate input and return appropriate error responses on failure.** See SPEC-REFERENCE.md for the error response format.
3. **All authenticated endpoints must return 401 when no valid JWT is present.**
4. **Every feature must have its Postman tests passing before moving to the next feature.** The implementation workflow below enforces this.
5. **All compiler warnings and errors must be resolved.** Never suppress or ignore them.
6. **The solution must build cleanly via `./build.sh BuildServer`.** Never run `dotnet` commands directly.
7. **All configurable values must use enums, constants, or reflection.** No magic strings (exception: SQL or UI text).

## Implementation Guidance

These are conventions to follow. See `.claude/rules/backend.md` for copy-pasteable code templates.

- Use FastEndpoints with `Endpoint<TRequest, TResponse, TMapper>` pattern
- Use MediatR for CQRS (commands and queries in Server.UseCases)
- Use FluentValidation `Validator<TRequest>` for request validation
- Use `Result<T>` return type from handlers (Ok, NotFound, Invalid, etc.)
- Use `Send.ResultMapperAsync` to map Result to HTTP responses
- Use `ResponseMapper<TResponse, TEntity>` for domain-to-DTO conversion
- Never write XML documentation comments
- Never add comments unless the logic is inherently unclear
- Never use python, perl, awk, sed, or regex for mass refactoring
- Never modify Roslyn analyzers unless explicitly instructed
- If modifying the Nuke build, build it first before committing

## Implementation Workflow (MANDATORY)

Follow this workflow for every feature. No exceptions.

1. Read `IMPLEMENTATION-PLAN.md` and pick the next incomplete story
2. Implement the feature (backend first, then frontend if applicable)
3. Run the story's test command (e.g., `./build.sh TestServerPostmanAuth`)
4. If tests **PASS**: commit with message `feat: implement <story-name> — tests passing`
5. If tests **FAIL**: fix and re-test. Do NOT move to the next story until this one passes.
6. Append to `PROGRESS.md`: what was implemented, test results, any gotchas discovered
7. Repeat from step 1

### Stop Condition

You are done when ALL Postman tests AND all E2E tests pass:
```
./build.sh TestServerPostmanAuth TestServerPostmanProfiles TestServerPostmanArticlesEmpty TestServerPostmanArticle TestServerPostmanFeedAndArticles TestE2e
```

## Circuit Breaker

If you are stuck on a single failing test or feature for more than 20 minutes:

1. Commit what you have (even if tests are failing for this feature)
2. Note the issue in `PROGRESS.md` under "Blocked Items"
3. Move to the next story in `IMPLEMENTATION-PLAN.md`
4. Return to blocked items only after all other stories are attempted

Breadth of feature coverage is more important than depth on any single feature.

## Context Management

If you notice your context is getting large (many files read, long conversation):

1. Commit all current progress immediately
2. Update `PROGRESS.md` with current state and next steps
3. If using compact, ensure `PROGRESS.md` and `IMPLEMENTATION-PLAN.md` are referenced in the summary
4. After compact, re-read `PROGRESS.md` and `IMPLEMENTATION-PLAN.md` to recover context

Commit-gate semantics ensure progress survives context resets. `PROGRESS.md` ensures knowledge survives.

## Progress Tracking

- Read `PROGRESS.md` at the start of every session to recover context from previous sessions.
- After each story: append what was done, which tests pass, any gotchas discovered.
- **NEVER delete or modify existing entries** — `PROGRESS.md` is append-only.
- This file survives context compression and is the primary cross-session memory.

## Implementation Plan

- Read `IMPLEMENTATION-PLAN.md` at the start of every session for the pre-planned implementation order.
- Follow the story order strictly — each story builds only on completed dependencies.
- Run the specified test command after each story to validate before moving on.
- See `SPEC-REFERENCE.md` for the complete API specification with endpoint details, schemas, and validation rules.
