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

### Toast Notifications

- Error/success feedback renders via `ToastNotification` in a fixed-position container — NOT inline
- Locate toasts with `GetByTestId("toast-error")`, `GetByTestId("toast-success")`, etc. — pattern is `toast-{kind}`
- Page models expose `ErrorDisplay` locator pointing to `toast-error`; use `VerifyErrorContainsTextAsync()` to assert error text

### Carbon SideNav Overlay (Mobile)

- Carbon's SideNav renders inside the Header's stacking context (`position: fixed`), which can cause page content to intercept pointer events — `ClickAsync()` fails with "element intercepts pointer events" and `Force = true` clicks wrong elements via coordinates. Use `DispatchEventAsync("click")` to bypass hit-testing entirely. Wait for `Expect(SideNav).ToBeVisibleAsync()` beforehand (NOT `ToBeInViewportAsync()` — the slide-in animation makes viewport intersection unreliable in headless CI). Example:
  ```csharp
  await HamburgerButton.ClickAsync();
  await Expect(SideNav).ToBeVisibleAsync();
  var link = SideNav.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true });
  await link.DispatchEventAsync("click");
  ```
- **Mobile tests:** Subclass `MobileAppPageTest` (375×667 viewport, `IsMobile = true`, `HasTouch = true`). Use `LoginOnMobileAsync()` instead of `LoginAsync()` — the standard login asserts sidebar link visibility which fails on mobile.

## HttpClient in E2E Test Fixtures

- **Never** construct `HttpClient` directly — use `IHttpClientFactory` via `ServiceCollection`
- Add `Microsoft.Extensions.Http.Resilience` for transient retry (408, 429, 503, 504)
- **Never** retry on 500 — that's a server bug, not a transient error
- Server returns 503 for `TimeoutException` (SQL timeouts etc.) — see `ExceptionHandlingBehavior`
- Pattern: see `ApiFixture.cs` constructor for `AddResilienceHandler` setup
- Dispose `ServiceProvider` in `DisposeAsync()` — it owns the handler pipeline
