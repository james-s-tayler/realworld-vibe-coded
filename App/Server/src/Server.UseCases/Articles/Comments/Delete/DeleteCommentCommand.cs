namespace Server.UseCases.Articles.Comments.Delete;

public record DeleteCommentCommand(string Slug, int CommentId, int UserId) : ICommand<Unit>;
