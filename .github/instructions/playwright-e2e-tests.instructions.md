---
applyTo: "Test/e2e/E2eTests/*.cs"
---

**Playwright E2E Test Guidelines**

**Core Principles**

Use Official .NET Playwright Driver

    Write tests in C# using the official .NET Playwright driver (Microsoft.Playwright)
    Leverage Playwright's built-in capabilities for browser automation

**Architecture**

 - Each test interacts only with page objects via the page object model
 - Each page object subclasses BasePage
 - Each page has a test folder with tests split into three classes
   - `${Page}HappyPathTests.cs`
   - `${Page}ValidationTests.cs`
   - `${Page}PermissionsTests.cs`
 - Each page test subclasses `AppPageTest`
 - All data in the database is automatically wiped before each test

Use Appropriate Selectors

    Prefer ARIA roles, labels, and accessible attributes
    Use data-test-id or test-specific attributes when available
    Avoid brittle CSS selectors or DOM structure dependencies
    Examples:
    Good: GetByRole(AriaRole.Button, new() { Name = "Sign up" })
    Good: GetByLabel("Username")
    Good: GetByTestId("article-preview")
    Avoid: .css-selector-class button:nth-child(2)

Use Appropriate Waiting Strategies

    Rely on Playwright's built-in auto-waiting
    Use Expect() assertions instead of manual waits
    Avoid WaitForAsync() - causes flaky tests
    Avoid WaitForURLAsync() - causes flaky tests
    Avoid WaitForLoadStateAsync() - causes flaky tests
    Avoid WaitForTimeoutAsync() - causes flaky tests
    Avoid Task.Delay() - causes flaky tests
    Avoid xUnit assertions - these are code smells
    Prefer:
    Expect(locator).ToBeVisibleAsync()
    Expect(page).ToHaveURLAsync(...)
    Expect(locator).ToHaveTextAsync(...)
    Expect(locator).ToHaveCountAsync(...)

**Best Practices**

Test Structure

    Follow AAA pattern: Arrange, Act, Assert
    The test should use the page object model to interact with pages
    Keep tests focused on one scenario

Test Data

    Use GUIDs for unique identifiers to ensure uniqueness across parallel CI runs
    Example: var email = $"{username}{Guid.NewGuid().ToString("N")[..8]}@test.com";
    Make test data realistic but minimal

**Anti-Patterns to Avoid**

❌ Manual sleeps: await Task.Delay(1000);
✅ Use: await Expect(element).ToBeVisibleAsync();

❌ Brittle selectors: await Page.Locator("div.class > span:nth-child(2)").ClickAsync();
✅ Use: await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

❌ Shared state: private static string sharedUsername;
✅ Use: var username = $"user_{Guid.NewGuid().ToString("N")[..8]}";

❌ Cross-test dependencies: Test B assumes Test A ran first
✅ Each test sets up its own preconditions

❌ Async void: public async void TestMethod()
✅ Use: public async Task TestMethod()