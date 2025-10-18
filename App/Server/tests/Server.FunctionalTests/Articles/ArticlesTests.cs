#pragma warning disable xUnit1051

using System.Net;
using System.Net.Http.Json;
using Server.Core.ArticleAggregate.Dtos;
using Server.UseCases.Articles;
using Server.Web.Articles;
using Server.Web.Articles.Comments;

namespace Server.FunctionalTests.Articles;

[Collection("Articles Integration Tests")]
public class ArticlesTests(ArticlesFixture App) : TestBase<ArticlesFixture>
{
  [Fact]
  public async Task ListArticles_ReturnsArticles()
  {
    var (response, result) = await App.Client.GETAsync<ListArticles, ArticlesResponse>();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.ArticlesCount.ShouldBeGreaterThanOrEqualTo(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentAuthorFilter_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?author=nonexistentauthor999");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentFavoritedFilter_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?favorited=nonexistentuser999");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task ListArticles_WithNonExistentTagFilter_ReturnsEmptyList()
  {
    var response = await App.Client.GetAsync("/api/articles?tag=nonexistenttag999");
    var result = await response.Content.ReadFromJsonAsync<ArticlesResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Articles.ShouldNotBeNull();
    result.Articles.ShouldBeEmpty();
    result.ArticlesCount.ShouldBe(0);
  }

  [Fact]
  public async Task CreateArticle_WithValidData_ReturnsArticle()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test Article",
        Description = "Test Description",
        Body = "Test Body",
        TagList = new List<string> { "test", "article" }
      }
    };

    var (response, result) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Test Article");
    result.Article.Description.ShouldBe("Test Description");
    result.Article.Body.ShouldBe("Test Body");
    result.Article.Slug.ShouldBe("test-article");
    result.Article.TagList.ShouldContain("test");
    result.Article.TagList.ShouldContain("article");
    result.Article.Author.ShouldNotBeNull();
    result.Article.Author.Username.ShouldBe(App.ArticlesUser1Username);
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task CreateArticle_WithoutTags_ReturnsArticle()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Article Without Tags",
        Description = "Description",
        Body = "Body"
      }
    };

    var (response, result) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Article Without Tags");
    result.Article.TagList.ShouldBeEmpty();
  }

  [Fact]
  public async Task CreateArticle_WithMissingRequiredFields_ReturnsValidationError()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData()
    };

    var (response, _) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithEmptyRequiredFields_ReturnsValidationError()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "",
        Description = "",
        Body = ""
      }
    };

    var (response, _) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithInvalidTagContainingComma_ReturnsValidationError()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test",
        TagList = new List<string> { "tag,with,comma" }
      }
    };

    var (response, _) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithEmptyTag_ReturnsValidationError()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test",
        TagList = new List<string> { "" }
      }
    };

    var (response, _) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithDuplicateSlug_ReturnsValidationError()
  {
    var request1 = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Duplicate Title",
        Description = "Description 1",
        Body = "Body 1"
      }
    };

    await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(request1);

    var request2 = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Duplicate Title",
        Description = "Description 2",
        Body = "Body 2"
      }
    };

    var (response, _) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request2);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var request = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Test",
        Description = "Test",
        Body = "Test"
      }
    };

    var (response, _) = await App.Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, object>(request);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task GetArticle_WithAuthentication_ReturnsArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.GetAsync($"/api/articles/{slug}");
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Title.ShouldBe("Get Article Test");
  }

  [Fact]
  public async Task GetArticle_WithoutAuthentication_ReturnsArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Get Article Unauthenticated",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.GetAsync($"/api/articles/{slug}");
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
  }

  [Fact]
  public async Task GetArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.Client.GetAsync("/api/articles/no-such-article");

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task FavoriteArticle_WithAuthentication_ReturnsArticleWithFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Slug.ShouldBe(slug);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_AlreadyFavorited_ReturnsArticleWithFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Already Favorited Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null);

    var response = await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(1);
  }

  [Fact]
  public async Task FavoriteArticle_ByDifferentUser_IncreasesFavoritesCount()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Multiple Favorites Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null);

    var response = await App.ArticlesUser2Client.PostAsync($"/api/articles/{slug}/favorite", null);
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(true);
    result.Article.FavoritesCount.ShouldBe(2);
  }

  [Fact]
  public async Task FavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Favorite Unauthenticated Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.PostAsync($"/api/articles/{slug}/favorite", null);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task FavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.PostAsync("/api/articles/no-such-article/favorite", null);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithAuthentication_ReturnsArticleWithoutFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    await App.ArticlesUser1Client.PostAsync($"/api/articles/{slug}/favorite", null);

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/favorite");
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_NotAlreadyFavorited_ReturnsArticleWithoutFavorite()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Not Favorited Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/favorite");
    var result = await response.Content.ReadFromJsonAsync<ArticleResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.Favorited.ShouldBe(false);
    result.Article.FavoritesCount.ShouldBe(0);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Unfavorite Unauthenticated Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.DeleteAsync($"/api/articles/{slug}/favorite");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UnfavoriteArticle_WithNonExistentSlug_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.DeleteAsync("/api/articles/no-such-article/favorite");

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task CreateComment_WithValidData_ReturnsComment()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Test Article",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "This is a test comment"
      }
    };

    var response = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);
    var result = await response.Content.ReadFromJsonAsync<CommentResponse>();
    result.ShouldNotBeNull();

    response.StatusCode.ShouldBe(HttpStatusCode.Created);
    result.Comment.ShouldNotBeNull();
    result.Comment.Body.ShouldBe("This is a test comment");
    result.Comment.Author.ShouldNotBeNull();
    result.Comment.Author.Username.ShouldBe(App.ArticlesUser1Username);
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
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Test comment"
      }
    };

    var response = await App.Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task CreateComment_WithNonExistentArticle_ReturnsNotFound()
  {
    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Test comment"
      }
    };

    var response = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/no-such-article/comments", createCommentRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task CreateComment_WithMissingRequiredFields_ReturnsValidationError()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Validation Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto()
    };

    var response = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task CreateComment_WithEmptyBody_ReturnsValidationError()
  {
    var createArticleRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Comment Empty Body Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = ""
      }
    };

    var response = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "First comment"
      }
    };

    await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);

    var (response, result) = await App.ArticlesUser1Client.GETAsync<Server.Web.Articles.Comments.Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment"
      }
    };

    await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);

    var (response, result) = await App.Client.GETAsync<Server.Web.Articles.Comments.Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var (response, result) = await App.Client.GETAsync<Server.Web.Articles.Comments.Get, GetCommentsRequest, CommentsResponse>(new GetCommentsRequest { Slug = slug });

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Comments.ShouldNotBeNull();
    result.Comments.ShouldBeEmpty();
  }

  [Fact]
  public async Task GetComments_WithNonExistentArticle_ReturnsNotFound()
  {
    var (response, _) = await App.Client.GETAsync<Server.Web.Articles.Comments.Get, GetCommentsRequest, object>(new GetCommentsRequest { Slug = "no-such-article" });

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment to delete"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>();
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
  }

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>();
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var createCommentRequest = new CreateCommentRequest
    {
      Comment = new CreateCommentDto
      {
        Body = "Comment"
      }
    };

    var createCommentResponse = await App.ArticlesUser1Client.PostAsJsonAsync($"/api/articles/{slug}/comments", createCommentRequest);
    var createCommentResult = await createCommentResponse.Content.ReadFromJsonAsync<CommentResponse>();
    createCommentResult.ShouldNotBeNull();
    var commentId = createCommentResult.Comment.Id;

    var response = await App.ArticlesUser2Client.DeleteAsync($"/api/articles/{slug}/comments/{commentId}");

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task DeleteComment_WithNonExistentArticle_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/no-such-article/comments/1");

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/999999");

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

    var (_, createArticleResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createArticleRequest);
    var slug = createArticleResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}/comments/abc");

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task UpdateArticle_WithValidData_ReturnsUpdatedArticle()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Original Title",
        Description = "Original Description",
        Body = "Original Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated Title",
        Description = "Updated Description",
        Body = "Updated Body"
      }
    };

    var (response, result) = await App.ArticlesUser1Client.PUTAsync<Server.Web.Articles.Update, UpdateArticleRequest, ArticleResponse>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.OK);
    result.Article.ShouldNotBeNull();
    result.Article.Title.ShouldBe("Updated Title");
    result.Article.Description.ShouldBe("Updated Description");
    result.Article.Body.ShouldBe("Updated Body");
    result.Article.Slug.ShouldBe("updated-title");
  }

  [Fact]
  public async Task UpdateArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Unauth Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated"
      }
    };

    var (response, _) = await App.Client.PUTAsync<Server.Web.Articles.Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task UpdateArticle_WithNonExistentArticle_ReturnsNotFound()
  {
    var updateRequest = new UpdateArticleRequest
    {
      Slug = "no-such-article",
      Article = new UpdateArticleData
      {
        Title = "Updated"
      }
    };

    var (response, _) = await App.ArticlesUser1Client.PUTAsync<Server.Web.Articles.Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task UpdateArticle_ByWrongUser_ReturnsForbidden()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Wrong User Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "Updated by wrong user"
      }
    };

    var (response, _) = await App.ArticlesUser2Client.PUTAsync<Server.Web.Articles.Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }

  [Fact]
  public async Task UpdateArticle_WithAllFieldsEmpty_ReturnsValidationError()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Update Empty Fields Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var updateRequest = new UpdateArticleRequest
    {
      Slug = slug,
      Article = new UpdateArticleData
      {
        Title = "",
        Description = "",
        Body = ""
      }
    };

    var (response, _) = await App.ArticlesUser1Client.PUTAsync<Server.Web.Articles.Update, UpdateArticleRequest, object>(updateRequest);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task DeleteArticle_WithAuthentication_ReturnsNoContent()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Article Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser1Client.DeleteAsync($"/api/articles/{slug}");

    response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
  }

  [Fact]
  public async Task DeleteArticle_WithNonExistentArticle_ReturnsNotFound()
  {
    var response = await App.ArticlesUser1Client.DeleteAsync("/api/articles/no-such-article");

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task DeleteArticle_WithoutAuthentication_ReturnsUnauthorized()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Unauth Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.Client.DeleteAsync($"/api/articles/{slug}");

    response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
  }

  [Fact]
  public async Task DeleteArticle_ByWrongUser_ReturnsForbidden()
  {
    var createRequest = new CreateArticleRequest
    {
      Article = new ArticleData
      {
        Title = "Delete Wrong User Test",
        Description = "Description",
        Body = "Body"
      }
    };

    var (_, createResult) = await App.ArticlesUser1Client.POSTAsync<Server.Web.Articles.Create, CreateArticleRequest, ArticleResponse>(createRequest);
    var slug = createResult.Article.Slug;

    var response = await App.ArticlesUser2Client.DeleteAsync($"/api/articles/{slug}");

    response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
  }
}
