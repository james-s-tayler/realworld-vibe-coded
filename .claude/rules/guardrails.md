---
paths: []
---

## Guardrail Enforcement Hierarchy

When adding a guardrail to prevent a class of defect, choose the earliest enforcement point:

| Rank | Stage | Mechanism | Feedback loop |
|------|-------|-----------|---------------|
| 1 | **Build** | Compiler errors, Roslyn analyzers (Error), TS `strict` | Immediate — code won't compile |
| 2 | **Lint** | ESLint custom rules, stylelint plugins, `dotnet format` | Pre-commit hook or CI lint job |
| 3 | **Test** | ArchUnit-style tests, convention tests | CI test job |
| 4 | **Guidance** | `.claude/rules/` documentation | AI-assisted only |

### Principles

- Prefer automated over manual — a rule that fails the build can't be ignored
- A single lint rule replaces infinite code review comments
- Guidance (rank 4) is a last resort for things that can't be mechanically checked
- When adding a guardrail, fix all existing violations in the same PR

### Existing Guardrails

**Backend (Roslyn analyzers — rank 1):**
- FF001: Feature flag names must use `FeatureFlags.*` constants
- FF002: Inject `IFeatureFlagService` not `IFeatureManager`
- SRV007: Use FastEndpoints test extensions not raw `HttpClient`
- I18N001/I18N002: Localization patterns in validators and handlers

**Frontend (ESLint custom rules — rank 2):**
- CBN001–CBN005: Carbon component usage patterns
- ARCH001–ARCH003: Architectural boundaries (context, generated imports, API isolation)
- TST001: No `userEvent.type()` in tests (CI flakiness)
- `i18next/no-literal-string`: No hardcoded UI strings
- `carbon/layout-use`, `carbon/theme-use`: Design token enforcement (stylelint)

**Guidance (`.claude/rules/` — rank 4):**
- `frontend-components.md`: Hook patterns, Carbon component API
- `testing.md`: E2E test conventions
- `backend.md`: Endpoint, CQRS, persistence patterns
