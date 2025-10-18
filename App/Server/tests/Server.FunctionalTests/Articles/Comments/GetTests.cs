using Server.Core.ArticleAggregate.Dtos;
using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Comments.Create;
using Server.Web.Articles.Comments.Get;
using Server.Web.Articles.Create;
using Create = Server.Web.Articles.Create.Create;

namespace Server.FunctionalTests.Articles.Comments;

[Collection("Articles Integration Tests")]
public class GetTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task GetComments_WithAuthentication_ReturnsComments()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Comments Test",
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
        Body = "First comment"
      }
    };

    await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    var (response, result) = await App.ArticlesUser1Client.GETAsync<Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.Count.ShouldBe(1);
    result.Comments[0].Body.ShouldBe("First comment");
  }

  [Fact]
  public async Task GetComments_WithoutAuthentication_ReturnsComments()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Comments Unauthenticated",
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

    await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    var (response, result) = await App.Client.GETAsync<Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.Count.ShouldBe(1);
  }

  [Fact]
  public async Task GetComments_WithNoComments_ReturnsEmptyList()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "No Comments Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var (response, result) = await App.Client.GETAsync<Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetComments_WithNonExistentArticle_ReturnsNotFound()
  {
    var (response, _) = await App.Client.GETAsync<Get, GetCommentsRequest, object>(new GetCommentsRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteComment_WithAuthentication_ReturnsSuccess()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Test",
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
        Body = "Comment to delete"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}", TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }
}
