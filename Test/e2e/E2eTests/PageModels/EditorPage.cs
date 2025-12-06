using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Article Editor page (/editor and /editor/:slug).
/// </summary>
public class EditorPage : BasePage
{
  public EditorPage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Article title input field.
  /// </summary>
  public ILocator TitleInput => Page.GetByPlaceholder("Article Title");

  /// <summary>
  /// Article description input field.
  /// </summary>
  public ILocator DescriptionInput => Page.GetByPlaceholder("What's this article about?");

  /// <summary>
  /// Article body textarea.
  /// </summary>
  public ILocator BodyInput => Page.GetByPlaceholder("Write your article (in markdown)");

  /// <summary>
  /// Tags input field.
  /// </summary>
  public ILocator TagsInput => Page.GetByPlaceholder("Enter tags");

  /// <summary>
  /// Publish article button.
  /// </summary>
  public ILocator PublishButton => Page.GetByRole(AriaRole.Button, new() { Name = "Publish Article" });

  /// <summary>
  /// Error display element.
  /// </summary>
  public ILocator ErrorDisplay => Page.GetByTestId("error-display");

  /// <summary>
  /// Fills in the article form without tags.
  /// </summary>
  public async Task FillArticleFormAsync(string title, string description, string body)
  {
    await TitleInput.FillAsync(title);
    await DescriptionInput.FillAsync(description);
    await BodyInput.FillAsync(body);
  }

  /// <summary>
  /// Fills in the article form with tags.
  /// </summary>
  public async Task FillArticleFormAsync(string title, string description, string body, string tags)
  {
    await FillArticleFormAsync(title, description, body);
    await TagsInput.FillAsync(tags);
  }

  /// <summary>
  /// Clears and updates the article title.
  /// </summary>
  public async Task UpdateTitleAsync(string newTitle)
  {
    await TitleInput.ClearAsync();
    await TitleInput.FillAsync(newTitle);
  }

  /// <summary>
  /// Clicks the publish button.
  /// </summary>
  public async Task ClickPublishButtonAsync()
  {
    await PublishButton.ClickAsync();
  }

  /// <summary>
  /// Creates a new article and navigates to the article page.
  /// </summary>
  /// <param name="title">Article title.</param>
  /// <param name="description">Article description.</param>
  /// <param name="body">Article body content.</param>
  /// <returns>ArticlePage after successful creation.</returns>
  public async Task<ArticlePage> CreateArticleAsync(string title, string description, string body)
  {
    await FillArticleFormAsync(title, description, body);
    await ClickPublishButtonAsync();

    var articlePage = new ArticlePage(Page, BaseUrl);
    await articlePage.VerifyArticleTitleAsync(title);
    return articlePage;
  }

  /// <summary>
  /// Creates a new article with tags and navigates to the article page.
  /// </summary>
  /// <param name="title">Article title.</param>
  /// <param name="description">Article description.</param>
  /// <param name="body">Article body content.</param>
  /// <param name="tags">Article tags (space or comma separated).</param>
  /// <returns>ArticlePage after successful creation.</returns>
  public async Task<ArticlePage> CreateArticleWithTagsAsync(string title, string description, string body, string tags)
  {
    await FillArticleFormAsync(title, description, body, tags);
    await ClickPublishButtonAsync();

    var articlePage = new ArticlePage(Page, BaseUrl);
    await articlePage.VerifyArticleTitleAsync(title);
    return articlePage;
  }

  /// <summary>
  /// Updates an existing article and navigates to the article page.
  /// </summary>
  /// <param name="newTitle">New article title.</param>
  /// <returns>ArticlePage after successful update.</returns>
  public async Task<ArticlePage> UpdateArticleAsync(string newTitle)
  {
    await UpdateTitleAsync(newTitle);
    await ClickPublishButtonAsync();

    var articlePage = new ArticlePage(Page, BaseUrl);
    await articlePage.VerifyArticleTitleAsync(newTitle);
    return articlePage;
  }

  /// <summary>
  /// Attempts to create an article and expects it to fail with an error.
  /// </summary>
  public async Task CreateArticleAndExpectErrorAsync(string title, string description, string body)
  {
    await FillArticleFormAsync(title, description, body);
    await ClickPublishButtonAsync();
    await Expect(ErrorDisplay).ToBeVisibleAsync(new() { Timeout = DefaultTimeout });
  }

  /// <summary>
  /// Verifies that the error display contains specific text.
  /// </summary>
  public async Task VerifyErrorContainsTextAsync(string expectedText)
  {
    await Expect(ErrorDisplay).ToContainTextAsync(expectedText, new() { Timeout = DefaultTimeout });
  }
}
