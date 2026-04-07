---
paths:
  - App/Server/**
---

## Tenant-Targeted Feature Flags

Feature flags support per-tenant targeting via `TenantTargetingContextAccessor`, which sets `TargetingContext.UserId = TenantId` from Finbuckle multi-tenancy. Percentage rollouts are deterministic per-tenant — all users in the same tenant see the same feature state.

Targeting-based flag in appsettings:
```json
{
  "id": "MyFlag",
  "enabled": true,
  "conditions": {
    "client_filters": [
      {
        "name": "Microsoft.Targeting",
        "parameters": {
          "Audience": {
            "DefaultRolloutPercentage": 50
          }
        }
      }
    ]
  }
}
```

The `Microsoft.Targeting` filter supports:
- `DefaultRolloutPercentage` — percentage of users (tenants) that see the feature
- `Users` — list of specific UserIds (TenantIds) to include
- `Groups` — list of groups with name and rollout percentage (not currently used)

When no tenant is resolved (unauthenticated requests), `UserId` is empty and targeting filters won't match.

## Frontend Feature Flags

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
