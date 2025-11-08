using Server.Core.ArticleAggregate.Dtos;
using Server.FunctionalTests.Articles.Fixture;
using Server.UseCases.Articles;
using Server.Web.Articles.Comments.Create;
using Server.Web.Articles.Create;
using Create = Server.Web.Articles.Create.Create;

namespace Server.FunctionalTests.Articles.Comments;

[Collection("Articles Integration Tests")]
public class CreateTests(ArticlesFixture app) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task CreateComment_WithValidData_ReturnsComment()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Test Article",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "This is a test comment",
      },
    };

    var response = await app.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var result = await response.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Comment.ShouldNotBeNull();
    result.Comment.Body.ShouldBe("This is a test comment");
    result.Comment.Author.ShouldNotBeNull();
    result.Comment.Author.Username.ShouldBe(app.ArticlesUser1Username);
  }

  [Fact]
  public async Task CreateComment_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Unauthenticated Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Test comment",
      },
    };

    var response = await app.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task CreateComment_WithNonExistentArticle_ReturnsNotFound()
  {
    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Test comment",
      },
    };

    var response = await app.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/no-such-article/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task CreateComment_WithMissingRequiredFields_ReturnsErrorDetail()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Validation Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto(),
    };

    var response = await app.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateComment_WithEmptyBody_ReturnsErrorDetail()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Empty Body Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await app.ArticlesUser1Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = string.Empty,
      },
    };

    var response = await app.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
