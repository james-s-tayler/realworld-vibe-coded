using Server.SharedKernel.MediatR;

namespace Server.UseCases.Comments.Delete;

public record DeleteCommentCommand(string Slug, string CommentId, Guid CurrentUserId) : ICommand<bool>;
