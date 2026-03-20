using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Comments;

public static class CommentMappers
{
  public static CommentDto MapToDto(Comment comment, bool isFollowing = false)
  {
    return new CommentDto(
      comment.Id,
      comment.CreatedAt,
      comment.UpdatedAt,
      comment.Body,
      new AuthorDto(
        comment.Author.Username,
        comment.Author.Bio,
        comment.Author.Image,
        isFollowing
      )
    );
  }
}
