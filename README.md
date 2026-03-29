# RealWorld Spec Challenge — Starter Template

> **This is a coding challenge.** Use this multi-tenant starter template as your starting point and implement the full [RealWorld](https://docs.realworld.show/introduction/) spec. Your goal: **get all the tests passing.**

## The Challenge

The [RealWorld spec](https://docs.realworld.show/introduction/) defines a fully-featured blogging platform (a Medium clone) called "Conduit" — with users, articles, comments, tags, favorites, and follow feeds. This starter template gives you a production-grade foundation (Clean Architecture, CQRS, multi-tenancy, auth) with **all tests already included**. Your job is to implement the missing features until every test goes green.

### What's Included

- A working multi-tenant .NET backend with authentication, user registration, and profiles
- A React + TypeScript frontend with login, registration, settings, and profile pages
- **Postman API contract tests** covering the full RealWorld API spec (Auth, Profiles, Articles, Feed)
- **Playwright E2E tests** covering the full UI (articles, editor, home feed, comments, favorites)
- A Nuke build system that runs everything through a single CLI

### What You Need to Implement

The tests define the target. Run them, read the failures, and implement what's missing:

- **Articles** — CRUD, slugs, tags, favorites, feed
- **Comments** — create, list, delete on articles
- **Tags** — list popular tags
- **Feed** — personalized feed of followed authors' articles
- **Frontend pages** — article view, editor, home page with feed/tags

### Running the Test Suites

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download), [Nuke](https://nuke.build/) (`dotnet tool install Nuke.GlobalTool --global`), [Node.js 20+](https://nodejs.org/), [Docker](https://www.docker.com/)

#### Postman API Tests (Newman)

Each collection tests a specific API domain. Run them individually:

```bash
nuke TestServerPostmanAuth              # Auth endpoints (register, login, current user)
nuke TestServerPostmanProfiles          # Profile endpoints (get, follow, unfollow)
nuke TestServerPostmanArticlesEmpty     # Article list/feed when empty
nuke TestServerPostmanArticle           # Full article CRUD (create, read, update, delete, favorite, comments, tags)
nuke TestServerPostmanFeedAndArticles   # Feed + article listing with multiple users
```

#### Playwright E2E Tests

```bash
nuke TestE2e                            # Full Playwright E2E suite (all pages)
```

#### All Backend Tests

```bash
nuke TestServer                         # xUnit backend unit/integration tests
```

### Approach

1. Start by running `nuke TestServerPostmanAuth` — those should pass already
2. Move on to `nuke TestServerPostmanProfiles` — those should also pass
3. Tackle `nuke TestServerPostmanArticlesEmpty` next — this will require implementing article endpoints
4. Work through `nuke TestServerPostmanArticle` and `nuke TestServerPostmanFeedAndArticles`
5. Run `nuke TestE2e` once the API is solid — the frontend tests exercise the full stack

### Starting with Claude Code

Open this repo in Claude Code and paste this prompt:

> Read CLAUDE.md and follow the Session Start instructions. Then implement the RealWorld spec by following the story order in the active exec plan exactly. For each story: implement the feature, run the specified test command, fix any failures, then commit. Track progress in PROGRESS.md. If stuck on a single test for more than 20 minutes, commit what you have, note the blocker in PROGRESS.md, and move to the next story. The win condition is the `nuke TestE2e` must be passing and cannot be modified.

Good luck!

---

# Agent-First Multi-Tenant Starter Template

A production-ready .NET + React starter template built for agent-first development. Includes Clean Architecture, CQRS, multi-tenancy via Finbuckle, Nuke build targets invocable directly via `./build.sh <Target>`, 32 custom Roslyn analyzers, and a 4-layer test suite — so AI coding agents can build, test, lint, and deploy through a single entry point.

## 📦 What You Get

- 🧱 **Clean Architecture** backend (.NET 10, FastEndpoints, MediatR CQRS, EF Core + SQL Server)
- ⚛️ **React + TypeScript** frontend (Vite, Carbon Design System)
- 🏢 **Multi-tenancy** via Finbuckle — claim-based tenant resolution, per-tenant data isolation, separate tenant store database
- 🔐 **Role-based authentication** with JWT and ASP.NET Identity (multi-tenant aware)
- 🔄 **Auto-generated TypeScript API client** via Kiota — backend endpoint changes automatically sync to frontend
- 🛡️ **32 custom Roslyn analyzers** enforcing architecture, persistence, and testing rules at compile time
- 🤖 **Nuke build targets** invocable directly via `./build.sh <Target>` — agents invoke build/test/lint/deploy by name
- 🧪 **4-layer test suite** — xUnit (backend), Vitest (frontend), Playwright (E2E), Postman (API)
- ⚙️ **Nuke build system** — single `nuke` entry point for all operations
- 🐳 **Docker support** for local dev, testing, and publishing
- 🚀 **GitHub Actions CI/CD** pipeline

## 🧰 Tech Stack

| Layer | Technologies |
|:------|:-------------|
| **Backend** | .NET 10, FastEndpoints, MediatR (CQRS), FluentValidation, EF Core + SQL Server, Finbuckle.MultiTenant, Audit.NET, Microsoft.FeatureManagement, Serilog |
| **Frontend** | React 19, Vite, TypeScript, Carbon Design System |
| **Testing** | xUnit, Vitest, Playwright, Postman/Newman |
| **Build** | Nuke Build, GitHub Actions |
| **Infrastructure** | Docker, GitHub Actions CI/CD |

## 🏛️ Architecture

The backend follows [Ardalis Clean Architecture](https://github.com/ardalis/CleanArchitecture) with CQRS via MediatR:

```
Server.Core              — Domain models, aggregates, value objects, TenantInfo
Server.SharedKernel      — Common types and shared abstractions
Server.UseCases          — MediatR command/query handlers (business logic)
Server.Infrastructure    — EF Core DbContext, repositories, persistence gateways
Server.Web               — FastEndpoints endpoints, DTOs, mappers, middleware
Server.Web.DevOnly       — Development-only endpoints (seeding, diagnostics)
Server.Analyzers         — 32 custom Roslyn analyzers (SRV + PV series)
```

Endpoints are thin: bind request → authorize → delegate to MediatR → map response. Business rules live in handlers. Persistence is abstracted behind repository interfaces.

**Cross-cutting concerns:**
- **Serilog** — structured logging with enrichers, file sinks, and Seq integration
- **Audit.NET** — automatic audit trails for EF Core entity changes and Identity operations
- **Microsoft.FeatureManagement** — feature flags for toggling functionality at runtime

### 🏢 Multi-Tenancy

Multi-tenancy is implemented using [Finbuckle.MultiTenant](https://www.finbuckle.com/MultiTenant):

- **Tenant Resolution:** Claim-based strategy — the tenant identifier is extracted from the authenticated user's JWT claims (`__tenant__` claim)
- **Tenant Store:** Separate `TenantStoreDbContext` backed by its own SQL Server database, storing tenant metadata (id, identifier, name)
- **Data Isolation:** `AppDbContext` extends `MultiTenantIdentityDbContext` — all entity queries are automatically scoped to the current tenant. Entities are annotated with `.IsMultiTenant()` in EF Core configuration
- **Identity:** ASP.NET Identity is tenant-aware via `Finbuckle.MultiTenant.Identity.EntityFrameworkCore`, so users, roles, and claims are isolated per tenant
- **Tenant Aggregate:** `TenantInfo` is a domain aggregate root in `Server.Core`, keeping tenant concerns within the domain model

**Two databases:**
1. 🗃️ **Tenant Store** — shared database holding tenant registration (which tenants exist)
2. 📊 **App Database** — tenant-scoped database with all application data filtered by tenant

## 📁 Project Structure

```
App/
  Client/                — React + Vite + TypeScript frontend
  Server/                — .NET backend (Clean Architecture)
    analyzers/           — Custom Roslyn analyzers
    src/                 — Application source (Core, UseCases, Infrastructure, Web)
    tests/               — xUnit test projects
Task/
  Runner/                — Nuke build system
  LocalDev/              — Docker Compose for local development
Test/
  e2e/                   — Playwright E2E tests
  Postman/               — Postman/Newman API tests
  Migrations/            — Migration verification tests
Infra/                   — Infrastructure as Code (placeholder)
Logs/                    — Serilog + Audit.NET logs (per run mode)
Reports/                 — Test reports generated by Nuke targets
Docs/                    — Project documentation
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Nuke global tool](https://nuke.build/) — `dotnet tool install Nuke.GlobalTool --global`
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/)

Once Nuke is installed, run `nuke --help` to display all available targets.

### Build & Run

```bash
# Build, start dependencies, and run the app locally (handles everything via DependsOn)
nuke RunLocalPublish
```

### 🧪 Run Tests

```bash
nuke TestServer                         # Backend xUnit tests
nuke TestClient                         # Frontend Vitest tests
nuke TestE2e                            # Playwright E2E tests
nuke TestServerPostmanAuth              # Postman Auth API tests
nuke TestServerPostmanProfiles          # Postman Profiles API tests
nuke TestServerPostmanArticlesEmpty     # Postman Articles (empty state) tests
nuke TestServerPostmanArticle           # Postman Article CRUD tests
nuke TestServerPostmanFeedAndArticles   # Postman Feed + Articles tests
```

### 🔍 Lint

```bash
nuke LintAllVerify           # Check all linting (CI mode)
nuke LintAllFix              # Auto-fix lint issues
```

## ⚙️ Nuke Build System

All operations go through `nuke <Target>`. No need to run `dotnet`, `npm`, or `docker` commands directly. Run `nuke --help` to see all available targets.

| Category | Targets |
|:---------|:--------|
| 🔨 **Build** | `BuildServer`, `BuildClient`, `BuildServerPublish`, `BuildGenerateApiClient` |
| 🧪 **Test** | `TestServer`, `TestClient`, `TestE2e`, `TestServerPostmanAuth`, `TestServerPostmanProfiles`, `TestServerPostmanArticlesEmpty`, `TestServerPostmanArticle`, `TestServerPostmanFeedAndArticles` |
| 🔍 **Lint** | `LintAllVerify`, `LintAllFix`, `LintServerVerify`, `LintClientVerify`, `LintApiClientVerify` |
| 🗄️ **Database** | `DbMigrationsAdd`, `DbMigrationsVerifyApply`, `DbMigrationsGenerateIdempotentScript`, `DbReset` |
| ▶️ **Run** | `RunLocalPublish`, `RunLocalDependencies`, `RunLocalClient` |
| 📥 **Install** | `InstallClient`, `InstallGitHooks`, `InstallDockerNetwork` |
| ✅ **Verify** | `nuke-verify` skill (orchestrates lint + build + test sequentially) |

Database migration targets accept a `--db-context` parameter to target either the app database or the tenant store database.

When a target fails, read the error messages carefully — they include specific guidance on where to find logs and reports.

## 🤖 Agent-First Workflow

This template is designed to be operated by AI coding agents. Every layer of the development workflow is exposed in a way that agents can discover and invoke.

### 📂 Semantic Folder Structure

The repository's top-level directories provide clean semantic separation that agents can navigate without guesswork:

- **`App/`** — all application source code (frontend and backend)
- **`Task/`** — build system and local dev tooling (Nuke, Docker Compose)
- **`Test/`** — integration and contract tests (Playwright, Postman, migration verification)
- **`Infra/`** — infrastructure as code (placeholder)
- **`Logs/`** — runtime logs (Serilog, Audit.NET)
- **`Reports/`** — generated test reports

Each directory has a single, clear responsibility. An agent can immediately locate where application code lives (`App/`), how to build it (`Task/`), how to test it (`Test/`), and where to find runtime output (`Logs/`, `Reports/`). No ambiguity, no overlapping concerns.

### 📋 CLAUDE.md — Project Instructions

The `CLAUDE.md` file at the repository root is a ~40-line map that points agents to the right files via progressive disclosure: invariants and conventions inline, everything else in `Docs/` (architecture, workflow, exec plans). This is the entry point for any agent working on the codebase.

### 🎯 Nuke Build Targets

All Nuke build targets are invocable directly via `./build.sh <Target>`. Run `nuke --help` to see available targets. No skill wrapper files needed — agents call targets directly:

- `./build.sh BuildServer` — compile the .NET backend
- `./build.sh TestE2e` — run Playwright end-to-end tests
- `./build.sh LintAllFix` — auto-fix all lint issues
- `./build.sh DbMigrationsAdd` — add a new EF Core migration (supports `--db-context` for tenant store)

The `.claude/skills/nuke-verify/` skill orchestrates full pre-commit verification (lint + build + test) sequentially.

### 📏 `.claude/rules/` — Coding Rules

Domain-specific coding rules for agents organized by area:

- `backend.md` — endpoint patterns, CQRS conventions, persistence rules, error handling
- `frontend.md` — React/TypeScript conventions, Carbon Design System usage
- `e2e.md` — Playwright Page Object Model, ARIA selectors, assertion patterns
- `functional-tests.md` — FastEndpoints test extensions, test data patterns
- `cicd.md` — GitHub Actions conventions, job naming

### 🛡️ Compile-Time Guardrails — Roslyn Analyzers + FastEndpoints Framework

The backend is built on a custom framework layered on top of FastEndpoints, `Result<T>`, and MediatR pipeline behaviors. This framework constrains generated code to be clean and correct by default — and 32 custom Roslyn analyzers enforce it at compile time.

**The framework:**
- **`Result<T>`** — all handlers return `Result<T>` with typed status codes (Ok, NotFound, Invalid, Conflict, CriticalError, etc.) instead of throwing exceptions. This makes error handling explicit and composable
- **`ICommand<T>` / `IQuery<T>`** — CQRS interfaces that expose `T` at compile time via `IResultRequest<T>`, enabling type-safe pipeline behaviors without reflection
- **MediatR pipeline behaviors** — every request flows through `ExceptionHandlingBehavior` (catches exceptions → Result), `TransactionBehavior` (wraps commands in DB transactions, auto-commits on success, rolls back on failure), and `LoggingBehavior` (structured logging with timing)
- **`Send.ResultMapperAsync`** — FastEndpoints extension that maps `Result<T>` status to HTTP responses automatically (Ok → 200, NotFound → 404, Invalid → 422, etc.)

The result: endpoints are thin wrappers (`request → mediator → result → response`), business logic lives in handlers, and cross-cutting concerns (transactions, error handling, logging) are handled by the pipeline — all enforced at compile time by 32 custom Roslyn analyzers covering architecture boundaries, persistence patterns, endpoint conventions, and testing rules.
### 📊 Observability — Logs & Reports on Disk

Application logs and test results are written to well-known directories on disk, giving agents concrete artifacts to inspect when something goes wrong:

- **`Logs/`** — Serilog structured logs and Audit.NET audit logs, organized by run mode
- **`Reports/`** — HTML test reports from xUnit, Vitest, and Playwright runs

Nuke build targets emit explicit guidance on failure, pointing agents to the exact log files and reports to inspect. Claude Code hooks (`.claude/hooks/`) enforce that agents read these outputs rather than guessing at root causes — creating a closed feedback loop between running a command and diagnosing its result.

### 🔎 Playwright Trace Debugging

When Playwright E2E tests fail, traces are automatically captured to disk. The `view-playwright-traces` skill lets agents analyze these trace files to see exactly what happened — page navigations, network requests, screenshots, and DOM snapshots at each step. This means agents can self-diagnose E2E failures without human intervention: run the tests, inspect the trace, identify the issue, and fix it.

### 🔄 Kiota API Client Generation

When backend endpoints change, run `nuke BuildGenerateApiClient` to regenerate the TypeScript API client. The drift check `nuke LintApiClientVerify` catches when the generated client is out of sync. This means agents adding or modifying API endpoints get frontend bindings automatically — no manual sync required.

## 🧪 Testing

| Layer | Framework | Location | Purpose |
|:------|:----------|:---------|:--------|
| **Unit/Integration** | xUnit | `App/Server/tests/` | Backend business logic, handlers, persistence |
| **Component** | Vitest + Testing Library | `App/Client/` | Frontend components, hooks, utilities |
| **E2E** | Playwright | `Test/e2e/` | Full user flows through the browser |
| **API** | Postman/Newman | `Test/Postman/` | API contract validation |

All test results are output to `Reports/` with HTML reports generated by Nuke.

## 🚢 Deployment

```bash
# Build a publishable artifact (backend + frontend bundled)
nuke BuildServerPublish
```

The `Infra/` directory is a placeholder for infrastructure-as-code definitions.

## ✨ Using This Template

1. Clone this branch as your starting point
2. Rename the solution/projects to match your domain
3. Replace the Conduit domain models in `Server.Core` with your own (keep `TenantInfo` aggregate)
4. Update MediatR handlers in `Server.UseCases`
5. Add your endpoints in `Server.Web`
6. Configure your tenants in the tenant store database
7. The Roslyn analyzers, build system, multi-tenancy infrastructure, test suite, and agent workflow carry over automatically
