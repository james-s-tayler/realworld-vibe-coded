using Server.UseCases.Comments;
using Server.Web.Profiles;

namespace Server.Web.Comments;

public class CommentsMapper : ResponseMapper<CommentsResponse, CommentsListResult>
{
  public override Task<CommentsResponse> FromEntityAsync(CommentsListResult result, CancellationToken ct)
  {
    var response = new CommentsResponse
    {
      Comments = result.Comments.Select(r => new CommentDto
      {
        Id = r.Comment.Id,
        Body = r.Comment.Body,
        CreatedAt = r.Comment.CreatedAt,
        UpdatedAt = r.Comment.UpdatedAt,
        Author = new ProfileDto
        {
          Username = r.Comment.Author.UserName ?? string.Empty,
          Bio = r.Comment.Author.Bio,
          Image = r.Comment.Author.Image,
          Following = r.AuthorFollowing,
        },
      }).ToList(),
    };

    return Task.FromResult(response);
  }
}
