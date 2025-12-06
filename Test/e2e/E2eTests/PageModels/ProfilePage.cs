using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Profile page (/profile/:username).
/// </summary>
public class ProfilePage : BasePage
{
  public ProfilePage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Profile username heading.
  /// </summary>
  public ILocator GetUsernameHeading(string username) =>
    Page.GetByRole(AriaRole.Heading, new() { Name = username, Exact = true });

  /// <summary>
  /// Follow button.
  /// </summary>
  public ILocator GetFollowButton(string username) =>
    Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {username}" });

  /// <summary>
  /// Unfollow button.
  /// </summary>
  public ILocator GetUnfollowButton(string username) =>
    Page.GetByRole(AriaRole.Button, new() { Name = $"Unfollow {username}" });

  /// <summary>
  /// My Articles tab.
  /// </summary>
  public ILocator MyArticlesTab => Page.GetByRole(AriaRole.Tab, new() { Name = "My Articles" });

  /// <summary>
  /// Favorited Articles tab.
  /// </summary>
  public ILocator FavoritedArticlesTab => Page.GetByRole(AriaRole.Tab, new() { Name = "Favorited Articles" });

  /// <summary>
  /// The currently visible tab panel.
  /// </summary>
  public ILocator TabPanel => Page.GetByRole(AriaRole.Tabpanel).First;

  /// <summary>
  /// Article previews in the currently visible tab panel.
  /// </summary>
  public ILocator ArticlePreviews => TabPanel.Locator(".article-preview");

  /// <summary>
  /// Loading indicator in the tab panel.
  /// </summary>
  public ILocator LoadingIndicator => TabPanel.GetByText("Loading articles...");

  /// <summary>
  /// Pagination control.
  /// </summary>
  public ILocator Pagination => TabPanel.Locator(".cds--pagination");

  /// <summary>
  /// Forward pagination button.
  /// </summary>
  public ILocator PaginationForwardButton => TabPanel.Locator(".cds--pagination__button--forward");

  /// <summary>
  /// Backward pagination button.
  /// </summary>
  public ILocator PaginationBackwardButton => TabPanel.Locator(".cds--pagination__button--backward");

  /// <summary>
  /// Bio text on the profile.
  /// </summary>
  public ILocator GetBioText(string bioText) => Page.GetByText(bioText);

  /// <summary>
  /// Waits for the profile to load.
  /// </summary>
  public async Task WaitForProfileToLoadAsync(string username)
  {
    var heading = GetUsernameHeading(username);
    await Expect(heading).ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks the follow button.
  /// </summary>
  public async Task ClickFollowButtonAsync(string username)
  {
    var followButton = GetFollowButton(username);
    await Expect(followButton).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
    await followButton.ClickAsync();
    await Expect(GetUnfollowButton(username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Clicks the unfollow button.
  /// </summary>
  public async Task ClickUnfollowButtonAsync(string username)
  {
    var unfollowButton = GetUnfollowButton(username);
    await unfollowButton.ClickAsync();
    await Expect(GetFollowButton(username)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Clicks the follow button without waiting for success (for unauthenticated tests).
  /// </summary>
  public async Task ClickFollowButtonWithoutWaitAsync(string username)
  {
    var followButton = GetFollowButton(username);
    await followButton.ClickAsync();
  }

  /// <summary>
  /// Clicks on the My Articles tab.
  /// </summary>
  public async Task ClickMyArticlesTabAsync()
  {
    await MyArticlesTab.ClickAsync();
    await WaitForArticlesToLoadAsync();
  }

  /// <summary>
  /// Clicks on the Favorited Articles tab.
  /// </summary>
  public async Task ClickFavoritedArticlesTabAsync()
  {
    await FavoritedArticlesTab.ClickAsync();
    await WaitForArticlesToLoadAsync();
  }

  /// <summary>
  /// Waits for the loading indicator to disappear.
  /// </summary>
  public async Task WaitForArticlesToLoadAsync()
  {
    await Expect(LoadingIndicator).ToBeHiddenAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Gets an article preview by title.
  /// </summary>
  public ILocator GetArticlePreviewByTitle(string title) =>
    TabPanel.Locator(".article-preview").Filter(new() { HasText = title }).First;

  /// <summary>
  /// Navigates to the next page of articles using pagination.
  /// </summary>
  public async Task ClickNextPageAsync()
  {
    await Expect(PaginationForwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
    await PaginationForwardButton.ClickAsync();
    await Expect(ArticlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Navigates to the previous page of articles using pagination.
  /// </summary>
  public async Task ClickPreviousPageAsync()
  {
    await Expect(PaginationBackwardButton).ToBeEnabledAsync(new() { Timeout = DefaultTimeout });
    await PaginationBackwardButton.ClickAsync();
    await Expect(ArticlePreviews.First).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the profile heading is visible.
  /// </summary>
  public async Task VerifyProfileHeadingAsync(string username)
  {
    var heading = GetUsernameHeading(username);
    await Expect(heading).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the My Articles tab is visible.
  /// </summary>
  public async Task VerifyMyArticlesTabVisibleAsync()
  {
    await Expect(MyArticlesTab).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that a specific number of article previews are displayed.
  /// </summary>
  public async Task VerifyArticleCountAsync(int expectedCount)
  {
    await Expect(ArticlePreviews).ToHaveCountAsync(expectedCount, new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that an article with the given title is visible.
  /// </summary>
  public async Task VerifyArticleVisibleAsync(string title)
  {
    var articlePreview = GetArticlePreviewByTitle(title);
    await Expect(articlePreview).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the bio text is visible on the profile.
  /// </summary>
  public async Task VerifyBioVisibleAsync(string bioText)
  {
    await Expect(GetBioText(bioText)).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that pagination is visible.
  /// </summary>
  public async Task VerifyPaginationVisibleAsync()
  {
    await Expect(Pagination).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }
}
