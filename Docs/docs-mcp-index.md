# Docs MCP Server — Libraries to Index

Review this list and remove anything you don't need. Then I'll index the approved ones.

## Backend — Web & API

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| FastEndpoints | 7.0.1 | https://fast-endpoints.com/docs | Primary API framework — endpoints, validation, Swagger, testing |
| MediatR | 12.5.0 | https://github.com/jbogard/MediatR/wiki | CQRS mediator — pipeline behaviors, notifications |
| Ardalis.Specification | 9.2.0 | https://specification.ardalis.com | Specification pattern for EF Core queries |
| Ardalis.GuardClauses | 5.0.0 | https://github.com/ardalis/GuardClauses | Pre-condition guard helpers |
| FluentValidation | (via FastEndpoints) | https://docs.fluentvalidation.net | Request validation rules |

## Backend — Data & Persistence

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| Finbuckle.MultiTenant | 10.0.1 | https://www.finbuckle.com/multitenant/docs/v10.0 | Multi-tenancy — stores, strategies, EF Core integration |
| Audit.NET | 31.3.1 | https://github.com/thepirat000/Audit.NET | EF Core audit trail, identity auditing |

## Backend — Observability

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| Serilog | 9.0.0 | https://github.com/serilog/serilog/wiki | Structured logging — sinks, enrichers, expressions |
| OpenTelemetry .NET | 1.15.x | https://opentelemetry.io/docs/languages/dotnet/ | Tracing, metrics, OTLP export |
| Seq | latest | https://docs.datalust.co/docs | Log aggregation UI, query language |

## Backend — Testing

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| xUnit v3 | 3.1.0 | https://xunit.net/?tabs=cs | Test framework |
| Shouldly | 4.3.0 | https://docs.shouldly.org | Fluent assertions |
| NSubstitute | 5.3.0 | https://nsubstitute.github.io/help | Mocking library |
| Testcontainers | 4.2.0 | https://dotnet.testcontainers.org | Docker-based test infrastructure |
| Playwright (.NET) | 1.55.0 | https://playwright.dev/dotnet/docs/intro | E2E browser testing |

## Frontend — Core

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| React | 19.1.1 | https://react.dev/reference/react | UI framework |
| React Router | 7.9.3 | https://reactrouter.com/start/framework/routing | Client-side routing |
| Carbon Design System | 1.92.1 | https://carbondesignsystem.com/developing/frameworks/react | UI component library |
| Vite | 7.1.7 | https://vite.dev/guide/ | Build tool and dev server |

## Frontend — API & State

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| @microsoft/feature-management | 2.3.0 | https://github.com/microsoft/FeatureManagement-JavaScript | Frontend feature flags |

## Frontend — i18n

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| i18next | 26.0.1 | https://www.i18next.com/overview/getting-started | Internationalization framework |
| react-i18next | 17.0.1 | https://react.i18next.com | React bindings for i18next |

## Frontend — Testing

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| Vitest | 3.2.4 | https://vitest.dev/guide/ | Unit test framework |
| Testing Library (React) | 16.3.0 | https://testing-library.com/docs/react-testing-library/intro | Component testing utilities |

## DevOps & Build

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| Nuke Build | 10.1.0 | https://nuke.build/docs/introduction/ | Build automation framework |
| Docker Compose | 3.x | https://docs.docker.com/compose/ | Container orchestration |
| GitHub Actions | — | https://docs.github.com/en/actions | CI/CD platform |

## Observability Infrastructure

| Library | Version | Docs URL | Why |
|---------|---------|----------|-----|
| Jaeger | latest | https://www.jaegertracing.io/docs/ | Distributed tracing backend |
| Prometheus | latest | https://prometheus.io/docs/ | Metrics collection |
| Grafana | latest | https://grafana.com/docs/grafana/latest/ | Visualization dashboards |
