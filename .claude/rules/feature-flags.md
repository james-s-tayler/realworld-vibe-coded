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
      { "id": "DashboardBanner", "enabled": false }
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
  - `appsettings.json` — default `false`
  - `appsettings.Testing.json` — default `false`
  - `appsettings.Development.json` — default `true` (E2E tests and local dev)
- Never use magic strings — always reference `FeatureFlags.*` constants (FF001 analyzer enforces this as a build error)
- Flags are in the `feature_management.feature_flags` array (v2 format), alphabetically ordered by `id`

### Azure App Configuration

- In production, set `AzureAppConfiguration:ConnectionString` to enable Azure App Configuration with feature flag refresh middleware
- Locally, flags are read from `appsettings.json` — no connection string needed

### Public Endpoint

`GET /api/feature-flags` — returns client-visible flags in v2 format (AllowAnonymous). Only flags in `FeatureFlags.ClientVisible` are exposed.

### Checking Flags in Backend Code

```csharp
// In a handler or service — inject IFeatureFlagService
var isEnabled = await featureFlagService.IsEnabledAsync(FeatureFlags.SampleFeature);

// Or inject IFeatureManager directly
var isEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SampleFeature);
```

### Frontend

Frontend constants mirror the backend in `App/Client/src/featureFlags.ts`.

The `FeatureFlagProvider` (in `App/Client/src/context/FeatureFlagContext.tsx`) fetches flags from `GET /api/feature-flags`, evaluates them via `@microsoft/feature-management` SDK, and exposes a sync `useFeatureFlag` hook:

```typescript
import { useFeatureFlag } from '../hooks/useFeatureFlag';
import { FEATURE_FLAGS } from '../featureFlags';

const showBanner = useFeatureFlag(FEATURE_FLAGS.DASHBOARD_BANNER);
```

When adding a new frontend-visible flag:
1. Add constant to `FeatureFlags.cs` and `FeatureFlags.ClientVisible`
2. Add to all three `appsettings*.json` files
3. Add constant to `featureFlags.ts`
4. Use `useFeatureFlag(FEATURE_FLAGS.FLAG_NAME)` in components
