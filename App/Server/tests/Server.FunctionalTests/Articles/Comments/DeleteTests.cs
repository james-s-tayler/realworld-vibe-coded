using Server.Core.ArticleAggregate.Dtos;
using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Comments.Create;
using Server.Web.Articles.Create;
using Create = Server.Web.Articles.Create.Create;

namespace Server.FunctionalTests.Articles.Comments;

[Collection("Articles Integration Tests")]
public class DeleteTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task DeleteComment_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Unauth Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteComment_ByWrongUser_ReturnsForbidden()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Wrong User",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.ArticlesUser2Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeleteComment_WithNonExistentArticle_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/no-such-article/comments/1", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteComment_WithNonExistentComment_ReturnsNotFound()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Nonexistent Comment",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/999999", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteComment_WithInvalidCommentId_ReturnsValidationError()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Invalid Comment Id Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/abc", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
