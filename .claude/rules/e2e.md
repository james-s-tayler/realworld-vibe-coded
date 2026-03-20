---
paths:
  - "Test/e2e/**"
---

## Playwright E2E Conventions

- Page Object Model: each page object subclasses `BasePage`
- Test classes per page: `${Page}HappyPathTests.cs`, `${Page}ValidationTests.cs`, `${Page}PermissionsTests.cs`
- All test classes subclass `AppPageTest`
- Database wiped before each test
- Use ARIA selectors (`GetByRole`, `GetByLabel`, `GetByTestId`) — avoid brittle CSS selectors
- Use `Expect()` assertions — **never** `WaitForAsync()`, `WaitForURLAsync()`, `WaitForLoadStateAsync()`, `Task.Delay()`
- GUIDs for test data uniqueness
