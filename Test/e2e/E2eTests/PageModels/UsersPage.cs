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
  /// Gets the edit roles modal.
  /// </summary>
  public ILocator EditRolesModal => Page.Locator(".cds--modal").Filter(new() { HasText = "Edit Roles" });

  /// <summary>
  /// Gets the Save button in the edit roles modal.
  /// </summary>
  public ILocator EditRolesSaveButton => EditRolesModal.GetByRole(AriaRole.Button, new() { Name = "Save" });

  /// <summary>
  /// Gets a checkbox for a specific role in the edit roles modal.
  /// </summary>
  public ILocator GetRoleCheckbox(string role) =>
    EditRolesModal.GetByRole(AriaRole.Checkbox, new() { Name = role, Exact = true });

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
  /// Gets the status tag for a user row.
  /// </summary>
  public ILocator GetUserStatusTag(string username) =>
    GetUserRowByUsername(username).Locator(".cds--tag");

  /// <summary>
  /// Gets the overflow menu button for a user row.
  /// </summary>
  public ILocator GetUserActionsMenu(string username) =>
    GetUserRowByUsername(username).GetByRole(AriaRole.Button, new() { Name = $"Actions for {username}" });

  /// <summary>
  /// Gets the pagination component.
  /// </summary>
  public ILocator Pagination => Page.Locator(".cds--pagination");

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

  /// <summary>
  /// Opens the overflow menu for a user and clicks a menu item.
  /// </summary>
  public async Task ClickUserAction(string username, string actionText)
  {
    await GetUserActionsMenu(username).ClickAsync();
    await Page.GetByRole(AriaRole.Menuitem, new() { Name = actionText }).ClickAsync();
  }

  /// <summary>
  /// Opens the Edit Roles modal for a user.
  /// </summary>
  public async Task OpenEditRolesModalAsync(string username)
  {
    await ClickUserAction(username, "Edit Roles");
    await Expect(EditRolesModal).ToBeVisibleAsync();
  }

  /// <summary>
  /// Saves the edit roles modal.
  /// </summary>
  public async Task SaveEditRolesAsync()
  {
    await EditRolesSaveButton.ClickAsync();
    await Expect(EditRolesModal).Not.ToBeVisibleAsync();
  }
}
