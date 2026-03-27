using Server.UseCases.Comments;
using Server.Web.Profiles;

namespace Server.Web.Comments;

public class CommentMapper : ResponseMapper<CommentResponse, CommentResult>
{
  public override Task<CommentResponse> FromEntityAsync(CommentResult result, CancellationToken ct)
  {
    var response = new CommentResponse
    {
      Comment = new CommentDto
      {
        Id = result.Comment.Id,
        Body = result.Comment.Body,
        CreatedAt = result.Comment.CreatedAt,
        UpdatedAt = result.Comment.UpdatedAt,
        Author = new ProfileDto
        {
          Username = result.Comment.Author.UserName ?? string.Empty,
          Bio = result.Comment.Author.Bio,
          Image = result.Comment.Author.Image,
          Following = result.AuthorFollowing,
        },
      },
    };

    return Task.FromResult(response);
  }
}
