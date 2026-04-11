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
- **Carbon Checkbox:** Use `Force = true` on `CheckAsync()`/`UncheckAsync()`. Carbon renders checkboxes as hidden `<input>` + visible `<label>` — the label intercepts pointer events. Example: `GetRoleCheckbox("ADMIN").CheckAsync(new() { Force = true })`
- **Expected login failures:** Use `Pages.LoginPage.LoginAndExpectErrorAsync()`, NOT `LoginAsync()`. `LoginAsync` asserts successful navigation and will timeout if login fails. For testing that a deactivated/invalid user cannot log in, use `LoginAndExpectErrorAsync` + `VerifyErrorContainsTextAsync`.

### Carbon SideNav Overlay (Mobile)

- SideNav links on mobile overlay: use `DispatchEventAsync("click")` — page content can intercept pointer events on fixed-position overlays
- Prefer `ToBeVisibleAsync` over `ToBeInViewportAsync` — SideNav slide-in animation makes viewport checks flaky in headless CI
- `MobileAppPageTest` base class: configures 375x667 viewport, `IsMobile`, `HasTouch`; provides `HamburgerButton`, `CloseMenuButton`, `SideNav` locators and `LoginOnMobileAsync()`

### HttpClient in E2E Test Fixtures

- **Never** construct `HttpClient` directly — use `IHttpClientFactory` via `ServiceCollection`
- Add `Microsoft.Extensions.Http.Resilience` for transient retry (408, 429, 503, 504)
- **Never** retry on 500 — that's a server bug, not a transient error
- Server returns 503 for `TimeoutException` (SQL timeouts etc.) — see `ExceptionHandlingBehavior`
- Pattern: see `ApiFixture.cs` constructor for `AddResilienceHandler` setup
- Dispose `ServiceProvider` in `DisposeAsync()` — it owns the handler pipeline
