---
paths:
  - App/Server/**
  - App/Client/**
---

## Feature Flags

### v2 Schema Format

Feature flags use the Microsoft Feature Management v2 schema (`feature_management.feature_flags` array):

```json
{
  "feature_management": {
    "feature_flags": [
      { "id": "MyFeature", "enabled": false }
    ]
  }
}
```

The vendored schema is at `App/Server/schemas/FeatureManagement.v2.0.0.schema.json`. `LintAppSettingsVerify` validates all `appsettings*.json` files against it.

### Constants

Feature flag names live in `App/Server/src/Server.SharedKernel/FeatureFlags/FeatureFlags.cs`.
Add new flags as `public const string` fields.

Flags exposed to the frontend must be listed in `FeatureFlags.ClientVisible` array.

### When to Use Feature Flags

- **Any change that can be feature flagged must be feature flagged.** New endpoints, behavior changes, and UI features should be gated behind a flag so they can be toggled without redeployment.
- When adding a feature-flagged change, **tests must cover both the enabled and disabled states** — verify the feature works when on and confirm the old behavior is preserved when off.

### Conventions

- New flags must be added to **three** appsettings files:
  - `appsettings.json` — default `false` (or with targeting filter)
  - `appsettings.Testing.json` — default `false` (or with targeting filter)
  - `appsettings.Development.json` — default `true` (E2E tests and local dev)
- Never use magic strings — always reference `FeatureFlags.*` constants (FF001 analyzer enforces this as a build error)
- Never inject `IFeatureManager` directly — use `IFeatureFlagService` (FF002 analyzer enforces this as a build error)
- Flags are in the `feature_management.feature_flags` array (v2 format), alphabetically ordered by `id`

### Roslyn Analyzers

| ID | Severity | Rule |
|----|----------|------|
| FF001 | Error | Feature flag names must use `FeatureFlags.*` constants, not string literals |
| FF002 | Error | Inject `IFeatureFlagService` instead of `IFeatureManager`/`IVariantFeatureManager` — exempts `FeatureFlagService` itself and test assemblies |

### Azure App Configuration

- In production, set `AzureAppConfiguration:ConnectionString` to enable Azure App Configuration with feature flag refresh middleware
- Locally, flags are read from `appsettings.json` — no connection string needed

### Public Endpoint

`GET /api/feature-flags` — returns client-visible flags in v2 format (requires authentication). Only flags in `FeatureFlags.ClientVisible` are exposed. Feature flags are only available on authenticated pages.

### Checking Flags in Backend Code

```csharp
var isEnabled = await featureFlagService.IsEnabledAsync(FeatureFlags.SampleFeature);
```
