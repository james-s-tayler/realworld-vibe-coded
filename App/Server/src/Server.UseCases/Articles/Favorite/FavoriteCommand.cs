using Server.SharedKernel.MediatR;

namespace Server.UseCases.Articles.Favorite;

public record FavoriteCommand(string Slug, Guid UserId) : ICommand<ArticleResult>;
