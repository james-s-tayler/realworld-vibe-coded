---
paths: []
---

## Guardrail Enforcement Hierarchy

When adding a guardrail to prevent a class of defect, choose the earliest enforcement point:

| Rank | Stage | Mechanism | Feedback loop |
|------|-------|-----------|---------------|
| 1 | **Build** | Compiler errors, Roslyn analyzers, TS `strict` | Immediate — code won't compile |
| 2 | **Lint** | ESLint rules, stylelint plugins, `dotnet format` | Pre-commit hook or CI lint job |
| 3 | **Test** | ArchUnit-style tests, convention tests | CI test job |
| 4 | **Guidance** | `.claude/rules/` documentation | AI-assisted only |

### Principles

- Prefer automated over manual — a rule that fails the build can't be ignored
- A single lint rule replaces infinite code review comments
- Guidance (rank 4) is a last resort for things that can't be mechanically checked
- When adding a guardrail, fix all existing violations in the same PR

### Guardrail Classes

| Name | Location | Type |
|------|----------|------|
| Server Roslyn Analyzers | `App/Server/analyzers/` | Build |
| Nuke Build Analyzers | `Task/Runner/Nuke.Analyzers/` | Build |
| E2E Test Analyzers | `Test/e2e/E2eTests.Analyzers/` | Build |
| ReferenceTrimmer | `App/Server/Directory.Build.props` | Build |
| TypeScript Strict Mode | `App/Client/tsconfig.app.json` | Build |
| .editorconfig + StyleCop | `.editorconfig` | Lint |
| Nuke Lint Targets | `Task/Runner/Nuke/Build.Lint.cs` | Lint |
| ESLint Custom Rules | `App/Client/eslint-plugin-custom-rules/` | Lint |
| Stylelint Carbon Tokens | `App/Client/.stylelintrc.json` | Lint |
| Locale Parity Check | `App/Client/scripts/check-locale-parity.js` | Lint |
| Pre-commit Hook | `.husky/pre-commit` | Lint |
| ArchUnit (Nuke) | `Task/Runner/Nuke.Tests/` | Test |
| `.claude/rules/*` | `.claude/rules/` | Guidance |
