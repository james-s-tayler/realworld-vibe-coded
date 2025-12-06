using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace E2eTests.PageModels;

/// <summary>
/// Page model for the Article page (/article/:slug).
/// </summary>
public class ArticlePage : BasePage
{
  public ArticlePage(IPage page, string baseUrl)
    : base(page, baseUrl)
  {
  }

  /// <summary>
  /// Article title heading.
  /// </summary>
  public ILocator GetArticleTitle(string title) =>
    Page.GetByRole(AriaRole.Heading, new() { Name = title });

  /// <summary>
  /// Author link in the article meta section.
  /// </summary>
  public ILocator GetAuthorLink(string username) =>
    Page.GetByRole(AriaRole.Link, new() { Name = username }).First;

  /// <summary>
  /// Edit article button (visible for article owner).
  /// </summary>
  public ILocator EditButton => Page.Locator("button").Filter(new() { HasText = "Edit Article" });

  /// <summary>
  /// Delete article button (visible for article owner).
  /// </summary>
  public ILocator DeleteButton => Page.GetByRole(AriaRole.Button, new() { Name = "Delete Article" });

  /// <summary>
  /// Favorite article button.
  /// </summary>
  public ILocator FavoriteButton =>
    Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("Favorite Article") });

  /// <summary>
  /// Unfavorite article button (visible when article is already favorited).
  /// </summary>
  public ILocator UnfavoriteButton =>
    Page.GetByRole(AriaRole.Button).Filter(new() { HasTextRegex = new Regex("Unfavorite Article") });

  /// <summary>
  /// Follow user button.
  /// </summary>
  public ILocator GetFollowButton(string username) =>
    Page.GetByRole(AriaRole.Button, new() { Name = $"Follow {username}" });

  /// <summary>
  /// Unfollow user button.
  /// </summary>
  public ILocator GetUnfollowButton(string username) =>
    Page.GetByRole(AriaRole.Button, new() { Name = $"Unfollow {username}" });

  /// <summary>
  /// Comment textarea.
  /// </summary>
  public ILocator CommentInput => Page.GetByPlaceholder("Write a comment...");

  /// <summary>
  /// Post comment button.
  /// </summary>
  public ILocator PostCommentButton => Page.GetByRole(AriaRole.Button, new() { Name = "Post Comment" });

  /// <summary>
  /// Delete comment button (trash icon).
  /// </summary>
  public ILocator DeleteCommentButton => Page.Locator(".mod-options").First;

  /// <summary>
  /// Navigates directly to an article page by slug.
  /// </summary>
  public override async Task GoToAsync(string slug)
  {
    await base.GoToAsync($"article/{slug}");
  }

  /// <summary>
  /// Clicks the edit button and navigates to the editor page.
  /// </summary>
  public async Task ClickEditButtonAsync()
  {
    await Expect(EditButton).ToBeVisibleAsync();
    await EditButton.ClickAsync();
    await Expect().ToHaveURLAsync(new Regex(@"/editor/"));
  }

  /// <summary>
  /// Clicks the delete button and accepts the confirmation dialog.
  /// </summary>
  public async Task DeleteArticleAsync()
  {
    await Expect(DeleteButton).ToBeVisibleAsync();

    Page.Dialog += async (_, dialog) =>
    {
      await dialog.AcceptAsync();
    };

    await DeleteButton.ClickAsync();
  }

  /// <summary>
  /// Clicks the favorite button.
  /// </summary>
  public async Task ClickFavoriteButtonAsync()
  {
    await Expect(FavoriteButton).ToBeVisibleAsync();
    await FavoriteButton.ClickAsync();
    await Expect(UnfavoriteButton).ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks the unfavorite button.
  /// </summary>
  public async Task ClickUnfavoriteButtonAsync()
  {
    await Expect(UnfavoriteButton).ToBeVisibleAsync();
    await UnfavoriteButton.ClickAsync();
    await Expect(FavoriteButton).ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks the follow button for the article author.
  /// </summary>
  public async Task ClickFollowButtonAsync(string username)
  {
    var followButton = GetFollowButton(username);
    await Expect(followButton).ToBeVisibleAsync();
    await followButton.ClickAsync();
    await Expect(GetUnfollowButton(username)).ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks the unfollow button for the article author.
  /// </summary>
  public async Task ClickUnfollowButtonAsync(string username)
  {
    var unfollowButton = GetUnfollowButton(username);
    await Expect(unfollowButton).ToBeVisibleAsync();
    await unfollowButton.ClickAsync();
    await Expect(GetFollowButton(username)).ToBeVisibleAsync();
  }

  /// <summary>
  /// Clicks the favorite button without waiting for success (for unauthenticated tests).
  /// </summary>
  public async Task ClickFavoriteButtonWithoutWaitAsync()
  {
    await Expect(FavoriteButton).ToBeVisibleAsync();
    await FavoriteButton.ClickAsync();
  }

  /// <summary>
  /// Clicks the follow button without waiting for success (for unauthenticated tests).
  /// </summary>
  public async Task ClickFollowButtonWithoutWaitAsync(string username)
  {
    var followButton = GetFollowButton(username);
    await Expect(followButton).ToBeVisibleAsync();
    await followButton.ClickAsync();
  }

  /// <summary>
  /// Adds a comment to the article.
  /// </summary>
  public async Task AddCommentAsync(string commentText)
  {
    await Expect(CommentInput).ToBeVisibleAsync();
    await CommentInput.FillAsync(commentText);
    await PostCommentButton.ClickAsync();
    await Expect(Page.GetByText(commentText).First).ToBeVisibleAsync();
  }

  /// <summary>
  /// Deletes a comment by clicking the delete button and waiting for the API response.
  /// </summary>
  public async Task DeleteCommentAsync(string commentText)
  {
    var comment = Page.GetByText(commentText);
    await Expect(comment).ToBeVisibleAsync();

    await DeleteCommentButton.ClickAsync();
    await Expect(comment).Not.ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the article title is displayed.
  /// </summary>
  public async Task VerifyArticleTitleAsync(string title)
  {
    var heading = GetArticleTitle(title);
    await Expect(heading).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that the author link is visible.
  /// </summary>
  public async Task VerifyAuthorAsync(string username)
  {
    var authorLink = GetAuthorLink(username);
    await Expect(authorLink).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that a comment is visible.
  /// </summary>
  public async Task VerifyCommentVisibleAsync(string commentText)
  {
    await Expect(Page.GetByText(commentText).First).ToBeVisibleAsync();
  }

  /// <summary>
  /// Verifies that a comment is not visible.
  /// </summary>
  public async Task VerifyCommentNotVisibleAsync(string commentText)
  {
    await Expect(Page.GetByText(commentText)).Not.ToBeVisibleAsync();
  }
}
