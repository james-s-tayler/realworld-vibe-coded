using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Home page (/) with feed tabs and tag sidebar.
/// </summary>
public class HomePage : BasePage
{
  public HomePage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Your Feed tab (only visible when logged in).
  /// </summary>
  public ILocator YourFeedTab => Page.GetByRole(AriaRole.Tab, new() { Name = "Your Feed" });

  /// <summary>
  /// Global Feed tab.
  /// </summary>
  public ILocator GlobalFeedTab => Page.GetByRole(AriaRole.Tab, new() { Name = "Global Feed" });

  /// <summary>
  /// Gets the tag filter tab by tag name.
  /// </summary>
  public ILocator GetTagFilterTab(string tagName) =>
    Page.GetByRole(AriaRole.Tab, new() { Name = $"#{tagName}" });

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
  /// Tag sidebar containing popular tags.
  /// </summary>
  public ILocator TagSidebar => Page.Locator(".sidebar .tag-list");

  /// <summary>
  /// Gets a specific tag from the sidebar.
  /// </summary>
  public ILocator GetSidebarTag(string tagName) =>
    Page.Locator(".sidebar .tag-list .cds--tag").Filter(new() { HasText = tagName });

  /// <summary>
  /// Clicks on the Your Feed tab and waits for articles to load.
  /// </summary>
  public async Task ClickYourFeedTabAsync()
  {
    await Expect(YourFeedTab).ToBeVisibleAsync();
    await Expect(GlobalFeedTab).ToBeVisibleAsync();
    await YourFeedTab.ClickAsync();
    await WaitForArticlesToLoadAsync();
  }

  /// <summary>
  /// Clicks on the Global Feed tab and waits for articles to load.
  /// </summary>
  public async Task ClickGlobalFeedTabAsync()
  {
    if (await IsUserLoggedInAsync())
    {
      await Expect(YourFeedTab).ToBeVisibleAsync();
    }

    await Expect(GlobalFeedTab).ToBeVisibleAsync();
    await GlobalFeedTab.ClickAsync();
    await WaitForArticlesToLoadAsync();
  }

  /// <summary>
  /// Clicks on a tag in the sidebar to filter articles.
  /// </summary>
  public async Task ClickSidebarTagAsync(string tagName)
  {
    var tag = GetSidebarTag(tagName);
    await Expect(tag).ToBeVisibleAsync();
    await tag.ClickAsync();
    await WaitForArticlesToLoadAsync();
  }

  /// <summary>
  /// Waits for the loading indicator to disappear.
  /// </summary>
  public async Task WaitForArticlesToLoadAsync()
  {
    await Expect(LoadingIndicator).ToBeHiddenAsync();
  }

  /// <summary>
  /// Gets an article preview by title.
  /// </summary>
  public ILocator GetArticlePreviewByTitle(string title) =>
    TabPanel.Locator(".article-preview").Filter(new() { HasText = title }).First;

  /// <summary>
  /// Clicks on an article link in the feed to navigate to the article page.
  /// </summary>
  public async Task ClickArticleAsync(string title)
  {
    var preview = GetArticlePreviewByTitle(title);
    await Expect(preview).ToBeVisibleAsync();
    await preview.Locator(".article-link").ClickAsync();
  }

  /// <summary>
  /// Clicks the favorite button on an article preview in the feed.
  /// </summary>
  public async Task ClickFavoriteButtonOnPreviewAsync()
  {
    var favoriteButton = TabPanel.Locator(".article-preview .favorite-button").First;
    await Expect(favoriteButton).ToBeVisibleAsync();
    await favoriteButton.ClickAsync();
  }

  /// <summary>
  /// Navigates to the next page of articles using pagination.
  /// </summary>
  public async Task ClickNextPageAsync()
  {
    await Expect(PaginationForwardButton).ToBeEnabledAsync();
    await PaginationForwardButton.ClickAsync();
    await Expect(ArticlePreviews.First).ToBeVisibleAsync();
  }

  /// <summary>
  /// Navigates to the previous page of articles using pagination.
  /// </summary>
  public async Task ClickPreviousPageAsync()
  {
    await Expect(PaginationBackwardButton).ToBeEnabledAsync();
    await PaginationBackwardButton.ClickAsync();
    await Expect(ArticlePreviews.First).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the Global Feed tab is selected.
  /// </summary>
  public async Task VerifyGlobalFeedIsSelectedAsync()
  {
    await Expect(GlobalFeedTab).ToHaveAttributeAsync("aria-selected", "true");
  }

  /// <summary>
  /// Verifies that a specific number of article previews are displayed.
  /// </summary>
  public async Task VerifyArticleCountAsync(int expectedCount)
  {
    await Expect(ArticlePreviews).ToHaveCountAsync(expectedCount);
  }

  /// <summary>
  /// Verifies that an article with the given title is visible in the feed.
  /// </summary>
  public async Task VerifyArticleVisibleAsync(string title)
  {
    var articlePreview = GetArticlePreviewByTitle(title);
    await Expect(articlePreview).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that an article with the given title is NOT visible in the feed.
  /// </summary>
  public async Task VerifyArticleNotVisibleAsync(string title)
  {
    var articleElement = Page.GetByText(title).First;
    await Expect(articleElement).Not.ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that pagination is visible.
  /// </summary>
  public async Task VerifyPaginationVisibleAsync()
  {
    await Expect(Pagination).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the backward pagination button is disabled.
  /// </summary>
  public async Task VerifyBackwardButtonDisabledAsync()
  {
    await Expect(PaginationBackwardButton).ToBeDisabledAsync();
  }

  /// <summary>
  /// Verifies that the tag filter tab is visible.
  /// </summary>
  public async Task VerifyTagFilterTabVisibleAsync(string tagName)
  {
    var tagTab = GetTagFilterTab(tagName);
    await Expect(tagTab).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that at least one article preview is visible.
  /// </summary>
  public async Task VerifyArticlesLoadedAsync()
  {
    await Expect(ArticlePreviews.First).ToBeVisibleAsync();
  }
}
