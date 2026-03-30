## Feature Flags

### Constants

Feature flag names live in `App/Server/src/Server.SharedKernel/FeatureFlags/FeatureFlags.cs`.
Add new flags as `public const string` fields.

### Conventions

- New flags must default to `false` in both `appsettings.json` and `appsettings.Testing.json`
- Never use magic strings — always reference `FeatureFlags.*` constants (FF001 analyzer enforces this as a build error)
- Both `appsettings.json` and `appsettings.Testing.json` must include all flags
- `LintAppSettingsVerify` validates the FeatureManagement section against a vendored JSON schema

### Azure App Configuration

- In production, set `AzureAppConfiguration:ConnectionString` to enable Azure App Configuration with feature flag refresh middleware
- Locally, flags are read from `appsettings.json` — no connection string needed

### Checking Flags in Code

```csharp
// In a handler or service — inject IFeatureFlagService
var isEnabled = await featureFlagService.IsEnabledAsync(FeatureFlags.SampleFeature);

// Or inject IFeatureManager directly
var isEnabled = await featureManager.IsEnabledAsync(FeatureFlags.SampleFeature);
```

### DevOnly Endpoint

`GET /api/devonly/feature-flags/{featureName}` — check flag state in development (excluded from production builds).
