using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.UserAggregate;

namespace Server.UseCases.Articles.Comments;

/// <summary>
/// Static mappers for Comment-related entities to DTOs to reduce duplication across handlers
/// </summary>
public static class CommentMappers
{
  /// <summary>
  /// Maps Comment entity to CommentDto with current user context for following status
  /// </summary>
  public static CommentDto MapToDto(Comment comment, User? currentUser = null)
  {
    var isFollowing = currentUser?.IsFollowing(comment.AuthorId) ?? false;

    return new CommentDto(
      comment.Id,
      comment.CreatedAt,
      comment.UpdatedAt,
      comment.Body,
      new AuthorDto(
        comment.Author.Username,
        comment.Author.Bio ?? string.Empty,
        comment.Author.Image,
        isFollowing
      )
    );
  }
}