---
applyTo: "Test/e2e/E2eTests/*.cs"
---

**Playwright E2E Test Guidelines**

**Core Principles**

Use Official .NET Playwright Driver

    Write tests in C# using the official .NET Playwright driver (Microsoft.Playwright)
    Leverage Playwright's built-in capabilities for browser automation

Keep Tests Deterministic, Isolated, and Readable

    Tests must be deterministic: same input always produces same output
    Tests must be isolated: no cross-test dependencies
    Avoid randomness in tests (use fixed test data)
    Each test should be independently runnable
    Tests should be self-documenting and easy to understand

Centralize Common Flows

    Extract common workflows (login, registration, navigation) into helper methods or page objects
    Use descriptive helper method names like LoginAsAsync(username, password), RegisterUserAsync(...), CreateArticleAsync(...)
    Avoid repeating the same action sequences across multiple tests
    Keep test methods focused on the specific scenario being tested

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
    Avoid WaitForTimeoutAsync() or Task.Delay() - these are code smells
    Prefer:
    Expect(locator).ToBeVisibleAsync()
    Expect(page).ToHaveURLAsync(...)
    Expect(locator).ToHaveTextAsync(...)
    Expect(locator).ToHaveCountAsync(...)
    Use WaitForLoadStateAsync() or WaitForResponseAsync() for specific scenarios only

Use Async/Await Everywhere

    All test methods must return Task (never async void)
    Use async/await for all Playwright operations
    Example:

    [Fact]
    public async Task UserCanSignUp()
    {
        await Page.GotoAsync("/register");
        await Page.GetByLabel("Username").FillAsync("testuser");
        // ...
    }

Reuse Shared Fixtures

    Use shared fixtures for Playwright/browser/context/page setup
    Avoid duplicating initialization logic across test classes
    Extend base test classes when fixtures are available
    Let the test framework manage lifecycle (setup/teardown)

Ensure Parallel Safety

    Tests should be safe to run in parallel
    No shared mutable state between tests
    Each test should use unique test data (e.g., unique usernames)
    Use test isolation features provided by the test framework

**Best Practices**

Test Structure

    Follow AAA pattern: Arrange, Act, Assert
    Keep tests focused on one scenario
    Use descriptive test method names that explain what is being tested

Error Handling

    Let tests fail naturally with meaningful error messages
    Use Playwright's assertions for clear failure messages
    Add screenshots and traces for debugging (configure in test setup)

Test Data

    Use unique identifiers (timestamps, GUIDs) to avoid conflicts
    Clean up test data when possible (or use isolated test databases)
    Make test data realistic but minimal

**Anti-Patterns to Avoid**

❌ Manual sleeps: await Task.Delay(1000);
✅ Use: await Expect(element).ToBeVisibleAsync();

❌ Brittle selectors: await Page.Locator("div.class > span:nth-child(2)").ClickAsync();
✅ Use: await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

❌ Shared state: private static string sharedUsername;
✅ Use: var username = $"user_{DateTime.UtcNow.Ticks}";

❌ Cross-test dependencies: Test B assumes Test A ran first
✅ Each test sets up its own preconditions

❌ Async void: public async void TestMethod()
✅ Use: public async Task TestMethod()