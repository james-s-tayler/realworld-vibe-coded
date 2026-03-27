using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Unfavorite;

public record UnfavoriteCommand(string Slug, Guid UserId) : ICommand<ArticleResult>;
