using Ardalis.Result;
using Ardalis.SharedKernel;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.Favorite;

public record FavoriteArticleCommand(int UserId, string Slug) : ICommand<Result<ArticleDto>>;
