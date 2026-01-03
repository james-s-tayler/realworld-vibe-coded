using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Users page (/users).
/// </summary>
public class UsersPage : BasePage
{
  public UsersPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Gets the page heading.
  /// </summary>
  public ILocator Heading => Page.GetByRole(AriaRole.Heading, new() { Name = "Users" });

  /// <summary>
  /// Gets the "Invite User" button.
  /// </summary>
  public ILocator InviteUserButton => Page.GetByRole(AriaRole.Button, new() { Name = "Invite User" });

  /// <summary>
  /// Gets the invite modal.
  /// </summary>
  public ILocator InviteModal => Page.Locator(".cds--modal").Filter(new() { HasText = "Invite User" });

  /// <summary>
  /// Gets the email input in the invite modal.
  /// </summary>
  public ILocator InviteEmailInput => InviteModal.GetByLabel("Email");

  /// <summary>
  /// Gets the password input in the invite modal.
  /// </summary>
  public ILocator InvitePasswordInput => InviteModal.GetByLabel("Password");

  /// <summary>
  /// Gets the invite submit button in the modal.
  /// </summary>
  public ILocator InviteSubmitButton => InviteModal.GetByRole(AriaRole.Button, new() { Name = "Invite", Exact = true });

  /// <summary>
  /// Gets the cancel button in the invite modal.
  /// </summary>
  public ILocator InviteCancelButton => InviteModal.GetByRole(AriaRole.Button, new() { Name = "Cancel" });

  /// <summary>
  /// Gets a user row by username.
  /// </summary>
  public ILocator GetUserRowByUsername(string username) =>
    Page.GetByRole(AriaRole.Row).Filter(new() { HasText = username });

  /// <summary>
  /// Gets a link to a user's profile by username in the users table.
  /// </summary>
  public new ILocator GetUserProfileLink(string username) =>
    Page.GetByRole(AriaRole.Link, new() { Name = username, Exact = true });

  /// <summary>
  /// Navigates to the Users page.
  /// </summary>
  public override async Task GoToAsync(string urlParams = "")
  {
    await Page.GotoAsync($"{BaseUrl}/users");
  }

  /// <summary>
  /// Opens the invite user modal.
  /// </summary>
  public async Task OpenInviteModalAsync()
  {
    await InviteUserButton.ClickAsync();
    await Expect(InviteModal).ToBeVisibleAsync();
  }

  /// <summary>
  /// Invites a user by filling in the form and submitting.
  /// </summary>
  public async Task InviteUserAsync(string email, string password)
  {
    await OpenInviteModalAsync();
    await InviteEmailInput.FillAsync(email);
    await InvitePasswordInput.FillAsync(password);
    await InviteSubmitButton.ClickAsync();
    await Expect(InviteModal).Not.ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks on a user's profile link.
  /// </summary>
  public async Task ClickUserProfileLinkAsync(string username)
  {
    await GetUserProfileLink(username).ClickAsync();
    await Expect().ToHaveURLAsync($"{BaseUrl}/profile/{username}");
  }

  /// <summary>
  /// Verifies that a user is visible in the table.
  /// </summary>
  public async Task VerifyUserVisibleAsync(string username)
  {
    await Expect(GetUserRowByUsername(username)).ToBeVisibleAsync();
  }
}
