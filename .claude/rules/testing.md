## E2E Tests

E2E tests are in `Test/e2e/E2eTests/`:
- Page objects: `PageModels/{PageName}.cs`
- Tests: `Tests/{PageName}/HappyPath.cs`, `Validation.cs`, `Permissions.cs`, `Screenshots.cs`
- Base class: `AppPageTest.cs` — all tests subclass this
- Data setup: `ApiFixture.cs` — creates users, articles, comments via API
- Progressive tier targets exist (`TestE2e{FunctionalArea}`) for faster feedback during development
- Full suite: `TestE2e` (required for commit)
