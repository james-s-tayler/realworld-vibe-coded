using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles;
using Server.Web.Articles.Comments.Create;
using Server.Web.Articles.Comments.Delete;
using Server.Web.Articles.Comments.Get;
using Server.Web.Articles.Create;
using Create = Server.Web.Articles.Create.Create;

namespace Server.FunctionalTests.Articles.Comments;

[Collection("Articles Integration Tests")]
public class GetTests : AppTestBase<ApiFixture>
{
  public GetTests(ApiFixture fixture) : base(fixture)
  {
  }

  [Fact]
  public async Task GetComments_WithAuthentication_ReturnsComments()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Comments Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "First comment",
      },
    };

    await user.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    var (response, result) = await user.Client.GETAsync<Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.Count.ShouldBe(1);
    result.Comments[0].Body.ShouldBe("First comment");
  }

  [Fact]
  public async Task GetComments_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Comments Unauthenticated",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment",
      },
    };

    await user.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);

    var (response, _) = await Fixture.Client.GETAsync<Get, GetCommentsRequest, object>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetComments_WithNoComments_ReturnsEmptyList()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "No Comments Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var (response, result) = await user.Client.GETAsync<Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetComments_WithNonExistentArticle_ReturnsUnprocessableEntity()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, _) = await user.Client.GETAsync<Get, GetCommentsRequest, object>(new GetCommentsRequest { Slug = "no-such-article" });

    response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
  }

  [Fact]
  public async Task DeleteComment_WithAuthentication_ReturnsSuccess()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment to delete",
      },
    };

    var createCommentResponse = await user.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var (response, _) = await user.Client.DELETEAsync<Delete, DeleteCommentRequest, object>(new DeleteCommentRequest { Slug = slug, Id = commentId });

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
  }
}
