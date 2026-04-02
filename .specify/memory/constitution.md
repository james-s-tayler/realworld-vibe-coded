# Project Constitution

## Project Identity

A multi-tenant B2B SaaS application.

- **Backend**: .NET 10 / FastEndpoints / MediatR CQRS / EF Core + SQLite / Serilog
- **Frontend**: React + Vite + TypeScript / Carbon Design System
- **Build**: Nuke build system (`./build.sh`) — never invoke `dotnet` directly
- **Tests**: xUnit (backend), Vitest (frontend), Playwright E2E

## Authoritative Sources

Specs live in two layers — **do not duplicate** information between them:

| Source | Scope | What it contains |
|--------|-------|-----------------|
| `CLAUDE.md` | Global | Invariants, build workflow, rules index |
| `.claude/rules/*.md` | Scoped by path | Implementation patterns, templates, conventions |
| `.specify/specs/` | Per-feature | Feature requirements, acceptance criteria, plans |

SpecKit specs define **what** to build. `.claude/rules/` defines **how** to build it.

## Core Principles

### I. Build System Authority

All compilation, testing, and local runs go through `./build.sh <target> --agent`. Never run `dotnet`, `npm`, or other tool CLIs directly. The Nuke build system is the single entry point.

### II. Tests Gate Progress

Every feature must have its E2E tests passing before moving to the next feature. No exceptions. Progressive test tiers (`TestE2e{Area}`) provide faster feedback during development; `TestE2e` is the full gate.

### III. Compiler Warnings Are Errors

All compiler warnings and errors must be resolved. Never suppress or ignore them. Roslyn analyzers enforce architectural invariants (see `.claude/rules/backend-analyzers.md`).

### IV. Backend-First Development

Backend changes come first. The frontend API client is generated via Kiota codegen (`./build.sh BuildGenerateApiClient`). Never reference fields in frontend code that don't exist in the generated types.

### V. Simplicity

Start simple. No over-engineering, no premature abstractions. Only make changes that are directly requested or clearly necessary. Three similar lines of code is better than a premature abstraction.

## Quality Standards

- **Accessibility**: ARIA-first selectors in tests and components; accessible by design
- **Internationalization**: All user-facing strings go through i18n (react-i18next frontend, FluentValidation auto-translate backend)
- **Observability**: OpenTelemetry tracing, Prometheus metrics, Serilog structured logging, Audit.NET for audit trails
- **Security**: No command injection, XSS, SQL injection, or OWASP top-10 vulnerabilities

## Governance

This constitution establishes principles. For implementation details, defer to:
- `.claude/rules/` — scoped patterns and templates (auto-loaded by path)
- `CLAUDE.md` — global invariants and rules index

Amendments to this constitution require updating both this file and any referenced sources to maintain consistency.

**Version**: 1.0.0 | **Ratified**: 2026-04-03
