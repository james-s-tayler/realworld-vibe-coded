using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles;
using Server.Web.Articles.Comments.Create;
using Server.Web.Articles.Comments.Delete;
using Server.Web.Articles.Create;
using Create = Server.Web.Articles.Create.Create;

namespace Server.FunctionalTests.Articles.Comments;

public class DeleteTests : AppTestBase
{
  public DeleteTests(ApiFixture apiFixture, ITestOutputHelper output) : base(apiFixture, output)
  {
  }

  [Fact]
  public async Task DeleteComment_WithoutAuthentication_ReturnsUnauthorized()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Unauth Test",
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

    var createCommentResponse = await user.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var (response, _) = await Fixture.Client.DELETEAsync<Delete, DeleteCommentRequest, object>(new DeleteCommentRequest { Slug = slug, Id = commentId });

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteComment_ByWrongUser_ReturnsForbidden()
  {
    // Arrange: Create two users in the same tenant
    var tenant = await Fixture.RegisterTenantWithUsersAsync(2);
    var user1 = tenant.Users[0];
    var user2 = tenant.Users[1];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Comment Wrong User",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user1.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment",
      },
    };

    var createCommentResponse = await user1.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest, cancellationToken: TestContext.Current.CancellationToken);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>(cancellationToken: TestContext.Current.CancellationToken);
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    // Act: User2 tries to delete User1's comment
    var (response, _) = await user2.Client.DELETEAsync<Delete, DeleteCommentRequest, object>(new DeleteCommentRequest { Slug = slug, Id = commentId });

    // Assert
    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeleteComment_WithNonExistentArticle_ReturnsUnprocessableEntity()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var (response, _) = await user.Client.DELETEAsync<Delete, DeleteCommentRequest, object>(new DeleteCommentRequest { Slug = "no-such-article", Id = Guid.NewGuid() });

    response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
  }

  [Fact]
  public async Task DeleteComment_WithNonExistentComment_ReturnsNotFound()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Nonexistent Comment",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var (response, _) = await user.Client.DELETEAsync<Delete, DeleteCommentRequest, object>(new DeleteCommentRequest { Slug = slug, Id = Guid.NewGuid() });

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteComment_WithInvalidCommentId_ReturnsErrorDetail()
  {
    var tenant = await Fixture.RegisterTenantAsync();
    var user = tenant.Users[0];

    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Invalid Comment Id Test",
        Description = "Description",
        Body = "Body",
      },
    };

    var (_, createArticleResult) = await user.Client.POSTAsync<Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    // SRV007: Using raw HttpClient.DeleteAsync is necessary here to test invalid comment ID format
    // (non-numeric "abc"). FastEndpoints DELETEAsync would require a valid DeleteCommentRequest with int Id,
    // which would not allow testing this edge case.
#pragma warning disable SRV007
    var response = await user.Client.DeleteAsync($"/api/articles/{slug}/comments/abc", TestContext.Current.CancellationToken);
#pragma warning restore SRV007

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
