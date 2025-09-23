using Ardalis.Result;
using Ardalis.SharedKernel;
using Server.Core.ArticleAggregate.Dtos;

namespace Server.UseCases.Articles.GetBySlug;

public record GetArticleBySlugQuery(string Slug, int? CurrentUserId = null) : IQuery<Result<ArticleDto>>;