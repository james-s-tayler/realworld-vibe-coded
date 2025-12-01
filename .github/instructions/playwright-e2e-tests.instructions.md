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

    **Critical for CI Stability:**
    After navigation that triggers data changes (e.g., creating articles, following users):
    1. Use WaitForLoadStateAsync(LoadState.NetworkIdle) to ensure all API calls complete
    2. Wait for loading indicators to disappear before asserting content
    3. Verify expected content is visible before proceeding to next action

    Example - After creating an article:
    ```csharp
    await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();
    await Page.WaitForURLAsync(new Regex(@"/article/"), new() { Timeout = DefaultTimeout });
    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = DefaultTimeout });
    var articleHeading = Page.GetByRole(AriaRole.Heading, new() { Name = articleTitle });
    await Expect(articleHeading).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    ```

    Example - Before checking tab content:
    ```csharp
    await favoritedTab.ClickAsync();
    var panel = Page.GetByRole(AriaRole.Tabpanel).First;
    var loadingIndicator = panel.GetByText("Loading articles...");
    await Expect(loadingIndicator).ToBeHiddenAsync(new() { Timeout = DefaultTimeout });
    // Now safe to check for articles
    ```

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

CI Environment Considerations

    CI environments are slower than local development machines
    Use generous timeouts (30s default) to account for CI variability
    Always use WaitForLoadStateAsync(NetworkIdle) after form submissions or navigation
    Never assume immediate state changes - always wait for UI confirmation

Test Structure

    Follow AAA pattern: Arrange, Act, Assert
    Keep tests focused on one scenario
    Use descriptive test method names that explain what is being tested

Error Handling

    Let tests fail naturally with meaningful error messages
    Use Playwright's assertions for clear failure messages
    Add screenshots and traces for debugging (configure in test setup)

Test Data

    Use GUIDs for unique identifiers to ensure uniqueness across parallel CI runs
    Example: var email = $"{username}{Guid.NewGuid():N[..8]}@test.com";
    Clean up test data when possible (or use isolated test databases)
    Make test data realistic but minimal
    Wipe database before each test to ensure isolation

**Anti-Patterns to Avoid**

❌ Manual sleeps: await Task.Delay(1000);
✅ Use: await Expect(element).ToBeVisibleAsync();

❌ Brittle selectors: await Page.Locator("div.class > span:nth-child(2)").ClickAsync();
✅ Use: await Page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

❌ Shared state: private static string sharedUsername;
✅ Use: var username = $"user_{Guid.NewGuid():N[..8]}";

❌ Cross-test dependencies: Test B assumes Test A ran first
✅ Each test sets up its own preconditions

❌ Async void: public async void TestMethod()
✅ Use: public async Task TestMethod()

❌ Short timeouts in CI: const int Timeout = 5000;
✅ Use: const int DefaultTimeout = 30000; // 30s for CI stability

❌ Assuming immediate state after navigation:
```csharp
await publishButton.ClickAsync();
await Page.WaitForURLAsync("/article/...");
// Immediately checking content - FLAKY!
var title = Page.GetByRole(AriaRole.Heading);
```
✅ Wait for network idle and verify content:
```csharp
await publishButton.ClickAsync();
await Page.WaitForURLAsync("/article/...");
await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
var title = Page.GetByRole(AriaRole.Heading, new() { Name = expectedTitle });
await Expect(title).ToBeVisibleAsync();
```

❌ Checking tab content without waiting for load:
```csharp
await tab.ClickAsync();
var article = panel.Locator(".article-preview"); // May still be loading!
```
✅ Wait for loading indicator to disappear:
```csharp
await tab.ClickAsync();
await Expect(panel.GetByText("Loading...")).ToBeHiddenAsync();
var article = panel.Locator(".article-preview");
await Expect(article).ToBeVisibleAsync();
```