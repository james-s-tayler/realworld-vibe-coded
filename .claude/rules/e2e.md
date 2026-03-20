---
paths:
  - "Test/e2e/**"
---

## Playwright E2E Conventions

- Page Object Model: each page object subclasses `BasePage`
- Tests organized by folder: `Tests/${Page}/HappyPath.cs`, `Tests/${Page}/Validation.cs`, `Tests/${Page}/Permissions.cs`
- All test classes subclass `AppPageTest`
- Database wiped before each test
- Use ARIA selectors (`GetByRole`, `GetByLabel`, `GetByTestId`) — avoid brittle CSS selectors
- Use `Expect()` assertions — **never** `WaitForAsync()`, `WaitForURLAsync()`, `WaitForLoadStateAsync()`, `Task.Delay()`
- GUIDs for test data uniqueness
