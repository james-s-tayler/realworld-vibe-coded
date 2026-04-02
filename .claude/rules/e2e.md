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
- **Carbon SideNav overlay (mobile):** Carbon's SideNav renders inside the Header's stacking context (`position: fixed`), which can cause page content to intercept pointer events — `ClickAsync()` fails with "element intercepts pointer events" and `Force = true` clicks wrong elements via coordinates. Use `DispatchEventAsync("click")` to bypass hit-testing entirely. Wait for `Expect(SideNav).ToBeVisibleAsync()` beforehand (NOT `ToBeInViewportAsync()` — the slide-in animation makes viewport intersection unreliable in headless CI). Example:
  ```csharp
  await HamburgerButton.ClickAsync();
  await Expect(SideNav).ToBeVisibleAsync();
  var link = SideNav.GetByRole(AriaRole.Link, new() { Name = "Settings", Exact = true });
  await link.DispatchEventAsync("click");
  ```
- **Mobile tests:** Subclass `MobileAppPageTest` (375×667 viewport, `IsMobile = true`, `HasTouch = true`). Use `LoginOnMobileAsync()` instead of `LoginAsync()` — the standard login asserts sidebar link visibility which fails on mobile.
