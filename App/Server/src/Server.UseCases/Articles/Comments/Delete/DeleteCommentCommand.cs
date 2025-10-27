using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Comments.Delete;

public record DeleteCommentCommand(string Slug, Guid CommentId, Guid UserId) : ICommand<Unit>;
