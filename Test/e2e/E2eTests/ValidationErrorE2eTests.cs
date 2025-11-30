using Microsoft.Playwright;

namespace E2eTests;

[Collection("E2E Tests")]
public class ValidationErrorE2eTests : ConduitPageTest
{
  [Fact]
  public async Task CreateArticle_WithDuplicateTitle_DisplaysErrorMessage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Duplicate Article Title Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register a user
      await RegisterUser();

      // Create the first article
      var uniqueId = GenerateUniqueId();
      var articleTitle = $"Duplicate Test Article {uniqueId}";

      await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

      await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
      await Page.GetByPlaceholder("What's this article about?").FillAsync("Test description");
      await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync("Test body content");
      await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

      // Wait for redirect to article page
      await Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"/article/"), new() { Timeout = DefaultTimeout });

      // Navigate to create another article with the same title
      await Page.GetByRole(AriaRole.Link, new() { Name = "New Article" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/editor", new() { Timeout = DefaultTimeout });

      // Fill in the same title
      await Page.GetByPlaceholder("Article Title").FillAsync(articleTitle);
      await Page.GetByPlaceholder("What's this article about?").FillAsync("Different description");
      await Page.GetByPlaceholder("Write your article (in markdown)").FillAsync("Different body content");
      await Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" }).ClickAsync();

      // Verify error message is displayed
      var errorDisplay = Page.GetByTestId("error-display");
      await Expect(errorDisplay).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify the error contains the validation message about the slug already being taken
      await Expect(errorDisplay).ToContainTextAsync("has already been taken", new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("duplicate_article_title_test");
    }
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_DisplaysErrorMessage()
  {
    await Context.Tracing.StartAsync(new()
    {
      Title = "Duplicate Email Registration Test",
      Screenshots = true,
      Snapshots = true,
      Sources = true,
    });

    try
    {
      // Register the first user with a unique email for this test
      var uniqueId = GenerateUniqueId();
      var email = $"duptest{uniqueId}@test.com";
      var username1 = $"dupusr1{uniqueId}";
      var username2 = $"dupusr2{GenerateUniqueId()}";
      var password = "TestPassword123!";

      // Register first user
      await Page.GotoAsync(BaseUrl, new() { WaitUntil = WaitUntilState.Load, Timeout = DefaultTimeout });
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/register", new() { Timeout = DefaultTimeout });

      await Page.GetByPlaceholder("Username").FillAsync(username1);
      await Page.GetByPlaceholder("Email").FillAsync(email);
      await Page.GetByPlaceholder("Password").FillAsync(password);
      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

      // Wait for successful registration (user link appears in header)
      await Page.GetByRole(AriaRole.Link, new() { Name = username1 }).First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });

      // Sign out
      await SignOut();

      // Try to register a second user with the same email
      await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
      await Page.WaitForURLAsync($"{BaseUrl}/register", new() { Timeout = DefaultTimeout });

      await Page.GetByPlaceholder("Username").FillAsync(username2);
      await Page.GetByPlaceholder("Email").FillAsync(email);
      await Page.GetByPlaceholder("Password").FillAsync(password);
      await Page.GetByRole(AriaRole.Button, new() { Name = "Sign up" }).ClickAsync();

      // Verify error message is displayed
      var errorDisplay = Page.GetByTestId("error-display");
      await Expect(errorDisplay).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });

      // Verify the error contains the validation message about email already existing
      await Expect(errorDisplay).ToContainTextAsync("Email already exists", new() { Timeout = DefaultTimeout });
    }
    finally
    {
      await SaveTrace("duplicate_email_registration_test");
    }
  }
}
