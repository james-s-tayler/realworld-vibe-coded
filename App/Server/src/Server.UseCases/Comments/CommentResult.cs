using Server.Core.CommentAggregate;

namespace Server.UseCases.Comments;

public record CommentResult(Comment Comment, bool AuthorFollowing);
