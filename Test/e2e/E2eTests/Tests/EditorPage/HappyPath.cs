using Microsoft.Playwright;

namespace E2eTests.Tests.EditorPage;

/// <summary>
/// Happy path tests for the Editor page (/editor and /editor/:slug).
/// </summary>
public class HappyPath : AppPageTest
{
  public HappyPath(ApiFixture apiFixture) : base(apiFixture)
  {
  }

  [Fact]
  public async Task UserCanCreateArticle_AndViewArticle()
  {
    // Arrange
    var user = await Api.CreateUserAsync();

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.HomePage.ClickNewArticleAsync();

    var articleTitle = $"E2E Test Article {GenerateUniqueUsername("art")}";
    var articleDescription = "This is a test article created by E2E tests";
    var articleBody = "# Test Article\n\nThis is the body of the test article created for E2E testing purposes.";
    var articleTag = "e2etest";

    // Act
    await Pages.EditorPage.CreateArticleWithTagsAsync(articleTitle, articleDescription, articleBody, articleTag);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(articleTitle);
    await Pages.ArticlePage.VerifyAuthorAsync(user.Email);
  }

  [Fact]
  public async Task UserCanCreateArticleWithTags_AndViewTagsOnArticle()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..8]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..8]}";

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.ClickNewArticleAsync();

    var title = $"Tagged Article {Guid.NewGuid().ToString("N")[..8]}";

    // Act
    await Pages.EditorPage.CreateArticleWithTagsListAsync(
      title, "Description", "Body content", tag1, tag2);

    // Assert
    await Pages.ArticlePage.VerifyArticleTagsVisibleAsync(tag1, tag2);
  }

  [Fact]
  public async Task TagsAppearBelowInput_WhenAddedViaEnter()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.ClickNewArticleAsync();

    var tag = $"tag{Guid.NewGuid().ToString("N")[..6]}";

    // Act
    await Pages.EditorPage.AddTagViaEnterAsync(tag);

    // Assert
    await Pages.EditorPage.VerifyTagVisibleAsync(tag);
  }

  [Fact]
  public async Task TagsAppearBelowInput_WhenAddedViaComma()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.ClickNewArticleAsync();

    var tag = $"tag{Guid.NewGuid().ToString("N")[..6]}";

    // Act
    await Pages.EditorPage.AddTagViaCommaAsync(tag);

    // Assert
    await Pages.EditorPage.VerifyTagVisibleAsync(tag);
  }

  [Fact]
  public async Task ExistingTagsDisplayed_WhenEditingArticle()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    var article = await Api.CreateArticleAsync(user.Token, [tag1, tag2]);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.ArticlePage.GoToAsync(article.Slug);
    await Pages.ArticlePage.ClickEditButtonAsync();

    // Assert — existing tags should be displayed
    await Pages.EditorPage.VerifyTagVisibleAsync(tag1);
    await Pages.EditorPage.VerifyTagVisibleAsync(tag2);
  }

  [Fact]
  public async Task TagsCanBeIndividuallyRemoved()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.ClickNewArticleAsync();

    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    await Pages.EditorPage.AddTagViaEnterAsync(tag1);
    await Pages.EditorPage.AddTagViaEnterAsync(tag2);
    await Pages.EditorPage.VerifyTagVisibleAsync(tag1);
    await Pages.EditorPage.VerifyTagVisibleAsync(tag2);

    // Act — remove first tag
    await Pages.EditorPage.RemoveTagAsync(tag1);

    // Assert
    await Pages.EditorPage.VerifyTagNotVisibleAsync(tag1);
    await Pages.EditorPage.VerifyTagVisibleAsync(tag2);
  }

  [Fact]
  public async Task RemovedTagIsPersistedAfterSave()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var tag1 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    var tag2 = $"tag{Guid.NewGuid().ToString("N")[..6]}";
    var article = await Api.CreateArticleAsync(user.Token, [tag1, tag2]);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.ArticlePage.GoToAsync(article.Slug);
    await Pages.ArticlePage.ClickEditButtonAsync();

    // Act — remove tag1 and save
    await Pages.EditorPage.RemoveTagAsync(tag1);
    await Pages.EditorPage.ClickPublishButtonAsync();
    await Pages.ArticlePage.VerifyArticleTitleAsync(article.Title);

    // Assert — tag1 should be gone, tag2 should remain
    await Pages.ArticlePage.VerifyArticleTagNotVisibleAsync(tag1);
    await Pages.ArticlePage.VerifyArticleTagsVisibleAsync(tag2);
  }

  [Fact]
  public async Task UnsubmittedTagInInputIsAddedOnPublish()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);
    await Pages.HomePage.ClickNewArticleAsync();

    var title = $"Pending Tag Article {Guid.NewGuid().ToString("N")[..8]}";
    var pendingTag = $"tag{Guid.NewGuid().ToString("N")[..6]}";

    // Act — fill form with tag text left in input (not pressed Enter)
    await Pages.EditorPage.FillArticleFormAsync(title, "Description", "Body content");
    await Pages.EditorPage.TagsInput.FillAsync(pendingTag);
    await Pages.EditorPage.ClickPublishButtonAsync();
    await Pages.ArticlePage.VerifyArticleTitleAsync(title);

    // Assert — the pending tag should have been added
    await Pages.ArticlePage.VerifyArticleTagsVisibleAsync(pendingTag);
  }

  [Fact]
  public async Task UserCanEditOwnArticle()
  {
    // Arrange
    var user = await Api.CreateUserAsync();
    var article = await Api.CreateArticleAsync(user.Token);

    await Pages.LoginPage.GoToAsync();
    await Pages.LoginPage.LoginAsync(user.Email, user.Password);

    await Pages.ArticlePage.GoToAsync(article.Slug);

    await Pages.ArticlePage.ClickEditButtonAsync();

    var updatedTitle = $"{article.Title} - Updated";

    // Act
    await Pages.EditorPage.UpdateArticleAsync(updatedTitle);

    // Assert
    await Pages.ArticlePage.VerifyArticleTitleAsync(updatedTitle);
  }
}
