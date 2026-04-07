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
  [TestCoverage(
    Id = "editor-happy-001",
    FeatureArea = "editor",
    Behavior = "User can create a new article with tags and view it on the article page",
    Verifies = ["article title is displayed on article page", "author matches the logged-in user"])]
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
  [TestCoverage(
    Id = "editor-happy-002",
    FeatureArea = "editor",
    Behavior = "User can create an article with multiple tags and see them on the article page",
    Verifies = ["both tags are visible on the published article"])]
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
  [TestCoverage(
    Id = "editor-happy-003",
    FeatureArea = "editor",
    Behavior = "Pressing Enter in the tag input adds the tag below the input field",
    Verifies = ["tag chip is visible below the tag input"])]
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
  [TestCoverage(
    Id = "editor-happy-004",
    FeatureArea = "editor",
    Behavior = "Typing a comma in the tag input adds the tag below the input field",
    Verifies = ["tag chip is visible below the tag input"])]
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
  [TestCoverage(
    Id = "editor-happy-005",
    FeatureArea = "editor",
    Behavior = "Existing tags are pre-populated when editing an article",
    Verifies = ["tag1 is visible in the editor", "tag2 is visible in the editor"])]
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
  [TestCoverage(
    Id = "editor-happy-006",
    FeatureArea = "editor",
    Behavior = "Individual tags can be removed from the editor without affecting other tags",
    Verifies = ["removed tag is no longer visible", "remaining tag is still visible"])]
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
  [TestCoverage(
    Id = "editor-happy-007",
    FeatureArea = "editor",
    Behavior = "Removing a tag in the editor and saving persists the removal on the article page",
    Verifies = ["removed tag is not visible on the article page", "remaining tag is still visible on the article page"])]
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
  [TestCoverage(
    Id = "editor-happy-008",
    FeatureArea = "editor",
    Behavior = "Tag text left in the input field without pressing Enter is automatically added when publishing",
    Verifies = ["pending tag appears on the published article page"])]
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
  [TestCoverage(
    Id = "editor-happy-009",
    FeatureArea = "editor",
    Behavior = "User can edit their own article's title and see the updated title on the article page",
    Verifies = ["updated article title is displayed on the article page"])]
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
