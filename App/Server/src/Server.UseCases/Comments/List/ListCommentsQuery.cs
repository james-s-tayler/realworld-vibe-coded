using Server.SharedKernel.MediatR;

namespace Server.UseCases.Comments.List;

public record ListCommentsQuery(string Slug, Guid CurrentUserId) : IQuery<CommentsListResult>;
