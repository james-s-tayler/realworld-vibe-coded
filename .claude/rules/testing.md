## E2E Tests

E2E tests are in `Test/e2e/E2eTests/`:
- Page objects: `PageModels/{PageName}.cs`
- Tests: `Tests/{PageName}/HappyPath.cs`, `Validation.cs`, `Permissions.cs`, `Screenshots.cs`
- Base class: `AppPageTest.cs` — all tests subclass this
- Data setup: `ApiFixture.cs` — creates users, articles, comments via API
- Progressive tier targets exist (`TestE2e{FunctionalArea}`) for faster feedback during development
- Full suite: `TestE2e` (required for commit)

### Feature Flags in Tests

E2E tests run with `ASPNETCORE_ENVIRONMENT=Development`, which loads `appsettings.Development.json` where all feature flags are enabled. This means E2E tests always see features in their enabled state.

Disabled-state coverage comes from:
- **Backend functional tests** — run with Testing config where flags default to `false`
- **Frontend unit tests** — mock `FeatureFlagContext` to test both enabled and disabled states
