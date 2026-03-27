using Server.SharedKernel.MediatR;

namespace Server.UseCases.Comments.Create;

public record CreateCommentCommand(string Slug, string Body, Guid AuthorId) : ICommand<CommentResult>;
