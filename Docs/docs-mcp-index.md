# Docs MCP Server — Libraries to Index

Index these libraries into the `@arabold/docs-mcp-server`. The **Scope** column indicates the crawl boundary needed to get full docs (`subpages` is the default — only listed when a wider scope is required).

## Backend — Web & API

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| FastEndpoints | 7.0.1 | https://fast-endpoints.com/docs | subpages | Primary API framework — endpoints, validation, Swagger, testing |
| MediatR | 12.5.0 | https://github.com/jbogard/MediatR/wiki | subpages | CQRS mediator — pipeline behaviors, notifications |
| Ardalis.Specification | 9.2.0 | https://specification.ardalis.com | subpages | Specification pattern for EF Core queries |
| Ardalis.GuardClauses | 5.0.0 | https://github.com/ardalis/GuardClauses | subpages | Pre-condition guard helpers |
| FluentValidation | (via FastEndpoints) | https://docs.fluentvalidation.net | subpages | Request validation rules |

## Backend — Data & Persistence

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| Finbuckle.MultiTenant | 10.0.1 | https://www.finbuckle.com/multitenant/docs/v10.0 | hostname | Multi-tenancy — stores, strategies, EF Core integration |
| Audit.NET | 31.3.1 | https://github.com/thepirat000/Audit.NET | subpages | EF Core audit trail, identity auditing |

## Backend — Observability

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| Serilog | 9.0.0 | https://github.com/serilog/serilog/wiki | subpages | Structured logging — sinks, enrichers, expressions |
| OpenTelemetry .NET | 1.15 | https://opentelemetry.io/docs/languages/dotnet/ | subpages | Tracing, metrics, OTLP export |
| Seq | latest | https://docs.datalust.co/docs | hostname | Log aggregation UI, query language |

## Backend — Testing

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| xUnit v3 | 3.1.0 | https://xunit.net/?tabs=cs | subpages | Test framework |
| Shouldly | 4.3.0 | https://docs.shouldly.org | subpages | Fluent assertions |
| NSubstitute | 5.3.0 | https://nsubstitute.github.io/help | subpages | Mocking library |
| Testcontainers | 4.2.0 | https://dotnet.testcontainers.org | subpages | Docker-based test infrastructure |
| Playwright (.NET) | 1.55.0 | https://playwright.dev/dotnet/docs/api/class-playwright | hostname | E2E browser testing |

## Frontend — Core

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| React | 19.1.1 | https://react.dev/reference/react | subpages | UI framework |
| React Router | 7.9.3 | https://reactrouter.com/start/framework/routing | hostname | Client-side routing |
| Carbon Design System | 1.92.1 | https://carbondesignsystem.com/developing/frameworks/react | hostname | UI component library |
| Vite | 7.1.7 | https://vite.dev/guide/ | subpages | Build tool and dev server |

## Frontend — API & State

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| @microsoft/feature-management | 2.3.0 | https://github.com/microsoft/FeatureManagement-JavaScript | subpages | Frontend feature flags |

## Frontend — i18n

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| i18next | 26.0.1 | https://www.i18next.com/overview/getting-started | hostname | Internationalization framework |
| react-i18next | 17.0.1 | https://react.i18next.com | subpages | React bindings for i18next |

## Frontend — Testing

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| Vitest | 3.2.4 | https://vitest.dev/guide/ | subpages | Unit test framework |
| Testing Library (React) | 16.3.0 | https://testing-library.com/docs/ | hostname | Component testing utilities |

## DevOps & Build

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| Nuke Build | 10.1.0 | https://nuke.build/docs/introduction/ | hostname | Build automation framework |
| Docker Compose | 3.x | https://docs.docker.com/compose/ | subpages | Container orchestration |
| GitHub Actions | — | https://docs.github.com/en/actions | subpages | CI/CD platform |

## Observability Infrastructure

| Library | Version | Docs URL | Scope | Why |
|---------|---------|----------|-------|-----|
| Jaeger | latest | https://www.jaegertracing.io/docs/ | subpages | Distributed tracing backend |
| Prometheus | latest | https://prometheus.io/docs/ | hostname | Metrics collection |
| Grafana | latest | https://grafana.com/docs/grafana/latest/ | subpages | Visualization dashboards |
